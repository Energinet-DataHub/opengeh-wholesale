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
from pyspark.sql.functions import col, sum, count, lit
from package.codelists import ChargeQuality, ChargeUnit
from package.constants import Colname


def calculate_tariff_price_per_ga_co_es(tariffs: DataFrame) -> DataFrame:
    """Calculate tariff price per grid area, charge owner and energy supplier."""

    agg_df = _sum_quantity_and_count_charges(tariffs)

    df = _join_with_agg_df(tariffs, agg_df)

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
        Colname.charge_resolution,
        Colname.charge_price,
        Colname.total_quantity,
        Colname.charge_count,
        Colname.total_amount,
        lit(ChargeUnit.KWH.value).alias(Colname.unit),
        lit(ChargeQuality.CALCULATED.value).alias(Colname.quality),  # TODO JMG: Replace with correct value
    )


def _sum_quantity_and_count_charges(tariffs: DataFrame) -> DataFrame:
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
    )
    return agg_df


def _join_with_agg_df(df: DataFrame, agg_df: DataFrame) -> DataFrame:
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
        .withColumn(Colname.total_amount, col(Colname.charge_price) * col(Colname.total_quantity))
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
