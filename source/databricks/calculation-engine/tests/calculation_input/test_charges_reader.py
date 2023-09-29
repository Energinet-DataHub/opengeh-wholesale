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

from decimal import Decimal
from datetime import datetime
from unittest.mock import patch
from pyspark.sql import SparkSession
from package.calculation_input.charges_reader import read_charges
from package.calculation_input.delta_table_reader import DeltaTableReader
from package.codelists import ChargeType
from package.constants import Colname

DEFAULT_CHARGE_ID = "4000"
DEFAULT_CHARGE_OWNER = "001"
DEFAULT_CHARGE_TYPE = ChargeType.TARIFF.value
DEFAULT_CHARGE_KEY = f"{DEFAULT_CHARGE_ID}-{DEFAULT_CHARGE_TYPE}-{DEFAULT_CHARGE_OWNER}"
DEFAULT_CHARGE_TAX = True
DEFAULT_CHARGE_PRICE = Decimal(1.0)
DEFAULT_RESOLUTION = "P1D"
DEFAULT_FROM_DATE = datetime(2020, 1, 1, 0, 0)
DEFAULT_TO_DATE = datetime(2020, 2, 1, 0, 0)
DEFAULT_OBSERVATION_TIME = datetime(2020, 1, 1, 0, 0)
DEFAULT_METERING_POINT_ID = "123456789012345678901234567"


def _create_charge_master_data_row(
    charge_key: str = DEFAULT_CHARGE_KEY,
    charge_id: str = DEFAULT_CHARGE_ID,
    charge_owner: str = DEFAULT_CHARGE_OWNER,
    charge_type: str = DEFAULT_CHARGE_TYPE,
    charge_tax: bool = DEFAULT_CHARGE_TAX,
    resolution: str = DEFAULT_RESOLUTION,
    from_date: datetime = DEFAULT_FROM_DATE,
    to_date: datetime = DEFAULT_TO_DATE,
) -> dict:
    row = {
        Colname.charge_key: charge_key,
        Colname.charge_id: charge_id,
        Colname.charge_owner: charge_owner,
        Colname.charge_type: charge_type,
        Colname.charge_tax: charge_tax,
        Colname.resolution: resolution,
        Colname.from_date: from_date,
        Colname.to_date: to_date,
    }
    return row


def _create_charge_link_periods_row(
    charge_key: str = DEFAULT_CHARGE_KEY,
    charge_id: str = DEFAULT_CHARGE_ID,
    charge_owner: str = DEFAULT_CHARGE_OWNER,
    charge_type: str = DEFAULT_CHARGE_TYPE,
    from_date: datetime = DEFAULT_FROM_DATE,
    to_date: datetime = DEFAULT_TO_DATE,
    metering_point_id: str = DEFAULT_METERING_POINT_ID,
) -> dict:
    row = {
        Colname.charge_key: charge_key,
        Colname.charge_id: charge_id,
        Colname.charge_owner: charge_owner,
        Colname.charge_type: charge_type,
        Colname.from_date: from_date,
        Colname.to_date: to_date,
        Colname.metering_point_id: metering_point_id,
    }
    return row


def _create_charges_prices_points_row(
    charge_key: str = DEFAULT_CHARGE_KEY,
    charge_id: str = DEFAULT_CHARGE_ID,
    charge_owner: str = DEFAULT_CHARGE_OWNER,
    charge_type: str = DEFAULT_CHARGE_TYPE,
    charge_price: Decimal = DEFAULT_CHARGE_PRICE,
    observation_time: datetime = DEFAULT_OBSERVATION_TIME,
) -> dict:
    row = {
        Colname.charge_key: charge_key,
        Colname.charge_id: charge_id,
        Colname.charge_owner: charge_owner,
        Colname.charge_type: charge_type,
        Colname.charge_price: charge_price,
        Colname.observation_time: observation_time,
    }
    return row


# 1 Test combinations ==, >=, <
# df[Colname.charge_key] == charge_links[Colname.charge_key],
# 1.1 Test two different charge_keys
# 1.2 Test two same charge_keys

# 1.3 Test that charge_time is after from_date --- df[Colname.charge_time] >= charge_links[Colname.from_date],
# 1.4 Test that charge_time is equal to from_date --- df[Colname.charge_time] >= charge_links[Colname.from_date],
# 1.5 Test that charge_time is before from_date --- df[Colname.charge_time] >= charge_links[Colname.from_date],

# 1.6 df[Colname.charge_time] < charge_links[Colname.to_date],


# 2a Test inner join with charge_prices
# 2b Test inner join with charge_links

# 3 Test schema read_changes?


@patch("package.calculation_input.charges_reader.DeltaTableReader")
def test__read_changes_joins_tables_charge_master_charge_link_and_charge_prices__returns_a_joined_data_row(
    calculation_input_reader_mock: DeltaTableReader, spark: SparkSession
) -> None:
    # Arrange
    charge_master_data_rows = [
        _create_charge_master_data_row(),
    ]

    charge_link_periods_rows = [
        _create_charge_link_periods_row(),
    ]

    charge_prices_points_rows = [
        _create_charges_prices_points_row(),
    ]

    calculation_input_reader_mock.read_charge_master_data_periods.return_value = (
        spark.createDataFrame(data=charge_master_data_rows)
    )
    calculation_input_reader_mock.read_charge_links_periods.return_value = (
        spark.createDataFrame(data=charge_link_periods_rows)
    )
    calculation_input_reader_mock.read_charge_price_points.return_value = (
        spark.createDataFrame(data=charge_prices_points_rows)
    )

    # Act
    actual = read_charges(calculation_input_reader_mock)

    # Assert
    assert actual.count() == 1
    actual_row = actual.collect()[0]
    assert actual_row[Colname.charge_key] == DEFAULT_CHARGE_KEY
    assert actual_row[Colname.charge_id] == DEFAULT_CHARGE_ID
    assert actual_row[Colname.charge_type] == DEFAULT_CHARGE_TYPE
    assert actual_row[Colname.charge_owner] == DEFAULT_CHARGE_OWNER
    assert actual_row[Colname.charge_tax] == DEFAULT_CHARGE_TAX
    assert actual_row[Colname.charge_resolution] == DEFAULT_RESOLUTION
    assert actual_row[Colname.charge_price] == DEFAULT_CHARGE_PRICE
    assert actual_row[Colname.from_date] == DEFAULT_FROM_DATE
    assert actual_row[Colname.to_date] == DEFAULT_TO_DATE
    assert actual_row[Colname.observation_time] == DEFAULT_OBSERVATION_TIME
    assert actual_row[Colname.metering_point_id] == DEFAULT_METERING_POINT_ID
