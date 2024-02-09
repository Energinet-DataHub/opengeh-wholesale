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
from package.calculation import PreparedDataReader
from package.calculation.CalculationResults import CalculationResultsContainer
from package.calculation.calculator_args import CalculatorArgs
from package.calculation.energy import energy_calculation
from package.calculation.wholesale import wholesale_calculation
from package.constants import Colname
from package.infrastructure import logging_configuration
from package.codelists import (
    ChargeResolution,
    CalculationType,
    MeteringPointType,
)
from pyspark.pandas import DataFrame

@logging_configuration.use_span("calculation")
def calculation_execute(
    args: CalculatorArgs, prepared_data_reader: PreparedDataReader
) -> CalculationResultsContainer:
    results = CalculationResultsContainer()

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

    results.energy_results = energy_calculation.execute(
        args.calculation_type,
        args.calculation_grid_areas,
        metering_point_time_series,
        grid_loss_responsible_df,
    )

    if (
        args.calculation_type == CalculationType.WHOLESALE_FIXING
        or args.calculation_type == CalculationType.FIRST_CORRECTION_SETTLEMENT
        or args.calculation_type == CalculationType.SECOND_CORRECTION_SETTLEMENT
        or args.calculation_type == CalculationType.THIRD_CORRECTION_SETTLEMENT
    ):
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

        results.wholesale_results = wholesale_calculation.execute(
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