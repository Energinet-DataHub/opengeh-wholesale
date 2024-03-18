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
import pyspark.sql.functions as f
from pyspark.sql.types import ArrayType, StringType

from package.codelists import ChargeType
from package.constants import Colname


def calculate_total_quantity_and_amount(
    prepared_charge: DataFrame,
) -> DataFrame:
    df = prepared_charge.groupBy(
        Colname.charge_key,
        Colname.charge_type,
        Colname.charge_code,
        Colname.charge_owner,
        Colname.grid_area,
        Colname.energy_supplier_id,
        Colname.charge_time,
        Colname.metering_point_type,
        Colname.settlement_method,
        Colname.resolution,
        Colname.charge_tax,
    ).agg(
        f.sum(Colname.charge_quantity).alias(Colname.total_quantity),
        f.sum(
            f.when(
                f.col(Colname.charge_price).isNotNull(),
                f.col(Colname.charge_quantity) * f.col(Colname.charge_price),
            )
        ).alias(Colname.total_amount),
        f.first(Colname.charge_price, ignorenulls=True).alias(Colname.charge_price),
        f.when(
            f.col(Colname.charge_type) == ChargeType.TARIFF.value,
            f.flatten(f.collect_set(Colname.qualities)).alias(Colname.qualities),
        ).otherwise(
            f.lit(None).cast(ArrayType(StringType())).alias(Colname.qualities),
        ),
    )

    return df
