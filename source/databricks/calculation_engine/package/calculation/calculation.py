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
from package.codelists import (
    ChargeResolution,
    MeteringPointType,
    ProcessType,
    TimeSeriesType,
    AggregationLevel,
)
from package.constants import Colname
from .CalculationResults import CalculationResultsContainer
from .calculator_args import CalculatorArgs
from .energy import energy_calculation
from .preparation import PreparedDataReader
from .wholesale import wholesale_calculation
from ..calculation_output import EnergyCalculationResultWriter


def execute(args: CalculatorArgs, prepared_data_reader: PreparedDataReader) -> None:
    results = _execute(args, prepared_data_reader)

    energy_result_writer = EnergyCalculationResultWriter(
        args.calculation_id,
        args.calculation_process_type,
        args.calculation_execution_time_start,
    )

    # TODO BJM: The following code is a bit repetitive. It could be refactored to a loop or a function in another Pr
    if results.energy_results.exchange_per_neighbour_ga is not None:
        # Only calculated for aggregations and balance fixings
        with logging_configuration.start_span("net_exchange_per_neighbour_ga"):
            energy_result_writer.write(
                results.energy_results.exchange_per_neighbour_ga,
                TimeSeriesType.NET_EXCHANGE_PER_NEIGHBORING_GA,
                AggregationLevel.TOTAL_GA,
            )

    with logging_configuration.start_span("net_exchange_per_ga"):
        energy_result_writer.write(
            results.energy_results.exchange_per_grid_area,
            TimeSeriesType.NET_EXCHANGE_PER_GA,
            AggregationLevel.TOTAL_GA,
        )

    with logging_configuration.start_span("temporary_production_per_ga"):
        energy_result_writer.write(
            results.energy_results.temporary_production_per_ga,
            TimeSeriesType.TEMP_PRODUCTION,
            AggregationLevel.TOTAL_GA,
        )

    with logging_configuration.start_span("temporary_flex_consumption_per_ga"):
        energy_result_writer.write(
            results.energy_results.temporary_flex_consumption_per_ga,
            TimeSeriesType.TEMP_FLEX_CONSUMPTION,
            AggregationLevel.TOTAL_GA,
        )

    with logging_configuration.start_span("grid_loss"):
        energy_result_writer.write(
            results.energy_results.grid_loss,
            TimeSeriesType.GRID_LOSS,
            AggregationLevel.TOTAL_GA,
        )

    with logging_configuration.start_span("positive_grid_loss"):
        energy_result_writer.write(
            results.energy_results.positive_grid_loss,
            TimeSeriesType.POSITIVE_GRID_LOSS,
            AggregationLevel.TOTAL_GA,
        )

    with logging_configuration.start_span("negative_grid_loss"):
        energy_result_writer.write(
            results.energy_results.negative_grid_loss,
            TimeSeriesType.NEGATIVE_GRID_LOSS,
            AggregationLevel.TOTAL_GA,
        )

    if results.energy_results.consumption_per_ga_and_brp is not None:
        # Only calculated for aggregations and balance fixings
        with logging_configuration.start_span("consumption_per_ga_and_brp"):
            energy_result_writer.write(
                results.energy_results.consumption_per_ga_and_brp,
                TimeSeriesType.NON_PROFILED_CONSUMPTION,
                AggregationLevel.BRP_PER_GA,
            )

    if results.energy_results.consumption_per_ga_and_brp_and_es is not None:
        # Only calculated for aggregations and balance fixings
        with logging_configuration.start_span("consumption_per_ga_and_brp_and_es"):
            energy_result_writer.write(
                results.energy_results.consumption_per_ga_and_brp_and_es,
                TimeSeriesType.NON_PROFILED_CONSUMPTION,
                AggregationLevel.ES_PER_BRP_PER_GA,
            )

    with logging_configuration.start_span("consumption_per_ga_and_es"):
        energy_result_writer.write(
            results.energy_results.consumption_per_ga_and_es,
            TimeSeriesType.NON_PROFILED_CONSUMPTION,
            AggregationLevel.ES_PER_GA,
        )

    with logging_configuration.start_span("consumption_per_ga"):
        energy_result_writer.write(
            results.energy_results.consumption_per_ga,
            TimeSeriesType.NON_PROFILED_CONSUMPTION,
            AggregationLevel.TOTAL_GA,
        )

    if results.energy_results.production_per_ga_and_brp_and_es is not None:
        # Only calculated for aggregations and balance fixings
        with logging_configuration.start_span("production_per_ga_and_brp_and_es"):
            energy_result_writer.write(
                results.energy_results.production_per_ga_and_brp_and_es,
                TimeSeriesType.PRODUCTION,
                AggregationLevel.ES_PER_BRP_PER_GA,
            )

    if results.energy_results.production_per_ga_and_brp is not None:
        # Only calculated for aggregations and balance fixings
        with logging_configuration.start_span("production_per_ga_and_brp"):
            energy_result_writer.write(
                results.energy_results.production_per_ga_and_brp,
                TimeSeriesType.PRODUCTION,
                AggregationLevel.BRP_PER_GA,
            )

    with logging_configuration.start_span("production_per_ga_and_es"):
        energy_result_writer.write(
            results.energy_results.production_per_ga_and_es,
            TimeSeriesType.PRODUCTION,
            AggregationLevel.ES_PER_GA,
        )

    with logging_configuration.start_span("production_per_ga"):
        energy_result_writer.write(
            results.energy_results.production_per_ga,
            TimeSeriesType.PRODUCTION,
            AggregationLevel.TOTAL_GA,
        )

    with logging_configuration.start_span("flex_consumption_per_ga"):
        energy_result_writer.write(
            results.energy_results.flex_consumption_per_ga,
            TimeSeriesType.FLEX_CONSUMPTION,
            AggregationLevel.TOTAL_GA,
        )

    with logging_configuration.start_span("flex_consumption_per_ga_and_es"):
        energy_result_writer.write(
            results.energy_results.flex_consumption_per_ga_and_es,
            TimeSeriesType.FLEX_CONSUMPTION,
            AggregationLevel.ES_PER_GA,
        )

    if results.energy_results.flex_consumption_per_ga_and_brp_and_es is not None:
        # Only calculated for aggregations and balance fixings
        with logging_configuration.start_span("flex_consumption_per_ga_and_brp_and_es"):
            energy_result_writer.write(
                results.energy_results.flex_consumption_per_ga_and_brp_and_es,
                TimeSeriesType.FLEX_CONSUMPTION,
                AggregationLevel.ES_PER_BRP_PER_GA,
            )

    if results.energy_results.flex_consumption_per_ga_and_brp is not None:
        # Only calculated for aggregations and balance fixings
        with logging_configuration.start_span("flex_consumption_per_ga_and_brp"):
            energy_result_writer.write(
                results.energy_results.flex_consumption_per_ga_and_brp,
                TimeSeriesType.FLEX_CONSUMPTION,
                AggregationLevel.BRP_PER_GA,
            )

    with logging_configuration.start_span("total_consumption"):
        energy_result_writer.write(
            results.energy_results.total_consumption,
            TimeSeriesType.TOTAL_CONSUMPTION,
            AggregationLevel.TOTAL_GA,
        )

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
        args.calculation_process_type,
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
