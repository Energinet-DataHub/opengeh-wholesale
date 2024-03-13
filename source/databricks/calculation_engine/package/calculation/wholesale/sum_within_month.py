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

import pyspark.sql.functions as f
from pyspark.sql import DataFrame
from pyspark.sql.types import StringType, DecimalType, ArrayType

from package.codelists import WholesaleResultResolution, ChargeType
from package.constants import Colname


def sum_within_month(
    df: DataFrame, period_start_datetime: datetime, charge_type: ChargeType
) -> DataFrame:
    if charge_type.value == ChargeType.TARIFF.value:
        qualities_expr = f.flatten(f.collect_set(Colname.qualities)).alias(
            Colname.qualities
        )
    else:
        qualities_expr = (
            f.lit(None).cast(ArrayType(StringType())).alias(Colname.qualities)
        )

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
            # charge_tax is the same for all tariffs in a given month
            f.first(Colname.charge_tax).alias(Colname.charge_tax),
            # tariff unit is the same for all tariffs in a given month (kWh)
            f.first(Colname.unit).alias(Colname.unit),
            qualities_expr,
        )
        .select(
            f.col(Colname.grid_area),
            f.col(Colname.energy_supplier_id),
            f.col(Colname.total_quantity),
            f.col(Colname.unit),
            f.col(Colname.qualities),
            f.lit(period_start_datetime).alias(Colname.charge_time),
            f.lit(WholesaleResultResolution.MONTH.value).alias(Colname.resolution),
            f.lit(None).cast(StringType()).alias(Colname.metering_point_type),
            f.lit(None).cast(StringType()).alias(Colname.settlement_method),
            f.lit(None).cast(DecimalType(18, 6)).alias(Colname.charge_price),
            f.col(Colname.total_amount),
            f.col(Colname.charge_tax),
            f.col(Colname.charge_code),
            f.col(Colname.charge_type),
            f.col(Colname.charge_owner),
        )
    )

    return agg_df
