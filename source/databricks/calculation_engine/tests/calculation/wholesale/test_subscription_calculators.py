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
from datetime import datetime, timedelta

import pytest

from pyspark import Row
from pyspark.sql import DataFrame, SparkSession

from package.calculation.wholesale.schemas.calculate_daily_subscription_price_schema import (
    subscriptions_schema,
)
from package.calculation.wholesale.subscription_calculators import (
    calculate_daily_subscription_amount,
)
from package.codelists import (
    MeteringPointType,
    SettlementMethod,
    ChargeType,
    QuantityQuality,
)
from package.constants import Colname


class DefaultValues:
    GRID_AREA = "543"
    CHARGE_CODE = "4000"
    CHARGE_OWNER = "001"
    CHARGE_TIME_HOUR_0 = datetime(2019, 12, 31, 23)
    CHARGE_PRICE = Decimal("2.000005")
    CHARGE_QUANTITY = 1
    ENERGY_SUPPLIER_ID = "1234567890123"
    METERING_POINT_ID = "123456789012345678901234567"
    QUANTITY = Decimal("1.005")
    QUALITY = QuantityQuality.CALCULATED
    CALCULATION_PERIOD_START = datetime(2020, 1, 31, 23, 0)
    CALCULATION_PERIOD_END = datetime(2020, 2, 29, 23, 0)
    DAYS_IN_MONTH = 29
    CALCULATION_MONTH = 2
    TIME_ZONE = "Europe/Copenhagen"


def _create_subscription_row(
    charge_key: str | None = None,
    charge_code: str = DefaultValues.CHARGE_CODE,
    charge_owner: str = DefaultValues.CHARGE_OWNER,
    charge_time: datetime = DefaultValues.CHARGE_TIME_HOUR_0,
    charge_price: Decimal | None = DefaultValues.CHARGE_PRICE,
    charge_quantity: int = DefaultValues.CHARGE_QUANTITY,
    energy_supplier_id: str = DefaultValues.ENERGY_SUPPLIER_ID,
    metering_point_id: str = DefaultValues.METERING_POINT_ID,
    grid_area: str = DefaultValues.GRID_AREA,
    quality: QuantityQuality = DefaultValues.QUALITY,
) -> Row:
    charge_type = ChargeType.SUBSCRIPTION.value
    row = {
        Colname.charge_key: charge_key or f"{charge_code}-{charge_type}-{charge_owner}",
        Colname.charge_type: charge_type,
        Colname.charge_owner: charge_owner,
        Colname.charge_code: charge_code,
        Colname.charge_time: charge_time,
        Colname.charge_price: charge_price,
        Colname.charge_tax: False,
        Colname.charge_quantity: charge_quantity,
        Colname.metering_point_type: MeteringPointType.CONSUMPTION.value,
        Colname.settlement_method: SettlementMethod.FLEX.value,
        Colname.metering_point_id: metering_point_id,
        Colname.grid_area: grid_area,
        Colname.energy_supplier_id: energy_supplier_id,
        Colname.qualities: [quality.value],
    }

    return Row(**row)


def _create_default_subscription_charges(spark: SparkSession) -> DataFrame:
    return spark.createDataFrame(
        [
            _create_subscription_row(),
        ],
        schema=subscriptions_schema,
    )


