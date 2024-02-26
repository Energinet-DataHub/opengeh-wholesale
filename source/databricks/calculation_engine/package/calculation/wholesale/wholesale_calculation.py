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

import package.calculation.output.wholesale_storage_model_factory as factory
import package.calculation.wholesale.tariff_calculators as tariffs
from package.common import assert_schema
from .schemas.tariffs_schema import tariff_schema
from ..CalculationResults import WholesaleResultsContainer
from ..calculator_args import CalculatorArgs
from ...codelists import AmountType
from ...infrastructure import logging_configuration


@logging_configuration.use_span("calculation.wholesale")
def execute(
    args: CalculatorArgs,
    tariffs_hourly_df: DataFrame,
    tariffs_daily_df: DataFrame,
) -> WholesaleResultsContainer:
    assert_schema(tariffs_hourly_df.schema, tariff_schema)

    results = WholesaleResultsContainer()

    _calculate_tariff_charges(
        args,
        tariffs_hourly_df,
        tariffs_daily_df,
        results,
    )

    return results


def _calculate_tariff_charges(
    args: CalculatorArgs,
    tariffs_hourly_df: DataFrame,
    tariffs_daily_df: DataFrame,
    results: WholesaleResultsContainer,
) -> None:
    hourly_tariff_per_ga_co_es = tariffs.calculate_tariff_price_per_ga_co_es(
        tariffs_hourly_df
    )

    results.hourly_tariff_per_ga_co_es = factory.create(
        args,
        hourly_tariff_per_ga_co_es,
        AmountType.AMOUNT_PER_CHARGE,
    )

    monthly_tariff_from_hourly_per_ga_co_es = tariffs.sum_within_month(
        hourly_tariff_per_ga_co_es, args.calculation_period_start_datetime
    )

    results.monthly_tariff_from_hourly_per_ga_co_es = factory.create(
        args,
        monthly_tariff_from_hourly_per_ga_co_es,
        AmountType.MONTHLY_AMOUNT_PER_CHARGE,
    )

    daily_tariff_per_ga_co_es = tariffs.calculate_tariff_price_per_ga_co_es(
        tariffs_daily_df
    )

    results.daily_tariff_per_ga_co_es = factory.create(
        args,
        daily_tariff_per_ga_co_es,
        AmountType.AMOUNT_PER_CHARGE,
    )

    monthly_tariff_from_daily_per_ga_co_es = tariffs.sum_within_month(
        daily_tariff_per_ga_co_es, args.calculation_period_start_datetime
    )

    results.monthly_tariff_from_daily_per_ga_co_es = factory.create(
        args,
        monthly_tariff_from_daily_per_ga_co_es,
        AmountType.MONTHLY_AMOUNT_PER_CHARGE,
    )
