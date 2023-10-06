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

import pytest
from pyspark import Row
from pyspark.sql import SparkSession

from package import calculation_input
from package.calculation.preparation.transformations import read_charges

from package.calculation_input.table_reader import TableReader
from package.codelists import ChargeType
from package.constants import Colname

DEFAULT_CHARGE_CODE = "4000"
DEFAULT_CHARGE_OWNER = "001"
DEFAULT_CHARGE_TYPE = ChargeType.TARIFF.value
DEFAULT_CHARGE_KEY = (
    f"{DEFAULT_CHARGE_CODE}-{DEFAULT_CHARGE_TYPE}-{DEFAULT_CHARGE_OWNER}"
)
DEFAULT_CHARGE_TAX = True
DEFAULT_CHARGE_PRICE = Decimal(1.0)
DEFAULT_RESOLUTION = "P1D"
DEFAULT_FROM_DATE = datetime(2020, 1, 1, 0, 0)
DEFAULT_TO_DATE = datetime(2020, 2, 1, 0, 0)
DEFAULT_OBSERVATION_TIME = datetime(2020, 1, 1, 0, 0)
DEFAULT_METERING_POINT_ID = "123456789012345678901234567"


def _create_charge_master_data_row(
    charge_key: str = DEFAULT_CHARGE_KEY,
    charge_code: str = DEFAULT_CHARGE_CODE,
    charge_owner: str = DEFAULT_CHARGE_OWNER,
    charge_type: str = DEFAULT_CHARGE_TYPE,
    charge_tax: bool = DEFAULT_CHARGE_TAX,
    resolution: str = DEFAULT_RESOLUTION,
    from_date: datetime = DEFAULT_FROM_DATE,
    to_date: datetime = DEFAULT_TO_DATE,
) -> Row:
    row = {
        Colname.charge_key: charge_key,
        Colname.charge_code: charge_code,
        Colname.charge_owner: charge_owner,
        Colname.charge_type: charge_type,
        Colname.charge_tax: charge_tax,
        Colname.resolution: resolution,
        Colname.from_date: from_date,
        Colname.to_date: to_date,
    }
    return Row(**row)


def _create_charge_link_periods_row(
    charge_key: str = DEFAULT_CHARGE_KEY,
    charge_code: str = DEFAULT_CHARGE_CODE,
    charge_owner: str = DEFAULT_CHARGE_OWNER,
    charge_type: str = DEFAULT_CHARGE_TYPE,
    from_date: datetime = DEFAULT_FROM_DATE,
    to_date: datetime = DEFAULT_TO_DATE,
    metering_point_id: str = DEFAULT_METERING_POINT_ID,
) -> Row:
    row = {
        Colname.charge_key: charge_key,
        Colname.charge_code: charge_code,
        Colname.charge_owner: charge_owner,
        Colname.charge_type: charge_type,
        Colname.from_date: from_date,
        Colname.to_date: to_date,
        Colname.metering_point_id: metering_point_id,
    }
    return Row(**row)


def _create_charges_price_points_row(
    charge_key: str = DEFAULT_CHARGE_KEY,
    charge_code: str = DEFAULT_CHARGE_CODE,
    charge_owner: str = DEFAULT_CHARGE_OWNER,
    charge_type: str = DEFAULT_CHARGE_TYPE,
    charge_price: Decimal = DEFAULT_CHARGE_PRICE,
    observation_time: datetime = DEFAULT_OBSERVATION_TIME,
) -> Row:
    row = {
        Colname.charge_key: charge_key,
        Colname.charge_code: charge_code,
        Colname.charge_owner: charge_owner,
        Colname.charge_type: charge_type,
        Colname.charge_price: charge_price,
        Colname.observation_time: observation_time,
    }
    return Row(**row)


