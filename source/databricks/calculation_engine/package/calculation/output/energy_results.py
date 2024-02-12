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
from package.calculation.CalculationResults import EnergyResultsContainer
from package.calculation.calculator_args import CalculatorArgs
from package.calculation_output import EnergyCalculationResultWriter
from package.codelists import TimeSeriesType, AggregationLevel
from package.infrastructure import logging_configuration


def write_energy_results(
    args: CalculatorArgs, energy_results: EnergyResultsContainer
) -> None:
    energy_result_writer = EnergyCalculationResultWriter(
        args.calculation_id,
        args.calculation_type,
        args.calculation_execution_time_start,
    )

    # TODO BJM: The following code is a bit repetitive. It could be refactored to a loop or a function in another Pr

    if energy_results.exchange_per_neighbour_ga is not None:
        # Only calculated for aggregations and balance fixings
        with logging_configuration.start_span("net_exchange_per_neighbour_ga"):
            energy_result_writer.write(
                energy_results.exchange_per_neighbour_ga,
                TimeSeriesType.NET_EXCHANGE_PER_NEIGHBORING_GA,
                AggregationLevel.TOTAL_GA,
            )

    with logging_configuration.start_span("net_exchange_per_ga"):
        energy_result_writer.write(
            energy_results.exchange_per_grid_area,
            TimeSeriesType.NET_EXCHANGE_PER_GA,
            AggregationLevel.TOTAL_GA,
        )

    with logging_configuration.start_span("temporary_production_per_ga"):
        energy_result_writer.write(
            energy_results.temporary_production_per_ga,
            TimeSeriesType.TEMP_PRODUCTION,
            AggregationLevel.TOTAL_GA,
        )

    with logging_configuration.start_span("temporary_flex_consumption_per_ga"):
        energy_result_writer.write(
            energy_results.temporary_flex_consumption_per_ga,
            TimeSeriesType.TEMP_FLEX_CONSUMPTION,
            AggregationLevel.TOTAL_GA,
        )

    with logging_configuration.start_span("grid_loss"):
        energy_result_writer.write(
            energy_results.grid_loss,
            TimeSeriesType.GRID_LOSS,
            AggregationLevel.TOTAL_GA,
        )

    with logging_configuration.start_span("positive_grid_loss"):
        energy_result_writer.write(
            energy_results.positive_grid_loss,
            TimeSeriesType.POSITIVE_GRID_LOSS,
            AggregationLevel.TOTAL_GA,
        )

    with logging_configuration.start_span("negative_grid_loss"):
        energy_result_writer.write(
            energy_results.negative_grid_loss,
            TimeSeriesType.NEGATIVE_GRID_LOSS,
            AggregationLevel.TOTAL_GA,
        )

    if energy_results.consumption_per_ga_and_brp is not None:
        # Only calculated for aggregations and balance fixings
        with logging_configuration.start_span("consumption_per_ga_and_brp"):
            energy_result_writer.write(
                energy_results.consumption_per_ga_and_brp,
                TimeSeriesType.NON_PROFILED_CONSUMPTION,
                AggregationLevel.BRP_PER_GA,
            )

    if energy_results.consumption_per_ga_and_brp_and_es is not None:
        # Only calculated for aggregations and balance fixings
        with logging_configuration.start_span("consumption_per_ga_and_brp_and_es"):
            energy_result_writer.write(
                energy_results.consumption_per_ga_and_brp_and_es,
                TimeSeriesType.NON_PROFILED_CONSUMPTION,
                AggregationLevel.ES_PER_BRP_PER_GA,
            )

    with logging_configuration.start_span("consumption_per_ga_and_es"):
        energy_result_writer.write(
            energy_results.consumption_per_ga_and_es,
            TimeSeriesType.NON_PROFILED_CONSUMPTION,
            AggregationLevel.ES_PER_GA,
        )

    with logging_configuration.start_span("consumption_per_ga"):
        energy_result_writer.write(
            energy_results.consumption_per_ga,
            TimeSeriesType.NON_PROFILED_CONSUMPTION,
            AggregationLevel.TOTAL_GA,
        )

    if energy_results.production_per_ga_and_brp_and_es is not None:
        # Only calculated for aggregations and balance fixings
        with logging_configuration.start_span("production_per_ga_and_brp_and_es"):
            energy_result_writer.write(
                energy_results.production_per_ga_and_brp_and_es,
                TimeSeriesType.PRODUCTION,
                AggregationLevel.ES_PER_BRP_PER_GA,
            )

    if energy_results.production_per_ga_and_brp is not None:
        # Only calculated for aggregations and balance fixings
        with logging_configuration.start_span("production_per_ga_and_brp"):
            energy_result_writer.write(
                energy_results.production_per_ga_and_brp,
                TimeSeriesType.PRODUCTION,
                AggregationLevel.BRP_PER_GA,
            )

    with logging_configuration.start_span("production_per_ga_and_es"):
        energy_result_writer.write(
            energy_results.production_per_ga_and_es,
            TimeSeriesType.PRODUCTION,
            AggregationLevel.ES_PER_GA,
        )

    with logging_configuration.start_span("production_per_ga"):
        energy_result_writer.write(
            energy_results.production_per_ga,
            TimeSeriesType.PRODUCTION,
            AggregationLevel.TOTAL_GA,
        )

    with logging_configuration.start_span("flex_consumption_per_ga"):
        energy_result_writer.write(
            energy_results.flex_consumption_per_ga,
            TimeSeriesType.FLEX_CONSUMPTION,
            AggregationLevel.TOTAL_GA,
        )

    with logging_configuration.start_span("flex_consumption_per_ga_and_es"):
        energy_result_writer.write(
            energy_results.flex_consumption_per_ga_and_es,
            TimeSeriesType.FLEX_CONSUMPTION,
            AggregationLevel.ES_PER_GA,
        )

    if energy_results.flex_consumption_per_ga_and_brp_and_es is not None:
        # Only calculated for aggregations and balance fixings
        with logging_configuration.start_span("flex_consumption_per_ga_and_brp_and_es"):
            energy_result_writer.write(
                energy_results.flex_consumption_per_ga_and_brp_and_es,
                TimeSeriesType.FLEX_CONSUMPTION,
                AggregationLevel.ES_PER_BRP_PER_GA,
            )

    if energy_results.flex_consumption_per_ga_and_brp is not None:
        # Only calculated for aggregations and balance fixings
        with logging_configuration.start_span("flex_consumption_per_ga_and_brp"):
            energy_result_writer.write(
                energy_results.flex_consumption_per_ga_and_brp,
                TimeSeriesType.FLEX_CONSUMPTION,
                AggregationLevel.BRP_PER_GA,
            )

    with logging_configuration.start_span("total_consumption"):
        energy_result_writer.write(
            energy_results.total_consumption,
            TimeSeriesType.TOTAL_CONSUMPTION,
            AggregationLevel.TOTAL_GA,
        )
