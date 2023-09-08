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
from pyspark.sql.functions import col, collect_set, count, flatten, lit, sum
from pyspark.sql.types import (
    ArrayType,
    BooleanType,
    DecimalType,
    StructType,
    StructField,
    StringType,
    TimestampType,
)
from package.codelists import ChargeUnit
from package.constants import Colname


tariff_schema = StructType(
    [
        StructField(Colname.charge_key, StringType(), False),
        StructField(Colname.charge_id, StringType(), False),
        StructField(Colname.charge_type, StringType(), False),
        StructField(Colname.charge_owner, StringType(), False),
        StructField(Colname.charge_tax, BooleanType(), False),
        StructField(Colname.charge_resolution, StringType(), False),
        StructField(Colname.charge_time, TimestampType(), False),
        StructField(Colname.charge_price, DecimalType(18, 6), False),
        StructField(Colname.metering_point_id, StringType(), False),
        StructField(Colname.energy_supplier_id, StringType(), False),
        StructField(Colname.metering_point_type, StringType(), False),
        StructField(Colname.settlement_method, StringType(), True),
        StructField(Colname.grid_area, StringType(), False),
        StructField(Colname.quantity, DecimalType(18, 3), False),
        StructField(Colname.qualities, ArrayType(StringType()), False),
    ]
)
"""Schema contract for tariffs"""


def calculate_tariff_price_per_ga_co_es(tariffs: DataFrame) -> DataFrame:
    """
    Calculate tariff price time series.
    A result is calculated per
    - grid area
    - charge key (charge id, charge type, charge owner)
    - settlement method
    - metering point type (except exchange metering points)
    - energy supplier

    Resolution has already been filtered, so only one resolution is present in the tariffs data frame.
    So responsibility of creating results per resolution is managed outside this module.
    """

    df = _sum_quantity_and_count_charges(tariffs)

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
        (col(Colname.charge_price) * col(Colname.total_quantity)).alias(Colname.total_amount),
        lit(ChargeUnit.KWH.value).alias(Colname.unit),
        Colname.qualities,
    )


def _sum_quantity_and_count_charges(tariffs: DataFrame) -> DataFrame:
    # Group by all columns that actually defines the groups, but also the additional columns that need to be present after aggregation
    agg_df = (
        tariffs.groupBy(
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
        )
        .agg(
            sum(Colname.quantity).alias(Colname.total_quantity),
            count(Colname.metering_point_id).alias(Colname.charge_count),
            flatten(collect_set(Colname.qualities)).alias(Colname.qualities),
        )
    )
    return agg_df
