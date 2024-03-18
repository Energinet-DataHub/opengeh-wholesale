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

from package.codelists import ChargeType, ChargeUnit
from package.constants import Colname


def calculate_total_quantity_and_amount(
    prepared_charge: DataFrame, charge_type: ChargeType
) -> DataFrame:
    qualities_function = _get_qualities_function(charge_type)

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
        qualities_function,
    )

    df = _add_charge_unit(df, charge_type)

    return df


def _get_qualities_function(charge_type: ChargeType):
    if charge_type == ChargeType.TARIFF:
        return f.flatten(f.collect_set(Colname.qualities)).alias(Colname.qualities)
    elif charge_type == ChargeType.FEE or charge_type == ChargeType.SUBSCRIPTION:
        return f.lit(None).cast(ArrayType(StringType())).alias(Colname.qualities)
    else:
        raise ValueError(f"Unknown charge type: {charge_type.value}")


def _add_charge_unit(df: DataFrame, charge_type: ChargeType) -> DataFrame:
    if charge_type == ChargeType.TARIFF:
        charge_unit = ChargeUnit.KWH
    elif charge_type == ChargeType.FEE or charge_type == ChargeType.SUBSCRIPTION:
        charge_unit = ChargeUnit.PIECES
    else:
        raise ValueError(f"Unknown charge type: {charge_type.value}")

    return df.withColumn(Colname.unit, f.lit(charge_unit.value))
