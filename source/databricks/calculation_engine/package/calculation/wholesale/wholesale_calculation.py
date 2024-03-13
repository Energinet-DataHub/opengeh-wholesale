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
import package.calculation.wholesale.tariff_calculators as tariffs
import package.calculation.wholesale.tariff_calculators as tariff_calculator
import package.calculation.wholesale.subscription_calculators as subscription_calculator
from .sum_within_month import sum_within_month

from ..calculation_results import WholesaleResultsContainer
from ..calculator_args import CalculatorArgs
from package.calculation.preparation.data_structures.prepared_charges import (
    PreparedChargesContainer,
)
from package.calculation.preparation.data_structures.prepared_subscriptions import (
    PreparedSubscriptions,
)
from package.calculation.preparation.data_structures.prepared_tariffs import (
    PreparedTariffs,
)
from ...codelists import AmountType, ChargeType
from ...infrastructure import logging_configuration


@logging_configuration.use_span("calculation.wholesale.execute")
def execute(
    args: CalculatorArgs,
    prepared_charges: PreparedChargesContainer,
) -> WholesaleResultsContainer:
    results = WholesaleResultsContainer()

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


@logging_configuration.use_span("calculate_subscriptions")
def _calculate_subscriptions(
    args: CalculatorArgs,
    prepared_subscriptions: PreparedSubscriptions,
    results: WholesaleResultsContainer,
) -> None:
    subscription_amount_per_charge = subscription_calculator.calculate(
        prepared_subscriptions,
        args.calculation_period_start_datetime,
        args.calculation_period_end_datetime,
        args.time_zone,
    )
    results.subscription_per_ga_co_es = factory.create(
        args, subscription_amount_per_charge, AmountType.AMOUNT_PER_CHARGE
    )

    monthly_subscription_amount_per_charge = sum_within_month(
        subscription_amount_per_charge,
        args.calculation_period_start_datetime,
        ChargeType.SUBSCRIPTION,
    )
    results.monthly_subscription_from_daily_per_ga_co_es = factory.create(
        args,
        monthly_subscription_amount_per_charge,
        AmountType.MONTHLY_AMOUNT_PER_CHARGE,
    )


@logging_configuration.use_span("calculate_tariff_charges")
def _calculate_tariff_charges(
    args: CalculatorArgs,
    prepared_hourly_tariffs: PreparedTariffs,
    prepared_daily_tariffs: PreparedTariffs,
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
        ChargeType.TARIFF,
    )

    results.monthly_tariff_from_hourly_per_ga_co_es = factory.create(
        args,
        monthly_tariff_from_hourly_per_ga_co_es,
        AmountType.MONTHLY_AMOUNT_PER_CHARGE,
    )

    daily_tariff_per_ga_co_es = tariffs.calculate_tariff_price_per_ga_co_es(
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
        ChargeType.TARIFF,
    )

    results.monthly_tariff_from_daily_per_ga_co_es = factory.create(
        args,
        monthly_tariff_from_daily_per_ga_co_es,
        AmountType.MONTHLY_AMOUNT_PER_CHARGE,
    )
