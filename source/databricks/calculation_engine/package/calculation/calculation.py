# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
from dependency_injector.wiring import inject, Provide
from pyspark.sql import SparkSession, Row

from package.calculation.basis_data.basis_data_results import write_basis_data
from package.calculation.energy.calculated_grid_loss import (
    add_calculated_grid_loss_to_metering_point_times_series,
)
from package.calculation.preparation.transformations.metering_point_periods_for_calculation_type import (
    get_metering_points_periods_for_wholesale_basis_data,
    get_metering_point_periods_for_energy_basis_data,
    get_metering_point_periods_for_wholesale_calculation,
)
from package.container import Container
from package.infrastructure import logging_configuration, paths
from .basis_data import basis_data_factory
from .calculation_results import (
    CalculationResultsContainer,
)
from .calculator_args import CalculatorArgs
from .energy import energy_calculation
from .output.energy_results import write_energy_results
from .output.total_monthly_amounts import write_total_monthly_amounts
from .output.wholesale_results import write_wholesale_results
from .preparation import PreparedDataReader
from .wholesale import wholesale_calculation
from ..codelists.calculation_type import is_wholesale_calculation_type
from ..constants.calculation_column_names import CalculationColumnNames


@logging_configuration.use_span("calculation")
def execute(args: CalculatorArgs, prepared_data_reader: PreparedDataReader) -> None:
    results = _execute(args, prepared_data_reader)
    _write_results(results)
    # IMPORTANT: Write the succeeded calculation after the results to ensure that the calculation
    # is only marked as succeeded when all results are written
    _write_succeeded_calculation(args)


def _execute(
    args: CalculatorArgs, prepared_data_reader: PreparedDataReader
) -> CalculationResultsContainer:
    results = CalculationResultsContainer()

    with logging_configuration.start_span("calculation.prepare"):
        # cache of metering_point_time_series had no effect on performance (01-12-2023)
        all_metering_point_periods = prepared_data_reader.get_metering_point_periods_df(
            args.calculation_period_start_datetime,
            args.calculation_period_end_datetime,
            args.calculation_grid_areas,
        )

        grid_loss_responsible_df = prepared_data_reader.get_grid_loss_responsible(
            args.calculation_grid_areas, all_metering_point_periods
        )

        metering_point_periods_df_without_grid_loss = (
            prepared_data_reader.get_metering_point_periods_without_grid_loss(
                all_metering_point_periods
            )
        )

        metering_point_time_series = (
            prepared_data_reader.get_metering_point_time_series(
                args.calculation_period_start_datetime,
                args.calculation_period_end_datetime,
                metering_point_periods_df_without_grid_loss,
            )
        )
        metering_point_time_series.cache_internal()

    (
        results.energy_results,
        positive_grid_loss,
        negative_grid_loss,
    ) = energy_calculation.execute(
        args,
        metering_point_time_series,
        grid_loss_responsible_df,
    )

    # This extends the content of metering_point_time_series with calculated grid loss,
    # which is used in the wholesale calculation and the basis data
    metering_point_time_series = (
        add_calculated_grid_loss_to_metering_point_times_series(
            metering_point_time_series,
            positive_grid_loss,
            negative_grid_loss,
        )
    )

    if is_wholesale_calculation_type(args.calculation_type):
        with logging_configuration.start_span("calculation.wholesale.prepare"):
            input_charges = prepared_data_reader.get_input_charges(
                args.calculation_period_start_datetime,
                args.calculation_period_end_datetime,
            )

            metering_point_periods_for_basis_data = (
                get_metering_points_periods_for_wholesale_basis_data(
                    all_metering_point_periods
                )
            )

            metering_point_periods_for_wholesale_calculation = (
                get_metering_point_periods_for_wholesale_calculation(
                    metering_point_periods_for_basis_data
                )
            )

            prepared_charges = prepared_data_reader.get_prepared_charges(
                metering_point_periods_for_wholesale_calculation,
                metering_point_time_series,
                input_charges,
                args.time_zone,
            )

        results.wholesale_results, results.total_monthly_amounts = (
            wholesale_calculation.execute(
                args,
                prepared_charges,
            )
        )
    else:
        metering_point_periods_for_basis_data = (
            get_metering_point_periods_for_energy_basis_data(all_metering_point_periods)
        )

    # Add basis data to results
    results.basis_data = basis_data_factory.create(
        args,
        metering_point_periods_for_basis_data,
        metering_point_time_series,
    )

    return results


@logging_configuration.use_span("calculation.write-results")
def _write_results(results: CalculationResultsContainer) -> None:
    write_energy_results(results.energy_results)
    if results.wholesale_results is not None:
        write_wholesale_results(results.wholesale_results)

    if results.total_monthly_amounts is not None:
        write_total_monthly_amounts(results.total_monthly_amounts)

    # We write basis data at the end of the calculation to make it easier to analyze performance of the calculation part
    write_basis_data(results.basis_data)


@logging_configuration.use_span("calculation.write-succeeded-calculation")
@inject
def _write_succeeded_calculation(
    args: CalculatorArgs,
    spark: SparkSession = Provide[Container.spark],
) -> None:
    calculation = {
        CalculationColumnNames.calculation_id: args.calculation_id,
        CalculationColumnNames.calculation_type: args.calculation_type,
        CalculationColumnNames.period_start: args.calculation_period_start_datetime,
        CalculationColumnNames.period_end: args.calculation_period_end_datetime,
        CalculationColumnNames.execution_time_start: args.calculation_execution_time_start,
        CalculationColumnNames.created_by_user_id: args.created_by_user_id,
    }

    calculation_schema = "calculation_id STRING NOT NULL, calculation_type STRING NOT NULL, period_start TIMESTAMP NOT NULL, period_end TIMESTAMP NOT NULL, execution_time_start TIMESTAMP NOT NULL, created_by_user_id STRING NOT NULL"

    df = spark.createDataFrame(data=[Row(**calculation)], schema=calculation_schema)
    df.write.format("delta").mode("append").option("mergeSchema", "false").insertInto(
        f"{paths.BASIS_DATA_DATABASE_NAME}.{paths.CALCULATIONS_TABLE_NAME}"
    )
