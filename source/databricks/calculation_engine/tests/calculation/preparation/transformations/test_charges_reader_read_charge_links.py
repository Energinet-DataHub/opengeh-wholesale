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
from package.calculation.preparation.transformations import read_charge_links
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
DEFAULT_METERING_POINT_ID = "123456789012345678901234567"


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


class TestWhenValidInput:
    @patch.object(calculation_input, TableReader.__name__)
    def test_returns_expected_joined_row_values(
        self, table_reader_mock: TableReader, spark: SparkSession
    ) -> None:
        # Arrange
        table_reader_mock.read_charge_links_periods.return_value = (
            spark.createDataFrame(data=[_create_charge_link_periods_row()])
        )

        # Act
        actual = read_charge_links(
            table_reader_mock, DEFAULT_FROM_DATE, DEFAULT_TO_DATE
        )

        # Assert
        assert actual.count() == 1
        actual_row = actual.collect()[0]
        assert actual_row[Colname.charge_key] == DEFAULT_CHARGE_KEY
        assert actual_row[Colname.charge_code] == DEFAULT_CHARGE_CODE
        assert actual_row[Colname.charge_type] == DEFAULT_CHARGE_TYPE
        assert actual_row[Colname.charge_owner] == DEFAULT_CHARGE_OWNER
        assert actual_row[Colname.from_date] == DEFAULT_FROM_DATE
        assert actual_row[Colname.to_date] == DEFAULT_TO_DATE
        assert actual_row[Colname.metering_point_id] == DEFAULT_METERING_POINT_ID


class TestWhenChargeLinkPeriodExceedsCalculationPeriod:
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
        table_reader_mock.read_charge_links_periods.return_value = (
            spark.createDataFrame(
                data=[
                    _create_charge_link_periods_row(
                        from_date=charge_from_date,
                        to_date=charge_to_date,
                    )
                ],
                schema=charge_master_data_periods_schema,
            )
        )

        # Act
        actual = read_charge_links(
            table_reader_mock, calculation_from_date, calculation_to_date
        )

        # Assert
        actual_row = actual.collect()[0]
        assert actual_row[Colname.from_date] == expected_from_date
        assert actual_row[Colname.to_date] == expected_to_date
