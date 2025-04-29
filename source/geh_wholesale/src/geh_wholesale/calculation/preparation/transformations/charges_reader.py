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

from geh_common.pyspark.clamp import clamp_period_end, clamp_period_start
from pyspark.sql import DataFrame
from pyspark.sql.functions import col, concat_ws

from geh_wholesale.calculation.preparation.data_structures.charge_price_information import (
    ChargePriceInformation,
)
from geh_wholesale.calculation.preparation.data_structures.charge_prices import ChargePrices
from geh_wholesale.constants import Colname
from geh_wholesale.databases.migrations_wholesale import MigrationsWholesaleRepository


def read_charge_price_information(
    repository: MigrationsWholesaleRepository,
    period_start_datetime: datetime,
    period_end_datetime: datetime,
) -> ChargePriceInformation:
    charge_price_information_periods = (
        repository.read_charge_price_information_periods()
        .where(col(Colname.from_date) < period_end_datetime)
        .where(col(Colname.to_date).isNull() | (col(Colname.to_date) > period_start_datetime))
    )

    charge_price_information_periods = charge_price_information_periods.withColumn(
        Colname.from_date, clamp_period_start(Colname.from_date, period_start_datetime)
    ).withColumn(Colname.to_date, clamp_period_end(Colname.to_date, period_end_datetime))

    charge_price_information_periods = _add_charge_key_column(charge_price_information_periods)
    return ChargePriceInformation(charge_price_information_periods)


def read_charge_prices(
    repository: MigrationsWholesaleRepository,
    period_start_datetime: datetime,
    period_end_datetime: datetime,
) -> ChargePrices:
    charge_price_points = (
        repository.read_charge_price_points()
        .where(col(Colname.charge_time) >= period_start_datetime)
        .where(col(Colname.charge_time) < period_end_datetime)
    )

    charge_price_points = _add_charge_key_column(charge_price_points)
    return ChargePrices(charge_price_points)


def read_charge_links(
    repository: MigrationsWholesaleRepository,
    period_start_datetime: datetime,
    period_end_datetime: datetime,
) -> DataFrame:
    charge_links_df = (
        repository.read_charge_link_periods()
        .where(col(Colname.from_date) < period_end_datetime)
        .where(col(Colname.to_date).isNull() | (col(Colname.to_date) > period_start_datetime))
    )

    charge_links_df = charge_links_df.withColumn(
        Colname.from_date, clamp_period_start(Colname.from_date, period_start_datetime)
    ).withColumn(Colname.to_date, clamp_period_end(Colname.to_date, period_end_datetime))
    charge_links_df = _add_charge_key_column(charge_links_df)

    return charge_links_df


def _add_charge_key_column(charge_df: DataFrame) -> DataFrame:
    return charge_df.withColumn(
        Colname.charge_key,
        concat_ws(
            "-",
            col(Colname.charge_code),
            col(Colname.charge_owner),
            col(Colname.charge_type),
        ),
    )
