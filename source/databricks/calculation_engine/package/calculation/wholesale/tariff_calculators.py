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


import pyspark.sql.functions as f
from pyspark.sql import DataFrame

from package.calculation.preparation.data_structures.prepared_tariffs import (
    PreparedTariffs,
)
from package.calculation.wholesale.data_structures.wholesale_results import (
    WholesaleResults,
)
from package.codelists import ChargeUnit
from package.constants import Colname


def calculate_tariff_price_per_ga_co_es(
    prepared_tariffs: PreparedTariffs,
) -> WholesaleResults:
    """
    Calculate tariff amount time series.
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

    df = _sum_quantity_and_count_charges(prepared_tariffs)

    df = df.select(
        Colname.energy_supplier_id,
        Colname.grid_area,
        Colname.charge_time,
        Colname.metering_point_type,
        Colname.settlement_method,
        Colname.charge_code,
        Colname.charge_type,
        Colname.charge_owner,
        Colname.charge_tax,
        Colname.resolution,
        Colname.charge_price,
        Colname.total_quantity,
        (f.col(Colname.charge_price) * f.col(Colname.total_quantity)).alias(
            Colname.total_amount
        ),
        f.lit(ChargeUnit.KWH.value).alias(Colname.unit),
        Colname.qualities,
    )

    return WholesaleResults(df)


def _sum_quantity_and_count_charges(prepared_tariffs: PreparedTariffs) -> DataFrame:
    # Group by all columns that actually defines the groups, but also the additional
    # columns that need to be present after aggregation
    agg_df = prepared_tariffs.df.groupBy(
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
        Colname.resolution,
        Colname.charge_price,
    ).agg(
        f.sum(Colname.sum_quantity).alias(Colname.total_quantity),
        f.flatten(f.collect_set(Colname.qualities)).alias(Colname.qualities),
    )

    return agg_df
