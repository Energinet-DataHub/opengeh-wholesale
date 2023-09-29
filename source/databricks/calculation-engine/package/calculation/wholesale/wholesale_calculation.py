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
import package.calculation.preparation as preparation
import package.calculation.wholesale.tariff_calculators as tariffs
from package.codelists import ChargeResolution, MeteringPointType
from package.constants import Colname
from package.calculation_output.wholesale_calculation_result_writer import (
    WholesaleCalculationResultWriter,
)
from datetime import datetime


def execute(
    wholesale_calculation_result_writer: WholesaleCalculationResultWriter,
    metering_points_periods_df: DataFrame,
    time_series_point_df: DataFrame,
    charges_df: DataFrame,
    period_start_datetime: datetime,
) -> None:
    # Get input data
    metering_points_periods_df = _get_production_and_consumption_metering_points(
        metering_points_periods_df
    )

    # Calculate and write to storage
    _calculate_tariff_charges(
        wholesale_calculation_result_writer,
        metering_points_periods_df,
        time_series_point_df,
        charges_df,
        period_start_datetime,
    )


def _calculate_tariff_charges(
    wholesale_calculation_result_writer: WholesaleCalculationResultWriter,
    metering_points_periods_df: DataFrame,
    time_series_point_df: DataFrame,
    charges_df: DataFrame,
    period_start_datetime: datetime,
) -> None:
    tariffs_hourly = preparation.get_tariff_charges(
        metering_points_periods_df,
        time_series_point_df,
        charges_df,
        ChargeResolution.HOUR,
    )

    hourly_tariff_per_ga_co_es = tariffs.calculate_tariff_price_per_ga_co_es(
        tariffs_hourly
    )
    wholesale_calculation_result_writer.write(hourly_tariff_per_ga_co_es)

    monthly_tariff_per_ga_co_es = tariffs.sum_within_month(
        hourly_tariff_per_ga_co_es, period_start_datetime
    )
    wholesale_calculation_result_writer.write(monthly_tariff_per_ga_co_es)


def _get_production_and_consumption_metering_points(
    metering_points_periods_df: DataFrame,
) -> DataFrame:
    return metering_points_periods_df.filter(
        (F.col(Colname.metering_point_type) == MeteringPointType.CONSUMPTION.value)
        | (F.col(Colname.metering_point_type) == MeteringPointType.PRODUCTION.value)
    )
