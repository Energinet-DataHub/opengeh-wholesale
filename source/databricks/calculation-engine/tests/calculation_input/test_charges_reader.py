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

from unittest.mock import patch
from pyspark.sql import SparkSession, DataFrame
from package.calculation_input.charges_reader import read_charges

import pytest

from package.calculation_input.delta_table_reader import DeltaTableReader
from package.codelists import ChargeType
from package.constants import Colname

from package.calculation.wholesale.schemas.charges_schema import (
    charges_schema,
    charge_prices_schema,
    charge_links_schema,
)

DEFAULT_CHARGE_ID = "4000"
DEFAULT_CHARGE_OWNER = "001"
DEFAULT_CHARGE_TYPE = ChargeType.TARIFF.value
DEFAULT_CHARGE_KEY = f"{DEFAULT_CHARGE_ID}-{DEFAULT_CHARGE_TYPE}-{DEFAULT_CHARGE_OWNER}"


def _create_charge_master_data_row(
    charge_key: str = DEFAULT_CHARGE_KEY,
    charge_id: str = DEFAULT_CHARGE_ID,
    charge_owner: str = DEFAULT_CHARGE_OWNER,
    charge_type: str = DEFAULT_CHARGE_TYPE,
) -> dict:
    row = {
        Colname.charge_key: charge_key,
        Colname.charge_id: charge_id,
        Colname.charge_owner: charge_owner,
        Colname.charge_type: charge_type,
    }
    return row


def _create_charge_link_periods_row(
    charge_key: str = DEFAULT_CHARGE_KEY,
    charge_id: str = DEFAULT_CHARGE_ID,
    charge_owner: str = DEFAULT_CHARGE_OWNER,
    charge_type: str = DEFAULT_CHARGE_TYPE,
) -> dict:
    row = {
        Colname.charge_key: charge_key,
        Colname.charge_id: charge_id,
        Colname.charge_owner: charge_owner,
        Colname.charge_type: charge_type,
    }
    return row


def _create_charges_prices_points_row(
    charge_key: str = DEFAULT_CHARGE_KEY,
    charge_id: str = DEFAULT_CHARGE_ID,
    charge_owner: str = DEFAULT_CHARGE_OWNER,
    charge_type: str = DEFAULT_CHARGE_TYPE,
) -> dict:
    row = {
        Colname.charge_key: charge_key,
        Colname.charge_id: charge_id,
        Colname.charge_owner: charge_owner,
        Colname.charge_type: charge_type,
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
def test__join(
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
