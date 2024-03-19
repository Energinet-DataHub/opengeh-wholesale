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

from pyspark.sql import SparkSession

from package.calculation.wholesale.subscription_calculators import (
    calculate,
)
from package.codelists import (
    MeteringPointType,
    SettlementMethod,
    QuantityQuality,
)
from package.constants import Colname
import tests.calculation.wholesale.prepared_subscriptions_factory as factory


class DefaultValues:
    GRID_AREA = "543"
    CHARGE_CODE = "4000"
    CHARGE_OWNER = "001"
    CHARGE_TIME_HOUR_0 = datetime(2019, 12, 31, 23)
    CHARGE_PRICE = Decimal("2.000005")
    CHARGE_QUANTITY = 1
    ENERGY_SUPPLIER_ID = "1234567890123"
    METERING_POINT_ID = "123456789012345678901234567"
    METERING_POINT_TYPE = MeteringPointType.CONSUMPTION
    SETTLEMENT_METHOD = SettlementMethod.FLEX
    QUANTITY = Decimal("1.005")
    QUALITY = QuantityQuality.CALCULATED
    CALCULATION_PERIOD_START = datetime(2020, 1, 31, 23, 0)
    CALCULATION_PERIOD_END = datetime(2020, 2, 29, 23, 0)
    DAYS_IN_MONTH = 29
    CALCULATION_MONTH = 2
    TIME_ZONE = "Europe/Copenhagen"


def _get_all_wholesale_metering_point_types() -> list[MeteringPointType]:
    return [
        metering_point_type
        for metering_point_type in MeteringPointType
        if metering_point_type != MeteringPointType.EXCHANGE
    ]


