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


from pyspark.sql import DataFrame
import package.calculation.wholesale.subscription_calculators as subscriptions
import package.calculation.wholesale.tariff_calculators as tariffs
from .schemas.tariffs_schema import tariff_schema
from package.calculation_output.wholesale_calculation_result_writer import (
    WholesaleCalculationResultWriter,
)
from datetime import datetime

from package.common import assert_schema
from package.codelists.amount_type import AmountType
from ...infrastructure import logging_configuration


@logging_configuration.use_span("calculation.wholesale")
def execute(
    wholesale_calculation_result_writer: WholesaleCalculationResultWriter,
    tariffs_hourly_df: DataFrame,
    tariffs_daily_df: DataFrame,
    subscription_charges_df,
    period_start_datetime: datetime,
) -> None:
    assert_schema(tariffs_hourly_df.schema, tariff_schema)

    # Calculate and write tariff charges to storage
    _calculate_tariff_charges(
        wholesale_calculation_result_writer,
        tariffs_hourly_df,
        tariffs_daily_df,
        period_start_datetime,
    )

    # Calculate and write subscription charges to storage
    _calculate_subscription_charges(
        wholesale_calculation_result_writer, subscription_charges_df
    )


def _calculate_tariff_charges(
    wholesale_calculation_result_writer: WholesaleCalculationResultWriter,
    tariffs_hourly_df: DataFrame,
    tariffs_daily_df: DataFrame,
    period_start_datetime: datetime,
) -> None:
    hourly_tariff_per_ga_co_es = tariffs.calculate_tariff_price_per_ga_co_es(
        tariffs_hourly_df
    )

    with logging_configuration.start_span("hourly_tariff_per_ga_co_es"):
        wholesale_calculation_result_writer.write(
            hourly_tariff_per_ga_co_es, AmountType.AMOUNT_PER_CHARGE
        )

    monthly_tariff_per_ga_co_es = tariffs.sum_within_month(
        hourly_tariff_per_ga_co_es, period_start_datetime
    )
    with logging_configuration.start_span("monthly_tariff_per_ga_co_es"):
        wholesale_calculation_result_writer.write(
            monthly_tariff_per_ga_co_es, AmountType.MONTHLY_AMOUNT_PER_CHARGE
        )

    daily_tariff_per_ga_co_es = tariffs.calculate_tariff_price_per_ga_co_es(
        tariffs_daily_df
    )
    with logging_configuration.start_span("daily_tariff_per_ga_co_es"):
        wholesale_calculation_result_writer.write(
            daily_tariff_per_ga_co_es, AmountType.AMOUNT_PER_CHARGE
        )


def _calculate_subscription_charges(
    wholesale_calculation_result_writer: WholesaleCalculationResultWriter,
    subscription_charges_df: DataFrame,
) -> None:
    with logging_configuration.start_span("subscription_charges"):
        subscription_amount_per_ga_co_es = (
            subscriptions.calculate_daily_subscription_amount(subscription_charges_df)
        )

        wholesale_calculation_result_writer.write(
            subscription_charges_df, AmountType.AMOUNT_PER_CHARGE
        )
