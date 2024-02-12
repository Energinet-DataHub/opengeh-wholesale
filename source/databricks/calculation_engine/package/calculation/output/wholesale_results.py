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
from package.calculation.CalculationResults import WholesaleResultsContainer
from package.calculation.calculator_args import CalculatorArgs
from package.calculation_output import WholesaleCalculationResultWriter
from package.codelists import AmountType
from package.infrastructure import logging_configuration


def write_wholesale_results(
    args: CalculatorArgs, wholesale_results: WholesaleResultsContainer
) -> None:
    wholesale_calculation_result_writer = WholesaleCalculationResultWriter(
        args.calculation_id,
        args.calculation_type,
        args.calculation_execution_time_start,
    )

    with logging_configuration.start_span("hourly_tariff_per_ga_co_es"):
        wholesale_calculation_result_writer.write(
            wholesale_results.hourly_tariff_per_ga_co_es,
            AmountType.AMOUNT_PER_CHARGE,
        )

    with logging_configuration.start_span("monthly_tariff_per_ga_co_es"):
        wholesale_calculation_result_writer.write(
            wholesale_results.monthly_tariff_from_hourly_per_ga_co_es,
            AmountType.MONTHLY_AMOUNT_PER_CHARGE,
        )

    with logging_configuration.start_span("daily_tariff_per_ga_co_es"):
        wholesale_calculation_result_writer.write(
            wholesale_results.daily_tariff_per_ga_co_es,
            AmountType.AMOUNT_PER_CHARGE,
        )

    with logging_configuration.start_span("monthly_tariff_per_ga_co_es"):
        wholesale_calculation_result_writer.write(
            wholesale_results.monthly_tariff_from_daily_per_ga_co_es,
            AmountType.MONTHLY_AMOUNT_PER_CHARGE,
        )