class TestWhenValidInput:
    @pytest.mark.parametrize(
        "period_start, period_end, input_charge_price, expected_output_charge_price",
        [
            (  # Entering daylights saving time
                datetime(2020, 1, 31, 23, 0),
                datetime(2020, 2, 29, 22, 0),
                Decimal("10"),
                Decimal("0.344828"),  # 10 / 29 (days)
            ),
            (  # Exiting daylights saving time
                datetime(2020, 9, 30, 22, 0),
                datetime(2020, 10, 31, 23, 0),
                Decimal("10"),
                Decimal("0.322581"),  # 10 / 31 (days)
            ),
        ],
    )
    def test__returns_expected_charge_price(
        self,
        spark: SparkSession,
        period_start: datetime,
        period_end: datetime,
        input_charge_price: Decimal,
        expected_output_charge_price: Decimal,
    ) -> None:
        # Arrange
        subscription_row = _create_subscription_row(charge_price=input_charge_price)
        subscription_charges = spark.createDataFrame(
            [subscription_row], schema=subscriptions_schema
        )

        # Act
        actual = calculate_daily_subscription_amount(
            subscription_charges,
            period_start,
            period_end,
            DefaultValues.TIME_ZONE,
        )

        # Assert
        assert actual.collect()[0][Colname.charge_price] == expected_output_charge_price

    def test__returns_expected_charge_count(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange
        quantity_1 = 1
        quantity_2 = 2
        expected_quantity = quantity_1 + quantity_2
        subscription_rows = [
            _create_subscription_row(metering_point_id="1", charge_quantity=quantity_1),
            _create_subscription_row(metering_point_id="2", charge_quantity=quantity_2),
        ]
        subscription_charges = spark.createDataFrame(
            subscription_rows, schema=subscriptions_schema
        )

        # Act
        actual = calculate_daily_subscription_amount(
            subscription_charges,
            DefaultValues.CALCULATION_PERIOD_START,
            DefaultValues.CALCULATION_PERIOD_END,
            DefaultValues.TIME_ZONE,
        )

        # Assert
        assert actual.collect()[0][Colname.charge_count] == expected_quantity

    def test__returns_expected_amount(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange
        quantity_1 = 1
        quantity_2 = 2
        price = Decimal("3.0")
        price_per_day = price / DefaultValues.DAYS_IN_MONTH
        expected_amount_not_rounded = (quantity_1 + quantity_2) * price_per_day
        expected_amount = round(expected_amount_not_rounded, 6)

        subscription_rows = [
            _create_subscription_row(
                metering_point_id="1",
                charge_quantity=quantity_1,
                charge_price=price,
            ),
            _create_subscription_row(
                metering_point_id="2",
                charge_quantity=quantity_2,
                charge_price=price,
            ),
        ]
        subscription_charges = spark.createDataFrame(
            subscription_rows, schema=subscriptions_schema
        )

        # Act
        actual = calculate_daily_subscription_amount(
            subscription_charges,
            DefaultValues.CALCULATION_PERIOD_START,
            DefaultValues.CALCULATION_PERIOD_END,
            DefaultValues.TIME_ZONE,
        )

        # Assert
        assert actual.collect()[0][Colname.total_amount] == expected_amount


class TestWhenMultipleMeteringPointsPerChargeTime:
    def test__returns_sum_charge_quantity_per_charge_time(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange
        time_1 = DefaultValues.CALCULATION_PERIOD_START
        time_2 = time_1 + timedelta(days=1)
        quantity_1 = 3
        quantity_2 = 4
        expected_charge_count_1 = 2 * quantity_1
        expected_charge_count_2 = 2 * quantity_2

        subscription_rows = [
            _create_subscription_row(
                metering_point_id="1",
                charge_time=time_1,
                charge_quantity=quantity_1,
            ),
            _create_subscription_row(
                metering_point_id="2",
                charge_time=time_1,
                charge_quantity=quantity_1,
            ),
            _create_subscription_row(
                metering_point_id="1",
                charge_time=time_2,
                charge_quantity=quantity_2,
            ),
            _create_subscription_row(
                metering_point_id="2",
                charge_time=time_2,
                charge_quantity=quantity_2,
            ),
        ]
        subscription_charges = spark.createDataFrame(
            subscription_rows, schema=subscriptions_schema
        )

        # Act
        actual = calculate_daily_subscription_amount(
            subscription_charges,
            DefaultValues.CALCULATION_PERIOD_START,
            DefaultValues.CALCULATION_PERIOD_END,
            DefaultValues.TIME_ZONE,
        )

        # Assert
        actual_rows = actual.orderBy(Colname.charge_time).collect()
        assert actual_rows[0][Colname.charge_count] == expected_charge_count_1
        assert actual_rows[1][Colname.charge_count] == expected_charge_count_2


class TestWhenCalculationPeriodIsNotFullMonth:
    @pytest.mark.parametrize(
        "period_start, period_end",
        [
            (  # Less than a month
                datetime(2020, 1, 10, 23, 0),
                datetime(2020, 2, 12, 23, 0),
            ),
            (  # More than a month
                datetime(2020, 1, 10, 23, 0),
                datetime(2020, 3, 12, 23, 0),
            ),
            (  # Entering daylights saving time - not ending at midnight
                datetime(2020, 2, 29, 23, 0),
                datetime(2020, 3, 31, 23, 0),
            ),
            (  # Exiting daylights saving time - not ending at midnight
                datetime(2020, 9, 30, 22, 0),
                datetime(2020, 10, 31, 22, 0),
            ),
        ],
    )
    def test__raises_exception(
        self, spark: SparkSession, period_start: datetime, period_end: datetime
    ) -> None:
        # Arrange
        subscription_row = _create_subscription_row(charge_time=period_start)
        subscription_charges = spark.createDataFrame(
            [subscription_row], schema=subscriptions_schema
        )

        # Act & Assert
        with pytest.raises(Exception):
            calculate_daily_subscription_amount(
                subscription_charges,
                period_start,
                period_end,
                DefaultValues.TIME_ZONE,
            )
