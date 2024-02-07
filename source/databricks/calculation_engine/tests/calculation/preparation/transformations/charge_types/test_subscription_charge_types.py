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

import pytest
from datetime import datetime, timedelta
from decimal import Decimal
from pyspark.sql import SparkSession, Row
from package.calculation.preparation.transformations import get_subscription_charges
import package.codelists as e

from package.calculation_input.schemas import (
    metering_point_period_schema,
)
from package.calculation.wholesale.schemas.charges_schema import charges_schema
from package.constants import Colname

DEFAULT_GRID_AREA = "543"
DEFAULT_CHARGE_CODE = "4000"
DEFAULT_CHARGE_OWNER = "001"
DEFAULT_CHARGE_TAX = True
DEFAULT_CHARGE_TIME_HOUR_0 = datetime(2020, 1, 1, 22)
DEFAULT_FROM_DATE = datetime(2020, 1, 1, 22)
DEFAULT_TO_DATE = datetime(2020, 1, 2, 22)
DEFAULT_CHARGE_PRICE = Decimal("2.000005")
DEFAULT_ENERGY_SUPPLIER_ID = "1234567890123"
DEFAULT_METERING_POINT_ID = "123456789012345678901234567"
DEFAULT_METERING_POINT_TYPE = e.MeteringPointType.CONSUMPTION
DEFAULT_SETTLEMENT_METHOD = e.SettlementMethod.FLEX

JAN_1ST = datetime(2020, 1, 1, 0)
JAN_2ND = datetime(2020, 1, 2, 0)
JAN_3RD = datetime(2020, 1, 3, 0)
JAN_4TH = datetime(2020, 1, 4, 0)


def _create_metering_point_row(
    metering_point_id: str = DEFAULT_METERING_POINT_ID,
    metering_point_type: e.MeteringPointType = DEFAULT_METERING_POINT_TYPE,
    calculation_type: str = "calculation_type",
    settlement_method: e.SettlementMethod = DEFAULT_SETTLEMENT_METHOD,
    grid_area: str = DEFAULT_GRID_AREA,
    resolution: e.MeteringPointResolution = e.MeteringPointResolution.HOUR,
    from_grid_area: str = "from_grid_area",
    to_grid_area: str = "to_grid_area",
    parent_metering_point_id: str = "parent_metering_point_id",
    energy_supplier_id: str = DEFAULT_ENERGY_SUPPLIER_ID,
    balance_responsible_id: str = "balance_responsible_id",
    from_date: datetime = DEFAULT_FROM_DATE,
    to_date: datetime = DEFAULT_TO_DATE,
) -> Row:
    row = {
        Colname.metering_point_id: metering_point_id,
        Colname.metering_point_type: metering_point_type.value,
        Colname.calculation_type: calculation_type,
        Colname.settlement_method: settlement_method.value,
        Colname.grid_area: grid_area,
        Colname.resolution: resolution.value,
        Colname.from_grid_area: from_grid_area,
        Colname.to_grid_area: to_grid_area,
        Colname.parent_metering_point_id: parent_metering_point_id,
        Colname.energy_supplier_id: energy_supplier_id,
        Colname.balance_responsible_id: balance_responsible_id,
        Colname.from_date: from_date,
        Colname.to_date: to_date,
    }
    return Row(**row)


def _create_subscription_or_fee_charges_row(
    charge_type: e.ChargeType,
    charge_code: str = DEFAULT_CHARGE_CODE,
    charge_owner: str = DEFAULT_CHARGE_OWNER,
    charge_tax: bool = DEFAULT_CHARGE_TAX,
    charge_time: datetime = DEFAULT_CHARGE_TIME_HOUR_0,
    from_date: datetime = DEFAULT_FROM_DATE,
    to_date: datetime = DEFAULT_TO_DATE,
    charge_price: Decimal = DEFAULT_CHARGE_PRICE,
    metering_point_id: str = DEFAULT_METERING_POINT_ID,
) -> Row:
    charge_key: str = f"{charge_code}-{charge_owner}-{charge_type.value}"

    row = {
        Colname.charge_key: charge_key,
        Colname.charge_code: charge_code,
        Colname.charge_type: charge_type.value,
        Colname.charge_owner: charge_owner,
        Colname.charge_tax: charge_tax,
        Colname.resolution: e.ChargeResolution.MONTH.value,
        Colname.charge_time: charge_time,
        Colname.from_date: from_date,
        Colname.to_date: to_date,
        Colname.charge_price: charge_price,
        Colname.metering_point_id: metering_point_id,
    }
    return Row(**row)


