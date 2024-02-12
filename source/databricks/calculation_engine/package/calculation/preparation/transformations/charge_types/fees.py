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

from pyspark.sql.dataframe import DataFrame
import pyspark.sql.functions as f
from package.codelists import ChargeType
from package.constants import Colname


def get_fee_charges(
    charges_df: DataFrame,
    metering_points: DataFrame,
) -> DataFrame:
    charges_df = charges_df.filter(f.col(Colname.charge_type) == ChargeType.FEE.value)

    df = _join_with_metering_points(charges_df, metering_points)

    df = df.select(
        Colname.charge_key,
        Colname.charge_code,
        Colname.charge_type,
        Colname.charge_owner,
        Colname.charge_time,
        Colname.charge_price,
        Colname.metering_point_type,
        Colname.settlement_method,
        Colname.grid_area,
        Colname.energy_supplier_id,
    )

    return df


def _join_with_metering_points(df: DataFrame, metering_points: DataFrame) -> DataFrame:
    df = df.join(
        metering_points,
        [
            df[Colname.metering_point_id] == metering_points[Colname.metering_point_id],
            df[Colname.charge_time] >= metering_points[Colname.from_date],
            df[Colname.charge_time] < metering_points[Colname.to_date],
        ],
        "inner",
    ).select(
        df[Colname.charge_key],
        df[Colname.charge_code],
        df[Colname.charge_type],
        df[Colname.charge_owner],
        df[Colname.charge_tax],
        df[Colname.resolution],
        df[Colname.charge_time],
        df[Colname.charge_price],
        metering_points[Colname.metering_point_type],
        metering_points[Colname.settlement_method],
        metering_points[Colname.grid_area],
        metering_points[Colname.energy_supplier_id],
    )

    return df