@patch.object(calculation_input, TableReader.__name__)
def test__read_changes__returns_expected_joined_row_values(
    table_reader_mock: TableReader, spark: SparkSession
) -> None:
    # Arrange
    table_reader_mock.read_charge_master_data_periods.return_value = (
        spark.createDataFrame(data=[_create_charge_master_data_row()])
    )
    table_reader_mock.read_charge_links_periods.return_value = spark.createDataFrame(
        data=[_create_charge_link_periods_row()]
    )
    table_reader_mock.read_charge_price_points.return_value = spark.createDataFrame(
        data=[_create_charges_price_points_row()]
    )

    # Act
    actual = read_charges(table_reader_mock)

    # Assert
    assert actual.count() == 1
    actual_row = actual.collect()[0]
    assert actual_row[Colname.charge_key] == DEFAULT_CHARGE_KEY
    assert actual_row[Colname.charge_code] == DEFAULT_CHARGE_CODE
    assert actual_row[Colname.charge_type] == DEFAULT_CHARGE_TYPE
    assert actual_row[Colname.charge_owner] == DEFAULT_CHARGE_OWNER
    assert actual_row[Colname.charge_tax] == DEFAULT_CHARGE_TAX
    assert actual_row[Colname.charge_resolution] == DEFAULT_RESOLUTION
    assert actual_row[Colname.charge_price] == DEFAULT_CHARGE_PRICE
    assert actual_row[Colname.from_date] == DEFAULT_FROM_DATE
    assert actual_row[Colname.to_date] == DEFAULT_TO_DATE
    assert actual_row[Colname.observation_time] == DEFAULT_OBSERVATION_TIME
    assert actual_row[Colname.metering_point_id] == DEFAULT_METERING_POINT_ID


@patch.object(calculation_input, TableReader.__name__)
def test__read_changes__when_multiple_charge_keys__returns_only_rows_with_matching_values_from_tables(
    table_reader_mock: TableReader, spark: SparkSession
) -> None:
    # Arrange
    table_reader_mock.read_charge_master_data_periods.return_value = (
        spark.createDataFrame(
            data=[
                _create_charge_master_data_row(),
                _create_charge_master_data_row(charge_key="4001-tariff-5790001330552"),
            ]
        )
    )
    table_reader_mock.read_charge_links_periods.return_value = spark.createDataFrame(
        data=[_create_charge_link_periods_row()]
    )
    table_reader_mock.read_charge_price_points.return_value = spark.createDataFrame(
        data=[_create_charges_price_points_row()]
    )

    # Act
    actual = read_charges(table_reader_mock)

    # Assert
    assert actual.count() == 1
    actual_row = actual.collect()[0]
    assert actual_row[Colname.charge_key] == DEFAULT_CHARGE_KEY


@pytest.mark.parametrize(
    "from_date, to_date, charge_time, expect_empty",
    [
        (  # Dataset: Charge time is the same as from-date and before to-date -> expect rows
            datetime(2020, 1, 1, 0, 0),
            datetime(2020, 1, 2, 0, 0),
            datetime(2020, 1, 1, 0, 0),
            False,
        ),
        (  # Dataset: Charge time is after from-date and before to-date -> expect rows
            datetime(2020, 1, 1, 0, 0),
            datetime(2020, 1, 2, 0, 0),
            datetime(2020, 1, 1, 1, 0),
            False,
        ),
        (  # Dataset: Charge time is AFTER to-date -> expect no rows
            datetime(2020, 1, 1, 0, 0),
            datetime(2020, 1, 2, 0, 0),
            datetime(2020, 1, 3, 0, 0),
            True,
        ),
        (  # Dataset: Charge time is BEFORE from-date -> expect on rows
            datetime(2020, 1, 2, 0, 0),
            datetime(2020, 1, 3, 0, 0),
            datetime(2020, 1, 1, 0, 0),
            True,
        ),
    ],
)
@patch.object(calculation_input, TableReader.__name__)
def test__read_changes__when_multiple_charge_keys__returns_only_rows_matching_join_statements(
    table_reader_mock: TableReader,
    spark: SparkSession,
    from_date: datetime,
    to_date: datetime,
    charge_time: datetime,
    expect_empty: bool,
) -> None:
    # Arrange
    table_reader_mock.read_charge_master_data_periods.return_value = (
        spark.createDataFrame(data=[_create_charge_master_data_row()])
    )

    table_reader_mock.read_charge_links_periods.return_value = spark.createDataFrame(
        [_create_charge_link_periods_row(from_date=from_date, to_date=to_date)]
    )

    table_reader_mock.read_charge_price_points.return_value = spark.createDataFrame(
        [_create_charges_price_points_row(observation_time=charge_time)]
    )

    # Act
    actual = read_charges(table_reader_mock)

    # Assert
    assert actual.isEmpty() == expect_empty