def _create_expected_subscriptions_row(
    charge_key: str = f"{DEFAULT_CHARGE_CODE}-{DEFAULT_CHARGE_OWNER}-{e.ChargeType.SUBSCRIPTION.value}",
    charge_code: str = DEFAULT_CHARGE_CODE,
    charge_owner: str = DEFAULT_CHARGE_OWNER,
    charge_time: datetime = DEFAULT_CHARGE_TIME_HOUR_0,
    charge_price: Decimal = DEFAULT_CHARGE_PRICE,
    energy_supplier_id: str = DEFAULT_ENERGY_SUPPLIER_ID,
    metering_point_type: e.MeteringPointType = DEFAULT_METERING_POINT_TYPE,
    settlement_method: e.SettlementMethod = DEFAULT_SETTLEMENT_METHOD,
    grid_area: str = DEFAULT_GRID_AREA,
) -> Row:
    row = {
        Colname.charge_key: charge_key,
        Colname.charge_code: charge_code,
        Colname.charge_type: e.ChargeType.SUBSCRIPTION.value,
        Colname.charge_owner: charge_owner,
        Colname.charge_time: charge_time,
        Colname.charge_price: charge_price,
        Colname.metering_point_type: metering_point_type.value,
        Colname.settlement_method: settlement_method.value,
        Colname.grid_area: grid_area,
        Colname.energy_supplier_id: energy_supplier_id,
    }
    return Row(**row)


