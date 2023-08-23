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
from pyspark.sql.functions import col, sum, count
from package.constants import Colname


def calculate_tariff_price_per_ga_co_es(tariffs: DataFrame) -> DataFrame:
    # sum quantity and count charges
    agg_df = sum_quantity_and_count_charges(tariffs)

    # select distinct tariffs
    df = select_distinct_tariffs(tariffs)  # Why is this needed?

    # join with agg_df
    df = join_with_agg_df(df, agg_df)

    return df.select(
        Colname.energy_supplier_id,
        Colname.grid_area,
        Colname.charge_time,
        Colname.metering_point_type,
        Colname.settlement_method,
        Colname.charge_key,
        Colname.charge_id,
        Colname.charge_type,
        Colname.charge_owner,
        Colname.charge_tax,
        Colname.resolution,
        Colname.charge_price,
        Colname.total_quantity,
        Colname.charge_count,
        Colname.total_amount,
    )


def sum_quantity_and_count_charges(tariffs: DataFrame) -> DataFrame:
    agg_df = (
        tariffs.groupBy(
            Colname.grid_area,
            Colname.energy_supplier_id,
            Colname.charge_time,
            Colname.metering_point_type,
            Colname.settlement_method,
            Colname.charge_key,
        )
        .agg(
            sum(Colname.quantity).alias(Colname.total_quantity),
            count(Colname.metering_point_id).alias(Colname.charge_count),
        )
        .select("*")
        .distinct()
    )
    return agg_df


def select_distinct_tariffs(tariffs: DataFrame) -> DataFrame:
    df = tariffs.select(
        tariffs[Colname.charge_key],
        tariffs[Colname.charge_id],
        tariffs[Colname.charge_type],
        tariffs[Colname.charge_owner],
        tariffs[Colname.charge_tax],
        tariffs[Colname.resolution],
        tariffs[Colname.charge_time],
        tariffs[Colname.charge_price],
        tariffs[Colname.energy_supplier_id],
        tariffs[Colname.metering_point_type],
        tariffs[Colname.settlement_method],
        tariffs[Colname.grid_area],
    ).distinct()
    return df


def join_with_agg_df(df: DataFrame, agg_df: DataFrame) -> DataFrame:
    df = (
        df.join(
            agg_df,
            [
                Colname.energy_supplier_id,
                Colname.grid_area,
                Colname.charge_time,
                Colname.metering_point_type,
                Colname.settlement_method,
                Colname.charge_key,
            ],
            "inner",
        )
        .withColumn("total_amount", col(Colname.charge_price) * col(Colname.total_quantity))
        .orderBy(
            [
                Colname.charge_key,
                Colname.grid_area,
                Colname.energy_supplier_id,
                Colname.charge_time,
                Colname.metering_point_type,
                Colname.settlement_method,
            ]
        )
    )
    return df
