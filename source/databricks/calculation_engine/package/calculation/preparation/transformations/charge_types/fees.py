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

from package.calculation.preparation.charge_link_metering_point_periods import (
    ChargeLinkMeteringPointPeriods,
)
from package.calculation.preparation.charge_master_data import ChargeMasterData
from package.calculation.preparation.charge_prices import ChargePrices
from package.calculation.preparation.transformations.charge_types.helper import (
    join_charge_master_data_and_charge_price,
)
from package.codelists import ChargeType
from package.constants import Colname


def get_fee_charges(
    charge_master_data: ChargeMasterData,
    charge_prices: ChargePrices,
    charge_link_metering_point_periods: ChargeLinkMeteringPointPeriods,
) -> DataFrame:
    charge_master_data = charge_master_data.df
    charge_prices = charge_prices.df
    charge_period_prices = join_charge_master_data_and_charge_price(
        charge_master_data, charge_prices
    )
    charge_link_metering_point_periods_df = charge_link_metering_point_periods.df
    fees = charge_period_prices.filter(
        f.col(Colname.charge_type) == ChargeType.FEE.value
    )

    fees = fees.join(
        charge_link_metering_point_periods_df,
        (
            fees[Colname.charge_key]
            == charge_link_metering_point_periods_df[Colname.charge_key]
        )
        & (
            fees[Colname.charge_time]
            >= charge_link_metering_point_periods_df[Colname.from_date]
        )
        & (
            fees[Colname.charge_time]
            < charge_link_metering_point_periods_df[Colname.to_date]
        ),
        how="inner",
    ).select(
        fees[Colname.charge_key],
        Colname.charge_code,
        Colname.charge_type,
        Colname.charge_owner,
        Colname.charge_time,
        Colname.charge_price,
        charge_link_metering_point_periods_df[Colname.metering_point_type],
        charge_link_metering_point_periods_df[Colname.settlement_method],
        charge_link_metering_point_periods_df[Colname.grid_area],
        charge_link_metering_point_periods_df[Colname.energy_supplier_id],
    )

    return fees
