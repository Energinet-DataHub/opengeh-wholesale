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
from package.calculation.preparation.prepared_fees import PreparedFees
from package.calculation.preparation.transformations.charge_types.helper import (
    join_charge_master_data_and_charge_price,
)
from package.codelists import ChargeType
from package.constants import Colname


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
from pyspark.sql import Window
from pyspark.sql.dataframe import DataFrame

from package.calculation.preparation.charge_link_metering_point_periods import (
    ChargeLinkMeteringPointPeriods,
)
from package.calculation.preparation.charge_master_data import ChargeMasterData
from package.calculation.preparation.charge_prices import ChargePrices
from package.codelists import ChargeType, WholesaleResultResolution
from package.constants import Colname


def get_fee_charges(
    charge_master_data: ChargeMasterData,
    charge_prices: ChargePrices,
    charge_link_metering_point_periods: ChargeLinkMeteringPointPeriods,
    time_zone: str,
) -> PreparedFees:
    """
    This method does the following:
    - Joins charge_master_data, charge_prices and charge_link_metering_point_periods
    - Filters the result to only include fee charges
    - Explodes the result from monthly to daily resolution
    - Add missing charge prices (None) to the result
    """

    fee_links = charge_link_metering_point_periods.filter_by_charge_type(ChargeType.FEE)
    fee_prices = charge_prices.filter_by_charge_type(ChargeType.FEE)
    fee_master_data = charge_master_data.filter_by_charge_type(ChargeType.FEE)

    fee_master_data_and_prices = _join_with_prices(
        fee_master_data, fee_prices, time_zone
    )

    fee = _join_with_links(fee_master_data_and_prices, fee_links.df)

    fees = fee.withColumn(
        Colname.resolution, f.lit(WholesaleResultResolution.DAY.value)
    )

    return PreparedFees(fees)


def _join_with_prices(
    fee_master_data: ChargeMasterData,
    fee_prices: ChargePrices,
    time_zone: str,
) -> DataFrame:
    """
    Join fee_master_data with fee_prices.
    This method also ensure
    - Missing charge prices will be set to None.
    - The charge price is the last known charge price for the charge key.
    """
    fee_prices = fee_prices.df
    fee_master_data = fee_master_data.df

    fee_master_data_with_charge_time = _add_charge_time(fee_master_data, time_zone)

    w = Window.partitionBy(Colname.charge_key, Colname.from_date).orderBy(
        Colname.charge_time
    )

    master_data_with_prices = (
        fee_master_data_with_charge_time.join(
            fee_prices, [Colname.charge_key, Colname.charge_time], "left"
        )
        .withColumn(
            Colname.charge_price,
            f.last(Colname.charge_price, ignorenulls=True).over(w),
        )
        .select(
            fee_master_data_with_charge_time[Colname.charge_key],
            fee_master_data_with_charge_time[Colname.charge_type],
            fee_master_data_with_charge_time[Colname.charge_owner],
            fee_master_data_with_charge_time[Colname.charge_code],
            fee_master_data_with_charge_time[Colname.from_date],
            fee_master_data_with_charge_time[Colname.to_date],
            fee_master_data_with_charge_time[Colname.resolution],
            fee_master_data_with_charge_time[Colname.charge_tax],
            fee_master_data_with_charge_time[Colname.charge_time],
            Colname.charge_price,
        )
    )

    return master_data_with_prices


def _add_charge_time(fee_master_data: DataFrame, time_zone: str) -> DataFrame:
    """
    Add charge_time column to fee_periods DataFrame.
    The charge_time column is created by exploding fee_periods using from_date and to_date with a resolution of 1 day.
    """

    charge_periods_with_charge_time = fee_master_data.withColumn(
        Colname.charge_time,
        f.explode(
            f.sequence(
                f.from_utc_timestamp(Colname.from_date, time_zone),
                f.from_utc_timestamp(Colname.to_date, time_zone),
                f.expr("interval 1 day"),
            )
        ),
    ).withColumn(
        Colname.charge_time,
        f.to_utc_timestamp(Colname.charge_time, time_zone),
    )

    return charge_periods_with_charge_time


def _join_with_links(
    fee_master_data_and_prices: DataFrame,
    fee_links: DataFrame,
) -> DataFrame:
    fees = fee_master_data_and_prices.join(
        fee_links,
        (
            fee_master_data_and_prices[Colname.charge_key]
            == fee_links[Colname.charge_key]
        )
        & (
            fee_master_data_and_prices[Colname.charge_time]
            >= fee_links[Colname.from_date]
        )
        & (
            fee_master_data_and_prices[Colname.charge_time] < fee_links[Colname.to_date]
        ),
        how="inner",
    ).select(
        fee_master_data_and_prices[Colname.charge_key],
        fee_master_data_and_prices[Colname.charge_type],
        fee_master_data_and_prices[Colname.charge_owner],
        fee_master_data_and_prices[Colname.charge_code],
        fee_master_data_and_prices[Colname.charge_time],
        fee_master_data_and_prices[Colname.charge_price],
        fee_master_data_and_prices[Colname.charge_tax],
        fee_links[Colname.charge_quantity],
        fee_links[Colname.metering_point_type],
        fee_links[Colname.metering_point_id],
        fee_links[Colname.settlement_method],
        fee_links[Colname.grid_area],
        fee_links[Colname.energy_supplier_id],
    )

    return fees