class TestWhenChargesContainsBothSubscriptionAndFee:
    def test_returns_only_subscription(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange
        metering_point_rows = [_create_metering_point_row()]
        charges_rows = [
            _create_subscription_or_fee_charges_row(
                charge_type=e.ChargeType.FEE,
            ),
            _create_subscription_or_fee_charges_row(
                charge_type=e.ChargeType.SUBSCRIPTION,
            ),
        ]

        metering_point = spark.createDataFrame(
            metering_point_rows, metering_point_period_schema
        )
        charges = spark.createDataFrame(charges_rows, charges_schema)

        # Act
        actual_subscription = get_subscription_charges(charges, metering_point)

        # Assert
        assert actual_subscription.count() == 1
        assert (
            actual_subscription.collect()[0][Colname.charge_type]
            == e.ChargeType.SUBSCRIPTION.value
        )


class TestWhenValidInput:
    @pytest.mark.parametrize(
        "charge_time, from_date, to_date, expected_day_count",
        [
            (
                datetime(2021, 6, 1, 0),
                datetime(2021, 5, 31, 22, 0, 0),
                datetime(2021, 6, 30, 22, 0, 0),
                30,
            ),
            # leap year
            (
                datetime(2020, 2, 1, 0),
                datetime(2020, 2, 1, 0),
                datetime(2020, 3, 1, 0),
                29,
            ),
            # non-leap year
            (
                datetime(2021, 2, 1, 0),
                datetime(2021, 2, 1, 0),
                datetime(2021, 3, 1, 0),
                28,
            ),
        ],
    )
    def test_splits_into_days_between_from_and_to_date(
        self,
        spark: SparkSession,
        charge_time: datetime,
        from_date: datetime,
        to_date: datetime,
        expected_day_count: int,
    ) -> None:
        # Arrange
        metering_point_rows = [
            _create_metering_point_row(from_date=from_date, to_date=to_date)
        ]
        charges_rows = [
            _create_subscription_or_fee_charges_row(
                charge_time=charge_time,
                from_date=from_date,
                to_date=to_date,
                charge_type=e.ChargeType.SUBSCRIPTION,
            ),
        ]

        metering_point = spark.createDataFrame(
            metering_point_rows, metering_point_period_schema
        )
        charges = spark.createDataFrame(charges_rows, charges_schema)

        # Act
        actual_subscription = get_subscription_charges(charges, metering_point)

        # Assert
        assert actual_subscription.count() == expected_day_count

    def test_returns_df_with_expected_values(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange
        metering_point_rows = [_create_metering_point_row()]
        charges_rows = [
            _create_subscription_or_fee_charges_row(
                e.ChargeType.SUBSCRIPTION,
                from_date=DEFAULT_CHARGE_TIME_HOUR_0,
                to_date=DEFAULT_CHARGE_TIME_HOUR_0 + timedelta(days=1),
            )
        ]

        expected_subscription_row = [_create_expected_subscriptions_row()]

        metering_point = spark.createDataFrame(
            metering_point_rows, metering_point_period_schema
        )
        charges = spark.createDataFrame(charges_rows, charges_schema)

        expected_subscription = spark.createDataFrame(expected_subscription_row)

        # Act
        actual = get_subscription_charges(
            charges,
            metering_point,
        )

        # Assert
        assert actual.collect() == expected_subscription.collect()


class TestWhenChargeTimeDiffersFromMeteringPointPeriod:
    @pytest.mark.parametrize(
        "charge_from_date, charge_to_date, metering_point_from_date, metering_point_to_date, expected_rows",
        [
            # charge period is within metering point period
            (JAN_2ND, JAN_3RD, JAN_1ST, JAN_4TH, 1),
            # charge period ends before metering point period
            (JAN_1ST, JAN_3RD, JAN_1ST, JAN_4TH, 2),
            # charge period starts after metering point period
            (JAN_2ND, JAN_4TH, JAN_1ST, JAN_4TH, 2),
            # charge period ends before start of metering point period
            (JAN_1ST, JAN_2ND, JAN_3RD, JAN_4TH, 0),
        ],
    )
    def test_only_include_charges_in_metering_point_period(
        self,
        spark: SparkSession,
        charge_from_date: datetime,
        charge_to_date: datetime,
        metering_point_from_date: datetime,
        metering_point_to_date: datetime,
        expected_rows: int,
    ) -> None:
        """
        Only charges where charge time is greater than or equal to the metering point from date and
        less than the metering point to date are accepted.
        """
        # Arrange
        metering_point_rows = [
            _create_metering_point_row(
                from_date=metering_point_from_date, to_date=metering_point_to_date
            ),
        ]
        charges_rows = [
            _create_subscription_or_fee_charges_row(
                charge_type=e.ChargeType.SUBSCRIPTION,
                from_date=charge_from_date,
                to_date=charge_to_date,
                charge_time=charge_from_date,
            )
        ]

        metering_point = spark.createDataFrame(
            metering_point_rows, metering_point_period_schema
        )
        charges = spark.createDataFrame(charges_rows, charges_schema)

        # Act
        actual = get_subscription_charges(
            charges,
            metering_point,
        )
        actual.show()

        # Assert
        assert actual.count() == expected_rows


class TestWhenTwoSubscriptionsOverlap:
    def test_returns_both_subscriptions(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange
        metering_point_rows = [_create_metering_point_row()]
        charges_rows = [
            _create_subscription_or_fee_charges_row(
                charge_code="4000", charge_type=e.ChargeType.SUBSCRIPTION
            ),
            _create_subscription_or_fee_charges_row(
                charge_code="3000", charge_type=e.ChargeType.SUBSCRIPTION
            ),
        ]

        metering_point = spark.createDataFrame(
            metering_point_rows, metering_point_period_schema
        )
        charges = spark.createDataFrame(charges_rows, charges_schema)

        # Act
        actual = get_subscription_charges(
            charges,
            metering_point,
        )

        # Assert
        assert actual.count() == 2
