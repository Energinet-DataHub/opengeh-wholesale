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
from pyspark.sql.functions import col, sum, count
from geh_stream.codelists import Colname, ChargeType
from geh_stream.schemas.output import calculate_tariff_price_per_ga_co_es_schema
from pyspark.sql.types import DecimalType

total_quantity = "total_quantity"
charge_count = "charge_count"


def calculate_tariff_price_per_ga_co_es(spark: SparkSession, tariffs: DataFrame) -> DataFrame:
    # sum quantity and count charges
    agg_df = sum_quantity_and_count_charges(tariffs)

    # select distinct tariffs
    df = select_distinct_tariffs(tariffs)

    # join with agg_df
    df = join_with_agg_df(df, agg_df)

    return spark.createDataFrame(df.rdd, calculate_tariff_price_per_ga_co_es_schema)


def sum_quantity_and_count_charges(tariffs: DataFrame) -> DataFrame:
    agg_df = tariffs \
        .groupBy(
            Colname.grid_area,
            Colname.energy_supplier_id,
            Colname.time,
            Colname.metering_point_type,
            Colname.settlement_method,
            Colname.charge_key
        ) \
        .agg(
             sum(Colname.quantity).alias(total_quantity),
             count(Colname.metering_point_id).alias(charge_count)
        ).select("*").distinct()
    return agg_df


def select_distinct_tariffs(tariffs: DataFrame) -> DataFrame:
    df = tariffs.select(
            tariffs[Colname.charge_key],
            tariffs[Colname.charge_id],
            tariffs[Colname.charge_type],
            tariffs[Colname.charge_owner],
            tariffs[Colname.charge_tax],
            tariffs[Colname.resolution],
            tariffs[Colname.time],
            tariffs[Colname.charge_price],
            tariffs[Colname.energy_supplier_id],
            tariffs[Colname.metering_point_type],
            tariffs[Colname.settlement_method],
            tariffs[Colname.grid_area]
         ).distinct()
    return df


def join_with_agg_df(df: DataFrame, agg_df: DataFrame) -> DataFrame:
    df = df.join(agg_df, [
        Colname.energy_supplier_id,
        Colname.grid_area,
        Colname.time,
        Colname.metering_point_type,
        Colname.settlement_method,
        Colname.charge_key
    ], "inner") \
    .withColumn("total_amount", col(Colname.charge_price) * col(total_quantity)) \
    .orderBy([Colname.charge_key, Colname.grid_area, Colname.energy_supplier_id, Colname.time, Colname.metering_point_type, Colname.settlement_method])
    return df
