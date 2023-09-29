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
import pyspark.sql.functions as F
from package.calculation.preparation import PreparedDataReader
import package.calculation.wholesale.tariff_calculators as tariffs
from package.codelists import ChargeResolution, MeteringPointType
from package.constants import Colname
from package.calculation_output.wholesale_calculation_result_writer import (
    WholesaleCalculationResultWriter,
)
from datetime import datetime


def execute(
    wholesale_calculation_result_writer: WholesaleCalculationResultWriter,
    tariffs_hourly_df: DataFrame,
    period_start_datetime: datetime,
) -> None:
    # Calculate and write to storage
    _calculate_tariff_charges(
        wholesale_calculation_result_writer,
        tariffs_hourly_df,
        period_start_datetime,
    )


def _calculate_tariff_charges(
    wholesale_calculation_result_writer: WholesaleCalculationResultWriter,
    tariffs_hourly_df: DataFrame,
    period_start_datetime: datetime,
) -> None:
    hourly_tariff_per_ga_co_es = tariffs.calculate_tariff_price_per_ga_co_es(
        tariffs_hourly_df
    )
    wholesale_calculation_result_writer.write(hourly_tariff_per_ga_co_es)

    monthly_tariff_per_ga_co_es = tariffs.sum_within_month(
        hourly_tariff_per_ga_co_es, period_start_datetime
    )
    wholesale_calculation_result_writer.write(monthly_tariff_per_ga_co_es)
