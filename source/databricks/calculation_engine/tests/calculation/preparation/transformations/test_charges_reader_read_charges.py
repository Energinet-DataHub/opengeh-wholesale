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
from package.calculation.preparation.transformations import read_charge_period_prices
from package.calculation_input.schemas import charge_master_data_periods_schema

from package.calculation_input.table_reader import TableReader
from package.codelists import ChargeType
from package.constants import Colname

DEFAULT_CHARGE_CODE = "4000"
DEFAULT_CHARGE_OWNER = "001"
DEFAULT_CHARGE_TYPE = ChargeType.TARIFF.value
DEFAULT_CHARGE_KEY = (
    f"{DEFAULT_CHARGE_CODE}-{DEFAULT_CHARGE_OWNER}-{DEFAULT_CHARGE_TYPE}"
)
DEFAULT_CHARGE_TAX = True
DEFAULT_CHARGE_PRICE = Decimal(1.0)
DEFAULT_RESOLUTION = "P1D"
DEFAULT_FROM_DATE = datetime(2020, 1, 1, 0, 0)
DEFAULT_TO_DATE = datetime(2020, 2, 1, 0, 0)
DEFAULT_CHARGE_TIME = datetime(2020, 1, 1, 0, 0)


def _create_charge_master_data_row(
    charge_code: str = DEFAULT_CHARGE_CODE,
    charge_owner: str = DEFAULT_CHARGE_OWNER,
    charge_type: str = DEFAULT_CHARGE_TYPE,
    charge_tax: bool = DEFAULT_CHARGE_TAX,
    resolution: str = DEFAULT_RESOLUTION,
    from_date: datetime = DEFAULT_FROM_DATE,
    to_date: datetime = DEFAULT_TO_DATE,
) -> Row:
    row = {
        Colname.charge_code: charge_code,
        Colname.charge_type: charge_type,
        Colname.charge_owner: charge_owner,
        Colname.resolution: resolution,
        Colname.charge_tax: charge_tax,
        Colname.from_date: from_date,
        Colname.to_date: to_date,
    }
    return Row(**row)


def _create_charges_price_points_row(
    charge_key: str = DEFAULT_CHARGE_KEY,
    charge_code: str = DEFAULT_CHARGE_CODE,
    charge_owner: str = DEFAULT_CHARGE_OWNER,
    charge_type: str = DEFAULT_CHARGE_TYPE,
    charge_price: Decimal = DEFAULT_CHARGE_PRICE,
    charge_time: datetime = DEFAULT_CHARGE_TIME,
) -> Row:
    row = {
        Colname.charge_key: charge_key,
        Colname.charge_code: charge_code,
        Colname.charge_owner: charge_owner,
        Colname.charge_type: charge_type,
        Colname.charge_price: charge_price,
        Colname.charge_time: charge_time,
    }
    return Row(**row)


class TestWhenValidInput:
    @patch.object(calculation_input, TableReader.__name__)
    def test_returns_expected_joined_row_values(
        self, table_reader_mock: TableReader, spark: SparkSession
    ) -> None:
        # Arrange
        table_reader_mock.read_charge_master_data_periods.return_value = (
            spark.createDataFrame(data=[_create_charge_master_data_row()])
        )
        table_reader_mock.read_charge_price_points.return_value = spark.createDataFrame(
            data=[_create_charges_price_points_row()]
        )

        # Act
        actual = read_charge_period_prices(
            table_reader_mock, DEFAULT_FROM_DATE, DEFAULT_TO_DATE
        )

        # Assert
        assert actual.df.count() == 1
        actual_row = actual.df.collect()[0]
        assert actual_row[Colname.charge_key] == DEFAULT_CHARGE_KEY
        assert actual_row[Colname.charge_code] == DEFAULT_CHARGE_CODE
        assert actual_row[Colname.charge_type] == DEFAULT_CHARGE_TYPE
        assert actual_row[Colname.charge_owner] == DEFAULT_CHARGE_OWNER
        assert actual_row[Colname.charge_tax] == DEFAULT_CHARGE_TAX
        assert actual_row[Colname.resolution] == DEFAULT_RESOLUTION
        assert actual_row[Colname.charge_price] == DEFAULT_CHARGE_PRICE
        assert actual_row[Colname.from_date] == DEFAULT_FROM_DATE
        assert actual_row[Colname.to_date] == DEFAULT_TO_DATE
        assert actual_row[Colname.charge_time] == DEFAULT_CHARGE_TIME


class TestWhenChargeTimeIsOutsideCalculationPeriod:
    @pytest.mark.parametrize(
        "from_date, to_date, charge_time",
        [
            (  # charge_time < from_data
                datetime(2020, 1, 2, 0, 0),
                datetime(2020, 1, 4, 0, 0),
                datetime(2020, 1, 1, 0, 0),
            ),
            (  # charge_time > to_data
                datetime(2020, 1, 2, 0, 0),
                datetime(2020, 1, 4, 0, 0),
                datetime(2020, 1, 5, 0, 0),
            ),
            (  # charge_time = to_data
                datetime(2020, 1, 2, 0, 0),
                datetime(2020, 1, 4, 0, 0),
                datetime(2020, 1, 4, 0, 0),
            ),
        ],
    )
    @patch.object(calculation_input, TableReader.__name__)
    def test__returns_empty_result(
        self,
        table_reader_mock: TableReader,
        spark: SparkSession,
        from_date,
        to_date,
        charge_time,
    ) -> None:
        # Arrange
        table_reader_mock.read_charge_master_data_periods.return_value = (
            spark.createDataFrame(
                data=[
                    _create_charge_master_data_row(
                        from_date=from_date,
                        to_date=from_date,
                    )
                ]
            )
        )

        table_reader_mock.read_charge_price_points.return_value = spark.createDataFrame(
            data=[_create_charges_price_points_row(charge_time=charge_time)]
        )

        # Act
        actual = read_charge_period_prices(table_reader_mock, from_date, to_date)

        # Assert
        assert actual.df.isEmpty()


