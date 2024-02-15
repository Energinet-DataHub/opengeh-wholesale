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
from pyspark.sql.dataframe import DataFrame

from package.codelists import ChargeType
from package.constants import Colname


def get_fee_charges(
    charges_df: DataFrame,
    charge_link_metering_points: DataFrame,
) -> DataFrame:
    fees = charges_df.filter(f.col(Colname.charge_type) == ChargeType.FEE.value)

    fees = fees.join(
        charge_link_metering_points,
        (fees[Colname.charge_key] == charge_link_metering_points[Colname.charge_key])
        & (fees[Colname.charge_time] >= charge_link_metering_points[Colname.from_date])
        & (fees[Colname.charge_time] < charge_link_metering_points[Colname.to_date]),
        how="inner",
    ).select(
        fees[Colname.charge_key],
        Colname.charge_code,
        Colname.charge_type,
        Colname.charge_owner,
        Colname.charge_time,
        Colname.charge_price,
        charge_link_metering_points[Colname.metering_point_type],
        charge_link_metering_points[Colname.settlement_method],
        charge_link_metering_points[Colname.grid_area],
        charge_link_metering_points[Colname.energy_supplier_id],
    )

    return fees
