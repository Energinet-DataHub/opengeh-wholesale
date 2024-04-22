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

from pyspark.sql import SparkSession

from package.calculation.wholesale.fee_calculators import (
    calculate,
)
from package.codelists import (
    MeteringPointType,
    SettlementMethod,
)
from package.constants import Colname
import calculation.wholesale.factories.prepared_fees_factory as factory


def _get_all_wholesale_metering_point_types() -> list[MeteringPointType]:
    return [
        metering_point_type
        for metering_point_type in MeteringPointType
        if metering_point_type != MeteringPointType.EXCHANGE
    ]


class TestWhenInputHasTwoMeteringPointsWithDifferentQuantities:
    def test__returns_expected_total_quantity(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange
        quantity_1 = 1
        quantity_2 = 2
        expected_total_quantity = quantity_1 + quantity_2
        prepared_fees_rows = [
            factory.create_row(metering_point_id="1", quantity=quantity_1),
            factory.create_row(metering_point_id="2", quantity=quantity_2),
        ]
        prepared_fees = factory.create(spark, prepared_fees_rows)

        # Act
        actual = calculate(prepared_fees)

        # Assert
        assert actual.df.collect()[0][Colname.total_quantity] == expected_total_quantity

    def test__returns_expected_amount(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange
        quantity_1 = 1
        quantity_2 = 2
        price = Decimal("3.0")
        expected_amount_not_rounded = (quantity_1 + quantity_2) * price
        expected_amount = round(expected_amount_not_rounded, 6)

        prepared_fees_rows = [
            factory.create_row(
                metering_point_id="1",
                quantity=quantity_1,
                charge_price=price,
            ),
            factory.create_row(
                metering_point_id="2",
                quantity=quantity_2,
                charge_price=price,
            ),
        ]
        prepared_fees = factory.create(spark, prepared_fees_rows)

        # Act
        actual = calculate(prepared_fees)

        # Assert
        assert actual.df.collect()[0][Colname.total_amount] == expected_amount


class TestWhenInputContainsMultipleMeteringPointTypes:
    def test__returns_result_per_metering_point_type(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange
        all_metering_point_types = _get_all_wholesale_metering_point_types()

        fees_rows = [
            factory.create_row(metering_point_type=metering_point_type)
            for metering_point_type in all_metering_point_types
        ]
        prepared_fees = factory.create(spark, fees_rows)

        # Act
        actual = calculate(prepared_fees).df

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


class TestWhenInputContainsMultipleSettlementMethods:
    def test__returns_result_per_settlement_method(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange

        prepared_fees_rows = [
            factory.create_row(settlement_method=SettlementMethod.FLEX),
            factory.create_row(settlement_method=SettlementMethod.NON_PROFILED),
        ]
        prepared_fees = factory.create(spark, prepared_fees_rows)

        # Act
        actual_df = calculate(prepared_fees).df

        # Assert
        expected = [SettlementMethod.FLEX.value, SettlementMethod.NON_PROFILED.value]
        assert actual_df.count() == 2
        actual_settlement_methods = [
            row[Colname.settlement_method] for row in actual_df.collect()
        ]
        assert set(actual_settlement_methods) == set(expected)


class TestWhenInputContainsMultipleChargeTimes:
    def test__returns_result_per_charge_time(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange
        charge_time_1 = datetime(2019, 12, 31, 23)
        charge_time_2 = charge_time_1 + timedelta(days=1)

        prepared_fees_rows = [
            factory.create_row(charge_time=charge_time_1, metering_point_id="1"),
            factory.create_row(charge_time=charge_time_1, metering_point_id="2"),
            factory.create_row(charge_time=charge_time_2, metering_point_id="1"),
            factory.create_row(charge_time=charge_time_2, metering_point_id="2"),
        ]
        prepared_fees = factory.create(spark, prepared_fees_rows)

        # Act
        actual_df = calculate(prepared_fees).df

        # Assert
        expected = [charge_time_1, charge_time_2]
        assert actual_df.count() == 2
        actual_charge_times = [row[Colname.charge_time] for row in actual_df.collect()]
        assert set(actual_charge_times) == set(expected)


class TestWhenMissingAllInputChargePrices:
    def test__returns_expected_result(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange
        quantity_1 = 1
        quantity_2 = 2
        expected_total_quantity = quantity_1 + quantity_2

        prepared_fees_rows = [
            factory.create_row(
                metering_point_id="1",
                quantity=quantity_1,
                charge_price=None,
            ),
            factory.create_row(
                metering_point_id="2",
                quantity=quantity_2,
                charge_price=None,
            ),
        ]
        prepared_fees = factory.create(spark, prepared_fees_rows)

        # Act
        actual_df = calculate(prepared_fees).df

        # Assert
        assert actual_df.count() == 1
        assert actual_df.collect()[0][Colname.total_quantity] == expected_total_quantity
        assert actual_df.collect()[0][Colname.charge_price] is None
        assert actual_df.collect()[0][Colname.total_amount] is None
