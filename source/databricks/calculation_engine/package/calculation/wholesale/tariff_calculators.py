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
import pyspark.sql.functions as f

from package.codelists import ChargeUnit, WholesaleResultResolution
from package.common import assert_schema
from package.constants import Colname
from .schemas.tariffs_schema import tariff_schema


def calculate_tariff_price_per_ga_co_es(tariffs: DataFrame) -> DataFrame:
    """
    Calculate tariff price time series.
    A result is calculated per
    - grid area
    - charge key (charge id, charge type, charge owner)
    - settlement method
    - metering point type (except exchange metering points)
    - energy supplier

    Resolution has already been filtered, so only one resolution is present
    in the tariffs data frame. So responsibility of creating results per
    resolution is managed outside this module.
    """

    assert_schema(tariffs.schema, tariff_schema)

    df = _sum_quantity_and_count_charges(tariffs)

    return df.select(
        Colname.energy_supplier_id,
        Colname.grid_area,
        Colname.charge_time,
        Colname.metering_point_type,
        Colname.settlement_method,
        Colname.charge_key,
        Colname.charge_code,
        Colname.charge_type,
        Colname.charge_owner,
        Colname.charge_tax,
        f.col(Colname.charge_resolution).alias(
            Colname.wholesale_result_resolution
        ),  # For these tariffs the input resolution equals output resolution
        Colname.charge_price,
        Colname.total_quantity,
        Colname.charge_count,
        (f.col(Colname.charge_price) * f.col(Colname.total_quantity)).alias(
            Colname.total_amount
        ),
        f.lit(ChargeUnit.KWH.value).alias(Colname.unit),
        Colname.qualities,
    )


def _sum_quantity_and_count_charges(tariffs: DataFrame) -> DataFrame:
    # Group by all columns that actually defines the groups, but also the additional
    # columns that need to be present after aggregation
    agg_df = tariffs.groupBy(
        Colname.energy_supplier_id,
        Colname.grid_area,
        Colname.charge_time,
        Colname.metering_point_type,
        Colname.settlement_method,
        Colname.charge_key,
        Colname.charge_code,
        Colname.charge_type,
        Colname.charge_owner,
        Colname.charge_tax,
        Colname.charge_resolution,
        Colname.charge_price,
    ).agg(
        f.sum(Colname.sum_quantity).alias(Colname.total_quantity),
        f.count(Colname.metering_point_id).alias(Colname.charge_count),
        f.flatten(f.collect_set(Colname.qualities)).alias(Colname.qualities),
    )
    return agg_df


def sum_within_month(df: DataFrame, period_start_datetime: datetime) -> DataFrame:
    agg_df = (
        df.groupBy(
            Colname.energy_supplier_id,
            Colname.grid_area,
            Colname.charge_key,
            Colname.charge_code,
            Colname.charge_type,
            Colname.charge_owner,
        )
        .agg(
            f.sum(Colname.total_amount).alias(Colname.total_amount),
            f.sum(Colname.total_quantity).alias(Colname.total_quantity),
            f.sum(Colname.charge_price).alias(Colname.charge_price),
            # charge_tax is the same for all tariffs in a given month
            f.first(Colname.charge_tax).alias(Colname.charge_tax),
            # tariff unit is the same for all tariffs in a given month (kWh)
            f.first(Colname.unit).alias(Colname.unit),
            f.flatten(f.collect_set(Colname.qualities)).alias(Colname.qualities),
        )
        .select(
            f.col(Colname.grid_area),
            f.col(Colname.energy_supplier_id),
            f.col(Colname.total_quantity),
            f.col(Colname.unit),
            f.col(Colname.qualities),
            f.lit(period_start_datetime).alias(Colname.charge_time),
            f.lit(WholesaleResultResolution.MONTH.value).alias(
                Colname.wholesale_result_resolution
            ),
            f.lit(None).alias(Colname.metering_point_type),
            f.lit(None).alias(Colname.settlement_method),
            f.col(Colname.charge_price),
            f.col(Colname.total_amount),
            f.col(Colname.charge_tax),
            f.col(Colname.charge_code),
            f.col(Colname.charge_type),
            f.col(Colname.charge_owner),
        )
    )

    return agg_df
