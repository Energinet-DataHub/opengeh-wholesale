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

from package.infrastructure import logging_configuration
from package.calculation_output.basis_data_writer import BasisDataWriter
from package.calculation_output.wholesale_calculation_result_writer import (
    WholesaleCalculationResultWriter,
)
from package.codelists import (
    ProcessType,
    TimeSeriesType,
    AggregationLevel,
    AmountType,
)
from .calculator_args import CalculatorArgs
from .preparation import PreparedDataReader
from package.calculation.calculation_execute import calculation_execute
from ..calculation_output import EnergyCalculationResultWriter


def execute(args: CalculatorArgs, prepared_data_reader: PreparedDataReader) -> None:

    results = calculation_execute(args, prepared_data_reader)

    energy_result_writer = EnergyCalculationResultWriter(
        args.calculation_id,
        args.calculation_process_type,
        args.calculation_execution_time_start,
    )

    # TODO BJM: The following code is a bit repetitive. It could be refactored to a loop or a
    #  function in another PR.
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

    wholesale_calculation_result_writer = WholesaleCalculationResultWriter(
        args.calculation_id,
        args.calculation_process_type,
        args.calculation_execution_time_start,
    )

    if (
        args.calculation_process_type == ProcessType.WHOLESALE_FIXING
        or args.calculation_process_type == ProcessType.FIRST_CORRECTION_SETTLEMENT
        or args.calculation_process_type == ProcessType.SECOND_CORRECTION_SETTLEMENT
        or args.calculation_process_type == ProcessType.THIRD_CORRECTION_SETTLEMENT
    ):
        with logging_configuration.start_span("hourly_tariff_per_ga_co_es"):
            wholesale_calculation_result_writer.write(
                results.wholesale_results.hourly_tariff_per_ga_co_es,
                AmountType.AMOUNT_PER_CHARGE,
            )

        with logging_configuration.start_span("monthly_tariff_per_ga_co_es"):
            wholesale_calculation_result_writer.write(
                results.wholesale_results.monthly_tariff_from_hourly_per_ga_co_es,
                AmountType.MONTHLY_AMOUNT_PER_CHARGE,
            )

        with logging_configuration.start_span("daily_tariff_per_ga_co_es"):
            wholesale_calculation_result_writer.write(
                results.wholesale_results.daily_tariff_per_ga_co_es,
                AmountType.AMOUNT_PER_CHARGE,
            )

        with logging_configuration.start_span("monthly_tariff_per_ga_co_es"):
            wholesale_calculation_result_writer.write(
                results.wholesale_results.monthly_tariff_from_daily_per_ga_co_es,
                AmountType.MONTHLY_AMOUNT_PER_CHARGE,
            )

    # We write basis data at the end of the calculation to make it easier to analyze
    # performance of the calculation part.
    basis_data_writer = BasisDataWriter(
        args.wholesale_container_path, args.calculation_id
    )
    basis_data_writer.write(
        results.basis_data.metering_point_periods,
        results.basis_data.metering_point_time_series,
        args.time_zone,
    )