class TestWhenValidInput:
    @pytest.mark.parametrize(
        "period_start, period_end, input_charge_price, expected_output_charge_price",
        [
            (  # month with 29 days
                datetime(2020, 1, 31, 23, 0),
                datetime(2020, 2, 29, 23, 0),
                Decimal("10"),
                Decimal("0.344828"),  # 10 / 29 (days)
            ),
            (  # month with 31 days
                datetime(2020, 4, 30, 22, 0),
                datetime(2020, 5, 31, 22, 0),
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
        subscriptions_row = factory.create_row(charge_price=input_charge_price)
        prepared_subscriptions_charges = factory.create(spark, [subscriptions_row])

        # Act
        actual = calculate(
            prepared_subscriptions_charges,
            period_start,
            period_end,
            DefaultValues.TIME_ZONE,
        ).df

        # Assert
        assert actual.count() == 1
        assert actual.collect()[0][Colname.charge_price] == expected_output_charge_price

    def test__returns_expected_total_quantity(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange
        quantity_1 = 1
        quantity_2 = 2
        expected_total_quantity = quantity_1 + quantity_2
        prepared_subscriptions_rows = [
            factory.create_row(metering_point_id="1", charge_quantity=quantity_1),
            factory.create_row(metering_point_id="2", charge_quantity=quantity_2),
        ]
        prepared_subscriptions = factory.create(spark, prepared_subscriptions_rows)

        # Act
        actual = calculate(
            prepared_subscriptions,
            DefaultValues.CALCULATION_PERIOD_START,
            DefaultValues.CALCULATION_PERIOD_END,
            DefaultValues.TIME_ZONE,
        ).df

        # Assert
        assert actual.collect()[0][Colname.total_quantity] == expected_total_quantity

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

        prepared_subscriptions_rows = [
            factory.create_row(
                metering_point_id="1",
                charge_quantity=quantity_1,
                charge_price=price,
            ),
            factory.create_row(
                metering_point_id="2",
                charge_quantity=quantity_2,
                charge_price=price,
            ),
        ]
        prepared_subscriptions = factory.create(spark, prepared_subscriptions_rows)

        # Act
        actual = calculate(
            prepared_subscriptions,
            DefaultValues.CALCULATION_PERIOD_START,
            DefaultValues.CALCULATION_PERIOD_END,
            DefaultValues.TIME_ZONE,
        ).df

        # Assert
        assert actual.collect()[0][Colname.total_amount] == expected_amount

    def test__returns_result_per_metering_point_type(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange
        all_metering_point_types = _get_all_wholesale_metering_point_types()

        subscriptions_rows = [
            factory.create_row(metering_point_type=metering_point_type)
            for metering_point_type in all_metering_point_types
        ]
        prepared_subscriptions = factory.create(spark, subscriptions_rows)

        # Act
        actual = calculate(
            prepared_subscriptions,
            DefaultValues.CALCULATION_PERIOD_START,
            DefaultValues.CALCULATION_PERIOD_END,
            DefaultValues.TIME_ZONE,
        ).df

        # Assert
        expected = [
            metering_point_type.value
            for metering_point_type in all_metering_point_types
        ]
        assert actual.count() == len(expected)
        actual_metering_point_types = [
            row[Colname.metering_point_type] for row in actual.collect()
        ]
        assert set(actual_metering_point_types) == set(expected)

    def test__returns_result_per_settlement_method(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange

        prepared_subscriptions_rows = [
            factory.create_row(settlement_method=SettlementMethod.FLEX),
            factory.create_row(settlement_method=SettlementMethod.NON_PROFILED),
        ]
        prepared_subscriptions = factory.create(spark, prepared_subscriptions_rows)

        # Act
        actual = calculate(
            prepared_subscriptions,
            DefaultValues.CALCULATION_PERIOD_START,
            DefaultValues.CALCULATION_PERIOD_END,
            DefaultValues.TIME_ZONE,
        ).df

        # Assert
        expected = [SettlementMethod.FLEX.value, SettlementMethod.NON_PROFILED.value]
        assert actual.count() == 2
        actual_settlement_methods = [
            row[Colname.settlement_method] for row in actual.collect()
        ]
        assert set(actual_settlement_methods) == set(expected)


class TestWhenMissingSomeInputChargePrice:
    def test__returns_expected_result(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange
        charge_quantity_1 = 1
        charge_quantity_2 = 2
        charge_price = Decimal("1.123456")
        expected_total_quantity = charge_quantity_1 + charge_quantity_2
        expected_charge_price = round(charge_price / DefaultValues.DAYS_IN_MONTH, 6)
        expected_charge_amount = charge_quantity_2 * expected_charge_price

        prepared_subscriptions_rows = [
            factory.create_row(
                metering_point_id="1",
                charge_quantity=charge_quantity_1,
                charge_price=None,
            ),
            factory.create_row(
                metering_point_id="2",
                charge_quantity=charge_quantity_2,
                charge_price=charge_price,
            ),
        ]
        prepared_subscriptions = factory.create(spark, prepared_subscriptions_rows)

        # Act
        actual = calculate(
            prepared_subscriptions,
            DefaultValues.CALCULATION_PERIOD_START,
            DefaultValues.CALCULATION_PERIOD_END,
            DefaultValues.TIME_ZONE,
        ).df

        # Assert
        assert actual.count() == 1
        assert actual.collect()[0][Colname.total_quantity] == expected_total_quantity
        assert actual.collect()[0][Colname.charge_price] == expected_charge_price
        assert actual.collect()[0][Colname.total_amount] == expected_charge_amount


class TestWhenMissingAllInputChargePrices:
    def test__returns_expected_result(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange
        charge_quantity_1 = 1
        charge_quantity_2 = 2
        expected_total_quantity = charge_quantity_1 + charge_quantity_2

        prepared_subscriptions_rows = [
            factory.create_row(
                metering_point_id="1",
                charge_quantity=charge_quantity_1,
                charge_price=None,
            ),
            factory.create_row(
                metering_point_id="2",
                charge_quantity=charge_quantity_2,
                charge_price=None,
            ),
        ]
        prepared_subscriptions = factory.create(spark, prepared_subscriptions_rows)

        # Act
        actual = calculate(
            prepared_subscriptions,
            DefaultValues.CALCULATION_PERIOD_START,
            DefaultValues.CALCULATION_PERIOD_END,
            DefaultValues.TIME_ZONE,
        ).df

        # Assert
        assert actual.count() == 1
        assert actual.collect()[0][Colname.total_quantity] == expected_total_quantity
        assert actual.collect()[0][Colname.charge_price] is None
        assert actual.collect()[0][Colname.total_amount] is None


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
        expected_total_quantity_1 = 2 * quantity_1
        expected_total_quantity_2 = 2 * quantity_2

        prepared_subscriptions_rows = [
            factory.create_row(
                metering_point_id="1",
                charge_time=time_1,
                charge_quantity=quantity_1,
            ),
            factory.create_row(
                metering_point_id="2",
                charge_time=time_1,
                charge_quantity=quantity_1,
            ),
            factory.create_row(
                metering_point_id="1",
                charge_time=time_2,
                charge_quantity=quantity_2,
            ),
            factory.create_row(
                metering_point_id="2",
                charge_time=time_2,
                charge_quantity=quantity_2,
            ),
        ]
        prepared_subscriptions = factory.create(spark, prepared_subscriptions_rows)

        # Act
        actual = calculate(
            prepared_subscriptions,
            DefaultValues.CALCULATION_PERIOD_START,
            DefaultValues.CALCULATION_PERIOD_END,
            DefaultValues.TIME_ZONE,
        ).df

        # Assert
        actual_rows = actual.orderBy(Colname.charge_time).collect()
        assert len(actual_rows) == 2
        assert actual_rows[0][Colname.total_quantity] == expected_total_quantity_1
        assert actual_rows[1][Colname.total_quantity] == expected_total_quantity_2


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
        prepared_subscriptions_rows = factory.create_row(charge_time=period_start)
        prepared_subscriptions = factory.create(spark, prepared_subscriptions_rows)

        # Act & Assert
        with pytest.raises(Exception):
            calculate(
                prepared_subscriptions,
                period_start,
                period_end,
                DefaultValues.TIME_ZONE,
            )
