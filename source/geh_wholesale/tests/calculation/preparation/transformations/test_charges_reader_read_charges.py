from datetime import datetime
from decimal import Decimal
from unittest.mock import patch

import pytest
from pyspark import Row
from pyspark.sql import SparkSession

from geh_wholesale.calculation.preparation.transformations import (
    read_charge_price_information,
    read_charge_prices,
)
from geh_wholesale.codelists import ChargeType
from geh_wholesale.constants import Colname
from geh_wholesale.databases import migrations_wholesale
from geh_wholesale.databases.migrations_wholesale import MigrationsWholesaleRepository
from geh_wholesale.databases.migrations_wholesale.schemas import (
    charge_price_information_periods_schema,
)

DEFAULT_CHARGE_CODE = "4000"
DEFAULT_CHARGE_OWNER = "001"
DEFAULT_CHARGE_TYPE = ChargeType.TARIFF.value
DEFAULT_CHARGE_KEY = f"{DEFAULT_CHARGE_CODE}-{DEFAULT_CHARGE_OWNER}-{DEFAULT_CHARGE_TYPE}"
DEFAULT_CHARGE_TAX = True
DEFAULT_CHARGE_PRICE = Decimal(1.0)
DEFAULT_RESOLUTION = "P1D"
DEFAULT_FROM_DATE = datetime(2020, 1, 1, 0, 0)
DEFAULT_TO_DATE = datetime(2020, 2, 1, 0, 0)
DEFAULT_CHARGE_TIME = datetime(2020, 1, 1, 0, 0)


def _create_charge_price_information_row(
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
    @patch.object(migrations_wholesale, MigrationsWholesaleRepository.__name__)
    def test_read_charge_price_information_returns_expected_row_values(
        self, repository_mock: MigrationsWholesaleRepository, spark: SparkSession
    ) -> None:
        # Arrange
        repository_mock.read_charge_price_information_periods.return_value = spark.createDataFrame(
            data=[_create_charge_price_information_row()]
        )

        # Act
        actual = read_charge_price_information(repository_mock, DEFAULT_FROM_DATE, DEFAULT_TO_DATE).df

        # Assert
        assert actual.count() == 1
        actual_row = actual.collect()[0]
        assert actual_row[Colname.charge_key] == DEFAULT_CHARGE_KEY
        assert actual_row[Colname.charge_code] == DEFAULT_CHARGE_CODE
        assert actual_row[Colname.charge_type] == DEFAULT_CHARGE_TYPE
        assert actual_row[Colname.charge_owner] == DEFAULT_CHARGE_OWNER
        assert actual_row[Colname.charge_tax] == DEFAULT_CHARGE_TAX
        assert actual_row[Colname.resolution] == DEFAULT_RESOLUTION
        assert actual_row[Colname.from_date] == DEFAULT_FROM_DATE
        assert actual_row[Colname.to_date] == DEFAULT_TO_DATE

    @patch.object(migrations_wholesale, MigrationsWholesaleRepository.__name__)
    def test_read_charge_prices_returns_expected_row_values(
        self, repository_mock: MigrationsWholesaleRepository, spark: SparkSession
    ) -> None:
        # Arrange
        repository_mock.read_charge_price_points.return_value = spark.createDataFrame(
            data=[_create_charges_price_points_row()]
        )

        # Act
        actual = read_charge_prices(repository_mock, DEFAULT_FROM_DATE, DEFAULT_TO_DATE).df

        # Assert
        assert actual.count() == 1
        actual_row = actual.collect()[0]
        assert actual_row[Colname.charge_key] == DEFAULT_CHARGE_KEY
        assert actual_row[Colname.charge_code] == DEFAULT_CHARGE_CODE
        assert actual_row[Colname.charge_type] == DEFAULT_CHARGE_TYPE
        assert actual_row[Colname.charge_owner] == DEFAULT_CHARGE_OWNER
        assert actual_row[Colname.charge_price] == DEFAULT_CHARGE_PRICE
        assert actual_row[Colname.charge_time] == DEFAULT_CHARGE_TIME


class TestWhenChargeTimeIsOutsideCalculationPeriod:
    @pytest.mark.parametrize(
        ("from_date", "to_date", "charge_time"),
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
    @patch.object(migrations_wholesale, MigrationsWholesaleRepository.__name__)
    def test__returns_empty_result(
        self,
        repository_mock: MigrationsWholesaleRepository,
        spark: SparkSession,
        from_date,
        to_date,
        charge_time,
    ) -> None:
        # Arrange
        repository_mock.read_charge_price_points.return_value = spark.createDataFrame(
            data=[_create_charges_price_points_row(charge_time=charge_time)]
        )

        # Act
        actual = read_charge_prices(repository_mock, from_date, to_date).df

        # Assert
        assert actual.isEmpty()


class TestWhenChargeTimeIsInsideCalculationPeriod:
    @pytest.mark.parametrize(
        ("from_date", "to_date", "charge_time"),
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
    @patch.object(migrations_wholesale, MigrationsWholesaleRepository.__name__)
    def test__returns_charge(
        self,
        repository_mock: MigrationsWholesaleRepository,
        spark: SparkSession,
        from_date,
        to_date,
        charge_time,
    ) -> None:
        # Arrange
        repository_mock.read_charge_price_points.return_value = spark.createDataFrame(
            data=[_create_charges_price_points_row(charge_time=charge_time)]
        )

        # Act
        actual = read_charge_prices(repository_mock, from_date, to_date).df

        # Assert
        assert actual.count() == 1


class TestWhenChargePeriodExceedsCalculationPeriod:
    @pytest.mark.parametrize(
        ("charge_from_date", "charge_to_date", "expected_from_date", "expected_to_date"),
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
    @patch.object(migrations_wholesale, MigrationsWholesaleRepository.__name__)
    def test__returns_expected_to_and_from_date(
        self,
        repository_mock: MigrationsWholesaleRepository,
        spark: SparkSession,
        charge_from_date: datetime,
        charge_to_date: datetime,
        expected_from_date: datetime,
        expected_to_date: datetime,
    ) -> None:
        # Arrange
        calculation_from_date = datetime(2020, 1, 2, 0, 0)
        calculation_to_date = datetime(2020, 1, 3, 0, 0)
        repository_mock.read_charge_price_information_periods.return_value = spark.createDataFrame(
            data=[
                _create_charge_price_information_row(
                    from_date=charge_from_date,
                    to_date=charge_to_date,
                )
            ],
            schema=charge_price_information_periods_schema,
        )

        # Act
        actual = read_charge_price_information(repository_mock, calculation_from_date, calculation_to_date).df

        # Assert
        actual_row = actual.collect()[0]
        assert actual_row[Colname.from_date] == expected_from_date
        assert actual_row[Colname.to_date] == expected_to_date
