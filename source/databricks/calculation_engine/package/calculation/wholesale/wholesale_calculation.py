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


import package.calculation.output.wholesale_storage_model_factory as factory
import package.calculation.wholesale.fee_calculators as fee_calculator
import package.calculation.wholesale.subscription_calculators as subscription_calculator
import package.calculation.wholesale.tariff_calculators as tariff_calculator
import package.calculation.wholesale.total_monthly_amount_calculator as total_amount_calculator
import package.calculation.preparation.data_structures as d
from .sum_within_month import sum_within_month

from ..calculation_results import WholesaleResultsContainer
from ..calculator_args import CalculatorArgs
from ...codelists import AmountType
from ...infrastructure import logging_configuration


@logging_configuration.use_span("calculation.wholesale.execute")
def execute(
    args: CalculatorArgs,
    prepared_charges: d.PreparedChargesContainer,
) -> WholesaleResultsContainer:
    results = WholesaleResultsContainer()

    _calculate_fees(
        args,
        prepared_charges.fees,
        results,
    )

    _calculate_subscriptions(
        args,
        prepared_charges.subscriptions,
        results,
    )

    _calculate_tariff_charges(
        args,
        prepared_charges.hourly_tariffs,
        prepared_charges.daily_tariffs,
        results,
    )

    return results


@logging_configuration.use_span("calculate_fees")
def _calculate_fees(
    args: CalculatorArgs,
    prepared_fees: d.PreparedFees,
    results: WholesaleResultsContainer,
) -> None:
    fee_per_ga_co_es = fee_calculator.calculate(
        prepared_fees,
    )
    results.fee_per_ga_co_es = factory.create(
        args, fee_per_ga_co_es, AmountType.AMOUNT_PER_CHARGE
    )
    monthly_fee_per_ga_co_es = sum_within_month(
        fee_per_ga_co_es,
        args.calculation_period_start_datetime,
    )
    results.monthly_fee_per_ga_co_es = factory.create(
        args,
        monthly_fee_per_ga_co_es,
        AmountType.MONTHLY_AMOUNT_PER_CHARGE,
    )


@logging_configuration.use_span("calculate_subscriptions")
def _calculate_subscriptions(
    args: CalculatorArgs,
    prepared_subscriptions: d.PreparedSubscriptions,
    results: WholesaleResultsContainer,
) -> None:
    subscription_per_ga_co_es = subscription_calculator.calculate(
        prepared_subscriptions,
        args.calculation_period_start_datetime,
        args.calculation_period_end_datetime,
        args.time_zone,
    )
    results.subscription_per_ga_co_es = factory.create(
        args, subscription_per_ga_co_es, AmountType.AMOUNT_PER_CHARGE
    )

    monthly_subscription_per_ga_co_es = sum_within_month(
        subscription_per_ga_co_es,
        args.calculation_period_start_datetime,
    )
    results.monthly_subscription_per_ga_co_es = factory.create(
        args,
        monthly_subscription_per_ga_co_es,
        AmountType.MONTHLY_AMOUNT_PER_CHARGE,
    )


@logging_configuration.use_span("calculate_tariff_charges")
def _calculate_tariff_charges(
    args: CalculatorArgs,
    prepared_hourly_tariffs: d.PreparedTariffs,
    prepared_daily_tariffs: d.PreparedTariffs,
    results: WholesaleResultsContainer,
) -> None:
    hourly_tariff_per_ga_co_es = tariff_calculator.calculate_tariff_price_per_ga_co_es(
        prepared_hourly_tariffs
    )

    results.hourly_tariff_per_ga_co_es = factory.create(
        args,
        hourly_tariff_per_ga_co_es,
        AmountType.AMOUNT_PER_CHARGE,
    )

    monthly_tariff_from_hourly_per_ga_co_es = sum_within_month(
        hourly_tariff_per_ga_co_es,
        args.calculation_period_start_datetime,
    )

    results.monthly_tariff_from_hourly_per_ga_co_es = factory.create(
        args,
        monthly_tariff_from_hourly_per_ga_co_es,
        AmountType.MONTHLY_AMOUNT_PER_CHARGE,
    )

    daily_tariff_per_ga_co_es = tariff_calculator.calculate_tariff_price_per_ga_co_es(
        prepared_daily_tariffs
    )

    results.daily_tariff_per_ga_co_es = factory.create(
        args,
        daily_tariff_per_ga_co_es,
        AmountType.AMOUNT_PER_CHARGE,
    )

    monthly_tariff_from_daily_per_ga_co_es = sum_within_month(
        daily_tariff_per_ga_co_es,
        args.calculation_period_start_datetime,
    )

    results.monthly_tariff_from_daily_per_ga_co_es = factory.create(
        args,
        monthly_tariff_from_daily_per_ga_co_es,
        AmountType.MONTHLY_AMOUNT_PER_CHARGE,
    )


@logging_configuration.use_span("calculate_total_monthly_amount")
def _calculate_total_monthly_amount(
    results: WholesaleResultsContainer,
) -> None:
    total_monthly_amount = total_amount_calculator.calculate(
        results.monthly_subscription_per_ga_co_es,
        results.monthly_fee_per_ga_co_es,
        results.monthly_tariff_from_hourly_per_ga_co_es,
        results.monthly_tariff_from_daily_per_ga_co_es,
    )
