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


from datetime import datetime

from pyspark.sql import DataFrame

import package.calculation.wholesale.tariff_calculators as tariffs
from package.common import assert_schema
from .schemas.tariffs_schema import tariff_schema
from ..CalculationResults import WholesaleResults
from ...infrastructure import logging_configuration


@logging_configuration.use_span("calculation.wholesale")
def execute(
    tariffs_hourly_df: DataFrame,
    tariffs_daily_df: DataFrame,
    period_start_datetime: datetime,
) -> WholesaleResults:
    assert_schema(tariffs_hourly_df.schema, tariff_schema)

    results = WholesaleResults()

    _calculate_tariff_charges(
        tariffs_hourly_df,
        tariffs_daily_df,
        period_start_datetime,
        results,
    )

    return results


def _calculate_tariff_charges(
    tariffs_hourly_df: DataFrame,
    tariffs_daily_df: DataFrame,
    period_start_datetime: datetime,
    results: WholesaleResults,
) -> None:
    results.hourly_tariff_per_ga_co_es = tariffs.calculate_tariff_price_per_ga_co_es(
        tariffs_hourly_df
    )

    results.monthly_tariff_from_hourly_per_ga_co_es = tariffs.sum_within_month(
        results.hourly_tariff_per_ga_co_es, period_start_datetime
    )

    results.daily_tariff_per_ga_co_es = tariffs.calculate_tariff_price_per_ga_co_es(
        tariffs_daily_df
    )

    results.monthly_tariff_from_daily_per_ga_co_es = tariffs.sum_within_month(
        results.daily_tariff_per_ga_co_es, period_start_datetime
    )
