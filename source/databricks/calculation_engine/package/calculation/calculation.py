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
import pyspark.sql.functions as f
from pyspark.sql import DataFrame

from package.infrastructure import logging_configuration
from package.calculation_output.basis_data_writer import BasisDataWriter
from package.calculation_output.wholesale_calculation_result_writer import (
    WholesaleCalculationResultWriter,
)
from package.codelists import ChargeResolution, MeteringPointType, ProcessType
from package.constants import Colname
from .CalculationResults import CalculationResults
from .calculator_args import CalculatorArgs
from .energy import energy_calculation
from .preparation import PreparedDataReader
from .wholesale import wholesale_calculation


def execute(args: CalculatorArgs, prepared_data_reader: PreparedDataReader) -> None:
    results = _execute(args, prepared_data_reader)

    # We write basis data at the end of the calculation to make it easier to analyze performance of the calculation part
    basis_data_writer = BasisDataWriter(
        args.wholesale_container_path, args.calculation_id
    )
    basis_data_writer.write(
        results.basis_data.metering_point_periods,
        results.basis_data.metering_point_time_series,
        args.time_zone,
    )


@logging_configuration.use_span("calculation")
def _execute(
    args: CalculatorArgs, prepared_data_reader: PreparedDataReader
) -> CalculationResults:
    results = CalculationResults()

    with logging_configuration.start_span("calculation.prepare"):
        # cache of metering_point_time_series had no effect on performance (01-12-2023)
        metering_point_periods_df = prepared_data_reader.get_metering_point_periods_df(
            args.calculation_period_start_datetime,
            args.calculation_period_end_datetime,
            args.calculation_grid_areas,
        )
        grid_loss_responsible_df = prepared_data_reader.get_grid_loss_responsible(
            args.calculation_grid_areas, metering_point_periods_df
        )

        metering_point_time_series = (
            prepared_data_reader.get_metering_point_time_series(
                args.calculation_period_start_datetime,
                args.calculation_period_end_datetime,
                metering_point_periods_df,
            ).cache()
        )

    energy_calculation.execute(
        args.calculation_id,
        args.calculation_process_type,
        args.calculation_execution_time_start,
        args.calculation_grid_areas,
        metering_point_time_series,
        grid_loss_responsible_df,
    )

    if (
        args.calculation_process_type == ProcessType.WHOLESALE_FIXING
        or args.calculation_process_type == ProcessType.FIRST_CORRECTION_SETTLEMENT
        or args.calculation_process_type == ProcessType.SECOND_CORRECTION_SETTLEMENT
        or args.calculation_process_type == ProcessType.THIRD_CORRECTION_SETTLEMENT
    ):
        wholesale_calculation_result_writer = WholesaleCalculationResultWriter(
            args.calculation_id,
            args.calculation_process_type,
            args.calculation_execution_time_start,
        )

        charges_df = prepared_data_reader.get_charges(
            args.calculation_period_start_datetime, args.calculation_period_end_datetime
        )
        metering_points_periods_for_wholesale_calculation_df = (
            _get_production_and_consumption_metering_points(metering_point_periods_df)
        )

        tariffs_hourly_df = prepared_data_reader.get_tariff_charges(
            metering_points_periods_for_wholesale_calculation_df,
            metering_point_time_series,
            charges_df,
            ChargeResolution.HOUR,
        )

        tariffs_daily_df = prepared_data_reader.get_tariff_charges(
            metering_points_periods_for_wholesale_calculation_df,
            metering_point_time_series,
            charges_df,
            ChargeResolution.DAY,
        )

        wholesale_calculation.execute(
            wholesale_calculation_result_writer,
            tariffs_hourly_df,
            tariffs_daily_df,
            args.calculation_period_start_datetime,
        )

    # Add basis data results
    results.basis_data.metering_point_periods = metering_point_periods_df
    results.basis_data.metering_point_time_series = metering_point_time_series

    return results


def _get_production_and_consumption_metering_points(
    metering_points_periods_df: DataFrame,
) -> DataFrame:
    return metering_points_periods_df.filter(
        (f.col(Colname.metering_point_type) == MeteringPointType.CONSUMPTION.value)
        | (f.col(Colname.metering_point_type) == MeteringPointType.PRODUCTION.value)
    )