class TestWhenChargeTimeIsInsideCalculationPeriod:
    @pytest.mark.parametrize(
        "from_date, to_date, charge_time",
        [
            (  # from_data < charge_time < to_date
                datetime(2020, 1, 2, 0, 0),
                datetime(2020, 1, 4, 0, 0),
                datetime(2020, 1, 3, 0, 0),
            ),
            (  # charge_time = from_data
                datetime(2020, 1, 2, 0, 0),
                datetime(2020, 1, 4, 0, 0),
                datetime(2020, 1, 2, 0, 0),
            ),
        ],
    )
    @patch.object(calculation_input, TableReader.__name__)
    def test__returns_charge(
        self,
        table_reader_mock: TableReader,
        spark: SparkSession,
        from_date,
        to_date,
        charge_time,
    ) -> None:
        # Arrange
        table_reader_mock.read_charge_master_data_periods.return_value = (
            spark.createDataFrame(
                data=[
                    _create_charge_master_data_row(
                        from_date=from_date,
                        to_date=to_date,
                    )
                ]
            )
        )

        table_reader_mock.read_charge_price_points.return_value = spark.createDataFrame(
            data=[_create_charges_price_points_row(charge_time=charge_time)]
        )

        # Act
        actual = read_charge_period_prices(table_reader_mock, from_date, to_date)

        # Assert
        assert actual.df.count() == 1


class TestWhenChargePeriodExceedsCalculationPeriod:
    @pytest.mark.parametrize(
        "charge_from_date, charge_to_date, expected_from_date, expected_to_date",
        [
            (  # Dataset: charge period starts before calculation period
                datetime(2020, 1, 1, 0, 0),
                datetime(2020, 1, 3, 0, 0),
                datetime(2020, 1, 2, 0, 0),
                datetime(2020, 1, 3, 0, 0),
            ),
            (  # Dataset: charge period ends after calculation period
                datetime(2020, 1, 2, 0, 0),
                datetime(2020, 1, 5, 0, 0),
                datetime(2020, 1, 2, 0, 0),
                datetime(2020, 1, 3, 0, 0),
            ),
            (  # Dataset: charge period exceeds calculation period at both ends
                datetime(2020, 1, 1, 0, 0),
                datetime(2020, 1, 5, 0, 0),
                datetime(2020, 1, 2, 0, 0),
                datetime(2020, 1, 3, 0, 0),
            ),
            (  # Dataset: charge period never stops (None)
                datetime(2020, 1, 1, 0, 0),
                None,
                datetime(2020, 1, 2, 0, 0),
                datetime(2020, 1, 3, 0, 0),
            ),
        ],
    )
    @patch.object(calculation_input, TableReader.__name__)
    def test__returns_expected_to_and_from_date(
        self,
        table_reader_mock: TableReader,
        spark: SparkSession,
        charge_from_date: datetime,
        charge_to_date: datetime,
        expected_from_date: datetime,
        expected_to_date: datetime,
    ) -> None:
        # Arrange
        calculation_from_date = datetime(2020, 1, 2, 0, 0)
        calculation_to_date = datetime(2020, 1, 3, 0, 0)
        table_reader_mock.read_charge_master_data_periods.return_value = (
            spark.createDataFrame(
                data=[
                    _create_charge_master_data_row(
                        from_date=charge_from_date,
                        to_date=charge_to_date,
                    )
                ],
                schema=charge_master_data_periods_schema,
            )
        )

        charge_time = max(
            charge_from_date, calculation_from_date
        )  # ensure time is included in period
        table_reader_mock.read_charge_price_points.return_value = spark.createDataFrame(
            data=[_create_charges_price_points_row(charge_time=charge_time)]
        )

        # Act
        actual = read_charge_period_prices(
            table_reader_mock, calculation_from_date, calculation_to_date
        )

        # Assert
        actual_row = actual.df.collect()[0]
        assert actual_row[Colname.from_date] == expected_from_date
        assert actual_row[Colname.to_date] == expected_to_date


class TestWhenMultipleChargeKeys:
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
    def test_returns_only_rows_matching_join_statements(
        self,
        table_reader_mock: TableReader,
        spark: SparkSession,
        from_date: datetime,
        to_date: datetime,
        charge_time: datetime,
        expect_empty: bool,
    ) -> None:
        # Arrange
        period_from_date = min(
            from_date, charge_time
        )  # ensure all times are within period
        period_to_date = max(to_date, charge_time)  # ensure all times are within period
        table_reader_mock.read_charge_master_data_periods.return_value = (
            spark.createDataFrame(
                data=[
                    _create_charge_master_data_row(from_date=from_date, to_date=to_date)
                ]
            )
        )

        table_reader_mock.read_charge_price_points.return_value = spark.createDataFrame(
            [_create_charges_price_points_row(charge_time=charge_time)]
        )

        # Act
        actual = read_charge_period_prices(
            table_reader_mock, period_from_date, period_to_date
        )

        # Assert
        assert actual.df.isEmpty() == expect_empty
