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
from pyspark.sql.functions import col, count, sum
from geh_stream.codelists import Colname, MarketEvaluationPointType, SettlementMethod
from geh_stream.schemas.output import calculate_fee_charge_price_schema


def calculate_fee_charge_price(spark: SparkSession, fee_charges: DataFrame) -> DataFrame:
    # filter on metering point type and settlement method
    charges_flex_settled_consumption = filter_on_metering_point_type_and_settlement_method(fee_charges)

    # get count of charges and total daily charge price
    df = get_count_of_charges_and_total_daily_charge_price(charges_flex_settled_consumption)

    return spark.createDataFrame(df.rdd, calculate_fee_charge_price_schema)


def filter_on_metering_point_type_and_settlement_method(fee_charges: DataFrame) -> DataFrame:
    charges_flex_settled_consumption = fee_charges \
        .filter(col(Colname.metering_point_type) == MarketEvaluationPointType.consumption.value) \
        .filter(col(Colname.settlement_method) == SettlementMethod.flex_settled.value)
    return charges_flex_settled_consumption


def get_count_of_charges_and_total_daily_charge_price(charges_flex_settled_consumption: DataFrame) -> DataFrame:
    grouped_charges = charges_flex_settled_consumption \
        .groupBy(Colname.charge_owner, Colname.grid_area, Colname.energy_supplier_id, Colname.time) \
        .agg(
            count("*").alias(Colname.charge_count),
            sum(Colname.charge_price).alias(Colname.total_daily_charge_price)
            ) \
        .select(
            Colname.charge_owner,
            Colname.grid_area,
            Colname.energy_supplier_id,
            Colname.time,
            Colname.charge_count,
            Colname.total_daily_charge_price
        )

    df = charges_flex_settled_consumption \
        .select("*").distinct().join(grouped_charges, [Colname.charge_owner, Colname.grid_area, Colname.energy_supplier_id, Colname.time], "inner") \
        .select(
            Colname.charge_key,
            Colname.charge_id,
            Colname.charge_type,
            Colname.charge_owner,
            Colname.charge_price,
            Colname.time,
            Colname.charge_count,
            Colname.total_daily_charge_price,
            Colname.metering_point_type,
            Colname.settlement_method,
            Colname.grid_area,
            Colname.connection_state,
            Colname.energy_supplier_id
        )
    return df
