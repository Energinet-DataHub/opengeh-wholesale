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
from typing import Tuple

import package.calculation.output.wholesale_storage_model_factory as wholesale_results_factory
import package.calculation.output.monthly_amounts_storage_model_factory as monthly_amounts_factory
import package.calculation.output.total_monthly_amounts_storage_model_factory as total_monthly_amounts_factory

import package.calculation.wholesale.fee_calculators as fee_calculator
import package.calculation.wholesale.subscription_calculators as subscription_calculator
import package.calculation.wholesale.tariff_calculators as tariff_calculator
import package.calculation.wholesale.total_monthly_amount_calculator as total_monthly_amount_calculator
import package.calculation.preparation.data_structures as d
from .data_structures import MonthlyAmountPerCharge
from .sum_within_month import sum_within_month

from ..calculation_results import (
    WholesaleResultsContainer,
)
from ..calculator_args import CalculatorArgs
from ...codelists import AmountType
from ...infrastructure import logging_configuration


@logging_configuration.use_span("calculation.wholesale.execute")
def execute(
    args: CalculatorArgs,
    prepared_charges: d.PreparedChargesContainer,
) -> WholesaleResultsContainer:
    results = WholesaleResultsContainer()

    monthly_fees = _calculate_fees(
        args,
        prepared_charges.fees,
        results,
    )

    monthly_subscriptions = _calculate_subscriptions(
        args,
        prepared_charges.subscriptions,
        results,
    )

    monthly_hourly_tariffs = _calculate_hourly_tariffs(
        args,
        prepared_charges.hourly_tariffs,
        results,
    )

    monthly_daily_tariffs = _calculate_daily_tariffs(
        args,
        prepared_charges.daily_tariffs,
        results,
    )

    _calculate_total_monthly_amount(
        args,
        monthly_fees,
        monthly_subscriptions,
        monthly_hourly_tariffs,
        monthly_daily_tariffs,
        results,
    )

    return results


@logging_configuration.use_span("calculate_fees")
def _calculate_fees(
    args: CalculatorArgs,
    prepared_fees: d.PreparedFees,
    results: WholesaleResultsContainer,
) -> MonthlyAmountPerCharge:
    fee_per_ga_co_es = fee_calculator.calculate(
        prepared_fees,
    )
    results.fee_per_ga_co_es = wholesale_results_factory.create(
        args, fee_per_ga_co_es, AmountType.AMOUNT_PER_CHARGE
    )
    monthly_fee_per_ga_co_es = sum_within_month(
        fee_per_ga_co_es,
        args.calculation_period_start_datetime,
    )

    # TODO JVM: Change to only monthly_amounts_factory.create when monthly amounts is fully implemented
    results.monthly_fee_per_ga_co_es = wholesale_results_factory.create(
        args,
        monthly_fee_per_ga_co_es,
        AmountType.MONTHLY_AMOUNT_PER_CHARGE,
    )

    monthly_fee_per_ga_co_es_as_monthly_amount = MonthlyAmountPerCharge(
        monthly_fee_per_ga_co_es.df
    )

    results.monthly_fee_per_ga_co_es = monthly_amounts_factory.create(
        args,
        monthly_fee_per_ga_co_es_as_monthly_amount,
    )
    return monthly_fee_per_ga_co_es_as_monthly_amount


@logging_configuration.use_span("calculate_subscriptions")
def _calculate_subscriptions(
    args: CalculatorArgs,
    prepared_subscriptions: d.PreparedSubscriptions,
    results: WholesaleResultsContainer,
) -> MonthlyAmountPerCharge:
    subscription_per_ga_co_es = subscription_calculator.calculate(
        prepared_subscriptions,
        args.calculation_period_start_datetime,
        args.calculation_period_end_datetime,
        args.time_zone,
    )
    results.subscription_per_ga_co_es = wholesale_results_factory.create(
        args, subscription_per_ga_co_es, AmountType.AMOUNT_PER_CHARGE
    )

    monthly_subscription_per_ga_co_es = sum_within_month(
        subscription_per_ga_co_es,
        args.calculation_period_start_datetime,
    )

    # TODO JVM: Change to only monthly_amounts_factory.create when monthly amounts is fully implemented
    results.monthly_subscription_per_ga_co_es = wholesale_results_factory.create(
        args,
        monthly_subscription_per_ga_co_es,
        AmountType.MONTHLY_AMOUNT_PER_CHARGE,
    )

    monthly_subscription_per_ga_co_es_as_monthly_amount = MonthlyAmountPerCharge(
        monthly_subscription_per_ga_co_es.df
    )

    results.monthly_subscription_per_ga_co_es = monthly_amounts_factory.create(
        args,
        monthly_subscription_per_ga_co_es_as_monthly_amount,
    )

    return monthly_subscription_per_ga_co_es_as_monthly_amount


