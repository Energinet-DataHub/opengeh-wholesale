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
from pyspark import Row
from pyspark.sql import SparkSession, DataFrame

from package.calculation.basis_data.basis_data_results import write_basis_data
from package.calculation.energy.calculated_grid_loss import (
    add_calculated_grid_loss_to_metering_point_times_series,
)
from package.calculation.preparation.transformations.grid_loss_metering_points import (
    get_grid_loss_metering_points,
)
from package.calculation.preparation.transformations.metering_point_periods_for_calculation_type import (
    get_metering_points_periods_for_wholesale_basis_data,
    get_metering_point_periods_for_energy_basis_data,
    get_metering_point_periods_for_wholesale_calculation,
)
from package.infrastructure import logging_configuration
from .basis_data import basis_data_factory
from .basis_data.schemas import calculations_schema
from .calculation_results import (
    CalculationResultsContainer,
)
from .calculator_args import CalculatorArgs
from .energy import energy_calculation
from .output.calculation_writer import write_calculation
from .output.energy_results import write_energy_results
from .output.total_monthly_amounts import write_total_monthly_amounts
from .output.wholesale_results import write_wholesale_results
from .preparation import PreparedDataReader
from .wholesale import wholesale_calculation
from ..codelists.calculation_type import is_wholesale_calculation_type
from ..constants.calculation_column_names import CalculationColumnNames


@logging_configuration.use_span("calculation")
def execute(
    args: CalculatorArgs,
    prepared_data_reader: PreparedDataReader,
    spark: SparkSession,
) -> None:
    results = _execute(args, prepared_data_reader, spark)
    _write_output(results, args, prepared_data_reader, spark)


def _execute(
    args: CalculatorArgs, prepared_data_reader: PreparedDataReader, spark: SparkSession
) -> CalculationResultsContainer:
    results = CalculationResultsContainer()

    with logging_configuration.start_span("calculation.prepare"):
        calculations = _create_calculation(args, prepared_data_reader, spark)

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

        grid_loss_metering_points_df = get_grid_loss_metering_points(
            grid_loss_responsible_df
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

        input_charges = None

    # Add basis data to results
    results.basis_data = basis_data_factory.create(
        args,
        calculations,
        metering_point_periods_for_basis_data,
        metering_point_time_series,
        input_charges,
        grid_loss_metering_points_df,
    )

    return results


@logging_configuration.use_span("calculation.write")
def _write_output(
    results: CalculationResultsContainer,
    args: CalculatorArgs,
    prepared_data_reader: PreparedDataReader,
    spark: SparkSession,
) -> None:
    write_energy_results(results.energy_results)
    if results.wholesale_results is not None:
        write_wholesale_results(results.wholesale_results)

    if results.total_monthly_amounts is not None:
        write_total_monthly_amounts(results.total_monthly_amounts)

    # We write basis data at the end of the calculation to make it easier to analyze performance of the calculation part
    write_basis_data(results.basis_data)

    # IMPORTANT: Write the succeeded calculation after the results to ensure that the calculation
    # is only marked as succeeded when all results are written
    write_calculation(args, prepared_data_reader, spark)


# TODO BJM: Create and use Calculations typed data frame?
def _create_calculation(
    args: CalculatorArgs,
    prepared_data_reader: PreparedDataReader,
    spark: SparkSession,
) -> DataFrame:
    latest_version = prepared_data_reader.get_latest_calculation_version(
        args.calculation_type
    )

    # Next version begins with 1 and increments by 1
    next_version = (latest_version or 0) + 1

    # TODO BJM: Use factory?
    calculation = {
        CalculationColumnNames.calculation_id: args.calculation_id,
        CalculationColumnNames.calculation_type: args.calculation_type.value,
        CalculationColumnNames.period_start: args.calculation_period_start_datetime,
        CalculationColumnNames.period_end: args.calculation_period_end_datetime,
        CalculationColumnNames.execution_time_start: args.calculation_execution_time_start,
        CalculationColumnNames.created_by_user_id: args.created_by_user_id,
        CalculationColumnNames.version: next_version,
    }

    return spark.createDataFrame(data=[Row(**calculation)], schema=calculations_schema)
