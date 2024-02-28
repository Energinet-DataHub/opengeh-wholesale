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
from pyspark.sql import DataFrame, SparkSession
from pyspark.sql.functions import col, last_day, dayofmonth, count, sum
from pyspark.sql.types import DecimalType
from package.calculation.wholesale.schemas.calculate_daily_subscription_price_schema import (
    calculate_daily_subscription_price_schema,
)
from package.constants import Colname


def calculate_daily_subscription_price(
    spark: SparkSession, subscription_charges: DataFrame
) -> DataFrame:
    # calculate price per day
    charges_per_day = calculate_price_per_day(subscription_charges)

    # get count of charges and total daily charge price
    df = get_count_of_charges_and_total_daily_charge_price(charges_per_day)

    return spark.createDataFrame(df.rdd, calculate_daily_subscription_price_schema)


def calculate_price_per_day(
    charges_per_day_flex_consumption: DataFrame,
) -> DataFrame:
    charges_per_day = charges_per_day_flex_consumption.withColumn(
        Colname.price_per_day,
        (
            col(Colname.charge_price) / dayofmonth(last_day(col(Colname.charge_time)))
        ).cast(DecimalType(14, 8)),
    )
    return charges_per_day


def get_count_of_charges_and_total_daily_charge_price(
    charges_per_day: DataFrame,
) -> DataFrame:
    grouped_charges_per_day = (
        charges_per_day.groupBy(
            Colname.charge_owner,
            Colname.grid_area,
            Colname.energy_supplier_id,
            Colname.charge_time,
        )
        .agg(
            count("*").alias(Colname.charge_count),
            sum(Colname.price_per_day).alias(Colname.total_daily_charge_price),
        )
        .select(
            Colname.charge_owner,
            Colname.grid_area,
            Colname.energy_supplier_id,
            Colname.charge_time,
            Colname.charge_count,
            Colname.total_daily_charge_price,
        )
    )

    df = (
        charges_per_day.select("*")
        .distinct()
        .join(
            grouped_charges_per_day,
            [
                Colname.charge_owner,
                Colname.grid_area,
                Colname.energy_supplier_id,
                Colname.charge_time,
            ],
            "inner",
        )
        .select(
            Colname.charge_key,
            Colname.charge_code,
            Colname.charge_type,
            Colname.charge_owner,
            Colname.charge_price,
            Colname.charge_time,
            Colname.price_per_day,
            Colname.charge_count,
            Colname.total_daily_charge_price,
            Colname.metering_point_type,
            Colname.settlement_method,
            Colname.grid_area,
            Colname.energy_supplier_id,
        )
    )
    return df