@logging_configuration.use_span("calculate_hourly_tariffs")
def _calculate_hourly_tariffs(
    args: CalculatorArgs,
    prepared_hourly_tariffs: d.PreparedTariffs,
    results: WholesaleResultsContainer,
) -> MonthlyAmountPerCharge:
    hourly_tariff_per_ga_co_es = tariff_calculator.calculate_tariff_price_per_ga_co_es(
        prepared_hourly_tariffs
    )

    results.hourly_tariff_per_ga_co_es = wholesale_results_factory.create(
        args,
        hourly_tariff_per_ga_co_es,
        AmountType.AMOUNT_PER_CHARGE,
    )

    monthly_tariff_from_hourly_per_ga_co_es = sum_within_month(
        hourly_tariff_per_ga_co_es,
        args.calculation_period_start_datetime,
    )

    # TODO JVM: Change to only monthly_amounts_factory.create when monthly amounts is fully implemented
    results.monthly_tariff_from_hourly_per_ga_co_es = wholesale_results_factory.create(
        args,
        monthly_tariff_from_hourly_per_ga_co_es,
        AmountType.MONTHLY_AMOUNT_PER_CHARGE,
    )

    monthly_tariff_from_hourly_per_ga_co_es_as_monthly_amount = MonthlyAmountPerCharge(
        monthly_tariff_from_hourly_per_ga_co_es.df
    )

    results.monthly_tariff_from_hourly_per_ga_co_es = monthly_amounts_factory.create(
        args,
        monthly_tariff_from_hourly_per_ga_co_es_as_monthly_amount,
    )

    return monthly_tariff_from_hourly_per_ga_co_es_as_monthly_amount


@logging_configuration.use_span("calculate_daily_tariffs")
def _calculate_daily_tariffs(
    args: CalculatorArgs,
    prepared_daily_tariffs: d.PreparedTariffs,
    results: WholesaleResultsContainer,
) -> MonthlyAmountPerCharge:

    daily_tariff_per_ga_co_es = tariff_calculator.calculate_tariff_price_per_ga_co_es(
        prepared_daily_tariffs
    )

    results.daily_tariff_per_ga_co_es = wholesale_results_factory.create(
        args,
        daily_tariff_per_ga_co_es,
        AmountType.AMOUNT_PER_CHARGE,
    )

    monthly_tariff_from_daily_per_ga_co_es = sum_within_month(
        daily_tariff_per_ga_co_es,
        args.calculation_period_start_datetime,
    )

    # TODO JVM: Change to only monthly_amounts_factory.create when monthly amounts is fully implemented
    results.monthly_tariff_from_daily_per_ga_co_es = wholesale_results_factory.create(
        args,
        monthly_tariff_from_daily_per_ga_co_es,
        AmountType.MONTHLY_AMOUNT_PER_CHARGE,
    )

    monthly_tariff_from_daily_per_ga_co_es_as_monthly_amount = MonthlyAmountPerCharge(
        monthly_tariff_from_daily_per_ga_co_es.df
    )

    results.monthly_tariff_from_daily_per_ga_co_es = monthly_amounts_factory.create(
        args,
        monthly_tariff_from_daily_per_ga_co_es_as_monthly_amount,
    )

    return monthly_tariff_from_daily_per_ga_co_es_as_monthly_amount


@logging_configuration.use_span("calculate_total_monthly_amount")
def _calculate_total_monthly_amount(
    args: CalculatorArgs,
    monthly_fees: MonthlyAmountPerCharge,
    monthly_subscriptions: MonthlyAmountPerCharge,
    monthly_hourly_tariffs: MonthlyAmountPerCharge,
    monthly_daily_tariffs: MonthlyAmountPerCharge,
    results: WholesaleResultsContainer,
) -> WholesaleResultsContainer:
    all_monthly_amounts = (
        monthly_fees.union(monthly_subscriptions)
        .union(monthly_hourly_tariffs)
        .union(monthly_daily_tariffs)
    )

    total_monthly_amounts_per_ga_co_es = (
        total_monthly_amount_calculator.calculate_per_ga_co_es(
            all_monthly_amounts,
        )
    )

    total_monthly_amounts_per_ga_es = (
        total_monthly_amount_calculator.calculate_per_ga_es(
            total_monthly_amounts_per_ga_co_es,
        )
    )

    results.total_monthly_amounts_per_ga_co_es = total_monthly_amounts_factory.create(
        args, total_monthly_amounts_per_ga_co_es
    )

    results.total_monthly_amounts_per_ga_es = total_monthly_amounts_factory.create(
        args, total_monthly_amounts_per_ga_es
    )

    return results
