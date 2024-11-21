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

from telemetry_logging import use_span

import package.calculation.preparation.data_structures as d
import package.calculation.wholesale.fee_calculators as fee_calculator
import package.calculation.wholesale.subscription_calculators as subscription_calculator
import package.calculation.wholesale.tariff_calculators as tariff_calculator
import package.calculation.wholesale.total_monthly_amount_calculator as total_monthly_amount_calculator
import package.databases.wholesale_results_internal.amounts_per_charge_storage_model_factory as amounts_per_charge_factory
import package.databases.wholesale_results_internal.monthly_amounts_per_charge_storage_model_factory as monthly_amounts_per_charge_factory
import package.databases.wholesale_results_internal.total_monthly_amounts_storage_model_factory as total_monthly_amounts_factory
from .data_structures import MonthlyAmountPerCharge
from .sum_within_month import sum_within_month
from ..calculation_output import WholesaleResultsOutput
from ..calculator_args import CalculatorArgs
from ...codelists import AmountType


@use_span("calculation.wholesale.execute")
def execute(
    args: CalculatorArgs,
    prepared_charges: d.PreparedChargesContainer,
) -> WholesaleResultsOutput:
    wholesale_results_output = WholesaleResultsOutput()

    monthly_fees = _calculate_fees(
        args,
        prepared_charges.fees,
        wholesale_results_output,
    )

    monthly_subscriptions = _calculate_subscriptions(
        args,
        prepared_charges.subscriptions,
        wholesale_results_output,
    )

    monthly_hourly_tariffs = _calculate_hourly_tariffs(
        args,
        prepared_charges.hourly_tariffs,
        wholesale_results_output,
    )

    monthly_daily_tariffs = _calculate_daily_tariffs(
        args,
        prepared_charges.daily_tariffs,
        wholesale_results_output,
    )

    _calculate_total_monthly_amount(
        args,
        monthly_fees,
        monthly_subscriptions,
        monthly_hourly_tariffs,
        monthly_daily_tariffs,
        wholesale_results_output,
    )

    return wholesale_results_output


@use_span("calculate_fees")
def _calculate_fees(
    args: CalculatorArgs,
    prepared_fees: d.PreparedFees,
    wholesale_results_output: WholesaleResultsOutput,
) -> MonthlyAmountPerCharge:
    fee_per_co_es = fee_calculator.calculate(
        prepared_fees,
    )
    wholesale_results_output.fee_per_co_es = amounts_per_charge_factory.create(
        args, fee_per_co_es, AmountType.AMOUNT_PER_CHARGE
    )
    monthly_fee_per_co_es = sum_within_month(
        fee_per_co_es,
        args.calculation_period_start_datetime,
    )

    wholesale_results_output.monthly_fee_per_co_es = (
        monthly_amounts_per_charge_factory.create(
            args,
            monthly_fee_per_co_es,
        )
    )
    return monthly_fee_per_co_es


@use_span("calculate_subscriptions")
def _calculate_subscriptions(
    args: CalculatorArgs,
    prepared_subscriptions: d.PreparedSubscriptions,
    wholesale_results_output: WholesaleResultsOutput,
) -> MonthlyAmountPerCharge:
    subscription_per_co_es = subscription_calculator.calculate(
        prepared_subscriptions,
        args.calculation_period_start_datetime,
        args.calculation_period_end_datetime,
        args.time_zone,
    )
    wholesale_results_output.subscription_per_co_es = amounts_per_charge_factory.create(
        args, subscription_per_co_es, AmountType.AMOUNT_PER_CHARGE
    )

    monthly_subscription_per_co_es = sum_within_month(
        subscription_per_co_es,
        args.calculation_period_start_datetime,
    )

    wholesale_results_output.monthly_subscription_per_co_es = (
        monthly_amounts_per_charge_factory.create(
            args,
            monthly_subscription_per_co_es,
        )
    )

    return monthly_subscription_per_co_es


@use_span("calculate_hourly_tariffs")
def _calculate_hourly_tariffs(
    args: CalculatorArgs,
    prepared_hourly_tariffs: d.PreparedTariffs,
    wholesale_results_output: WholesaleResultsOutput,
) -> MonthlyAmountPerCharge:
    hourly_tariff_per_co_es = tariff_calculator.calculate_tariff_price_per_co_es(
        prepared_hourly_tariffs
    )
    hourly_tariff_per_co_es.cache_internal()

    wholesale_results_output.hourly_tariff_per_co_es = (
        amounts_per_charge_factory.create(
            args,
            hourly_tariff_per_co_es,
            AmountType.AMOUNT_PER_CHARGE,
        )
    )

    monthly_tariff_from_hourly_per_co_es = sum_within_month(
        hourly_tariff_per_co_es,
        args.calculation_period_start_datetime,
    )

    wholesale_results_output.monthly_tariff_from_hourly_per_co_es = (
        monthly_amounts_per_charge_factory.create(
            args,
            monthly_tariff_from_hourly_per_co_es,
        )
    )

    return monthly_tariff_from_hourly_per_co_es


@use_span("calculate_daily_tariffs")
def _calculate_daily_tariffs(
    args: CalculatorArgs,
    prepared_daily_tariffs: d.PreparedTariffs,
    wholesale_results_output: WholesaleResultsOutput,
) -> MonthlyAmountPerCharge:

    daily_tariff_per_co_es = tariff_calculator.calculate_tariff_price_per_co_es(
        prepared_daily_tariffs
    )

    wholesale_results_output.daily_tariff_per_co_es = amounts_per_charge_factory.create(
        args,
        daily_tariff_per_co_es,
        AmountType.AMOUNT_PER_CHARGE,
    )

    monthly_tariff_from_daily_per_co_es = sum_within_month(
        daily_tariff_per_co_es,
        args.calculation_period_start_datetime,
    )

    wholesale_results_output.monthly_tariff_from_daily_per_co_es = (
        monthly_amounts_per_charge_factory.create(
            args,
            monthly_tariff_from_daily_per_co_es,
        )
    )

    return monthly_tariff_from_daily_per_co_es


@use_span("calculate_total_monthly_amount")
def _calculate_total_monthly_amount(
    args: CalculatorArgs,
    monthly_fees: MonthlyAmountPerCharge,
    monthly_subscriptions: MonthlyAmountPerCharge,
    monthly_hourly_tariffs: MonthlyAmountPerCharge,
    monthly_daily_tariffs: MonthlyAmountPerCharge,
    results: WholesaleResultsOutput,
) -> WholesaleResultsOutput:
    all_monthly_amounts = (
        monthly_fees.union(monthly_subscriptions)
        .union(monthly_hourly_tariffs)
        .union(monthly_daily_tariffs)
    )

    total_monthly_amounts_per_co_es = (
        total_monthly_amount_calculator.calculate_per_co_es(
            all_monthly_amounts,
        )
    )

    total_monthly_amounts_per_es = total_monthly_amount_calculator.calculate_per_es(
        all_monthly_amounts,
    )

    results.total_monthly_amounts_per_co_es = total_monthly_amounts_factory.create(
        args, total_monthly_amounts_per_co_es
    )

    results.total_monthly_amounts_per_es = total_monthly_amounts_factory.create(
        args, total_monthly_amounts_per_es
    )

    return results
