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

import pytest
from datetime import datetime
from pyspark.sql import SparkSession, Row
from pyspark.sql import functions as f

from package.calculation.preparation.transformations import (
    get_subscription_charges,
)
from package.calculation.wholesale.schemas.charges_schema import charge_prices_schema
from package.constants import Colname
import package.codelists as e

import tests.calculation.charges_factory as factory

DEFAULT_TIME_ZONE = "Europe/Copenhagen"
JAN_1ST = datetime(2022, 1, 1, 23)
JAN_2ND = datetime(2022, 1, 2, 23)
JAN_3RD = datetime(2022, 1, 3, 23)
JAN_4TH = datetime(2022, 1, 4, 23)
JAN_5TH = datetime(2022, 1, 5, 23)
JAN_6TH = datetime(2022, 1, 6, 23)


class TestWhenValidInput:
    @pytest.mark.parametrize(
        "charge_time, from_date, to_date, expected_day_count",
        [
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
    def test__returns_row_for_each_day_in_link_period(
        self,
        spark: SparkSession,
        charge_time: datetime,
        from_date: datetime,
        to_date: datetime,
        expected_day_count: int,
    ) -> None:
        # Arrange
        charge_link_metering_points_rows = [
            factory.create_charge_link_metering_point_periods_row(
                charge_type=e.ChargeType.FEE,
                from_date=from_date,
                to_date=to_date,
            ),
            factory.create_charge_link_metering_point_periods_row(
                charge_type=e.ChargeType.SUBSCRIPTION,
                from_date=from_date,
                to_date=to_date,
            ),
            factory.create_charge_link_metering_point_periods_row(
                charge_type=e.ChargeType.FEE,
                from_date=from_date,
                to_date=to_date,
            ),
        ]
        charge_master_data_rows = [
            factory.create_charge_master_data_row(
                charge_type=e.ChargeType.FEE,
                from_date=from_date,
                to_date=to_date,
            ),
            factory.create_charge_master_data_row(
                charge_type=e.ChargeType.SUBSCRIPTION,
                from_date=from_date,
                to_date=to_date,
            ),
            factory.create_charge_master_data_row(),
        ]
        charge_prices_rows = [
            factory.create_charge_prices_row(
                charge_type=e.ChargeType.FEE,
            ),
            factory.create_charge_prices_row(
                charge_type=e.ChargeType.SUBSCRIPTION,
            ),
            factory.create_charge_prices_row(
                charge_type=e.ChargeType.TARIFF,
            ),
        ]

        charge_link_metering_point_periods = (
            factory.create_charge_link_metering_point_periods(
                spark, charge_link_metering_points_rows
            )
        )
        charge_master_data = factory.create_charge_master_data(
            spark, charge_master_data_rows
        )
        charge_prices = factory.create_charge_prices(spark, charge_prices_rows)

        # Act
        actual_subscription = get_subscription_charges(
            charge_master_data,
            charge_prices,
            charge_link_metering_point_periods,
            DEFAULT_TIME_ZONE,
        )

        # Assert
        assert actual_subscription.count() == expected_day_count


class TestWhenNoPricesForPeriod:
    def test__returns_rows_where_price_is_none(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange
        from_date = factory.DefaultValues.DEFAULT_FROM_DATE
        to_date = factory.DefaultValues.DEFAULT_TO_DATE

        charge_link_metering_point_periods = (
            factory.create_charge_link_metering_point_periods(
                spark,
                factory.create_charge_link_metering_point_periods_row(
                    charge_type=e.ChargeType.SUBSCRIPTION,
                    from_date=from_date,
                    to_date=to_date,
                ),
            )
        )

        charge_prices_empty = spark.createDataFrame([], charge_prices_schema)

        charge_master_data = factory.create_charge_master_data(
            spark,
            [
                factory.create_charge_master_data_row(
                    charge_type=e.ChargeType.SUBSCRIPTION,
                    from_date=from_date,
                    to_date=to_date,
                )
            ],
        )

        # Act
        actual_subscription = get_subscription_charges(
            charge_master_data,
            charge_prices_empty,
            charge_link_metering_point_periods,
            time_zone=DEFAULT_TIME_ZONE,
        )

        # Assert
        actual_prices = actual_subscription.select(Colname.charge_price)
        assert actual_prices.count() == (to_date - from_date).days
        assert (
            actual_prices.filter(f.col(Colname.charge_price).isNotNull()).count() == 0
        )


class TestWhenInputContainsOtherChargeTypes:
    def test__returns_only_subscription_charge_type(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange
        charge_master_data = factory.create_charge_master_data(
            spark,
            [
                factory.create_charge_master_data_row(charge_type=e.ChargeType.FEE),
                factory.create_charge_master_data_row(
                    charge_type=e.ChargeType.SUBSCRIPTION
                ),
                factory.create_charge_master_data_row(charge_type=e.ChargeType.TARIFF),
            ],
        )

        charge_link_metering_point_periods = (
            factory.create_charge_link_metering_point_periods(
                spark,
                [
                    factory.create_charge_link_metering_point_periods_row(
                        charge_type=e.ChargeType.FEE
                    ),
                    factory.create_charge_link_metering_point_periods_row(
                        charge_type=e.ChargeType.SUBSCRIPTION
                    ),
                    factory.create_charge_link_metering_point_periods_row(
                        charge_type=e.ChargeType.TARIFF
                    ),
                ],
            )
        )
        charge_prices = factory.create_charge_prices(
            spark,
            [
                factory.create_charge_prices_row(charge_type=e.ChargeType.FEE),
                factory.create_charge_prices_row(charge_type=e.ChargeType.SUBSCRIPTION),
                factory.create_charge_prices_row(charge_type=e.ChargeType.TARIFF),
            ],
        )

        # Act
        actual_subscription = get_subscription_charges(
            charge_master_data,
            charge_prices,
            charge_link_metering_point_periods,
            time_zone=DEFAULT_TIME_ZONE,
        )

        # Assert
        assert (
            actual_subscription.collect()[0][Colname.charge_type]
            == e.ChargeType.SUBSCRIPTION.value
        )


class TestWhenChargePriceChangesDuringPeriod:
    def test__returns_expected_charge_time_and_price(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange
        from_date = JAN_1ST
        to_date = JAN_6TH
        input_charge_time_and_price = {
            JAN_1ST: Decimal("1.000000"),
            JAN_3RD: Decimal("2.000000"),
        }
        expected_charge_time_and_price = {
            JAN_1ST: Decimal("1.000000"),
            JAN_2ND: Decimal("1.000000"),
            JAN_3RD: Decimal("2.000000"),
            JAN_4TH: Decimal("2.000000"),
            JAN_5TH: Decimal("2.000000"),
        }

        charge_link_metering_point_periods = (
            factory.create_charge_link_metering_point_periods(
                spark,
                [
                    factory.create_charge_link_metering_point_periods_row(
                        charge_type=e.ChargeType.SUBSCRIPTION,
                        from_date=from_date,
                        to_date=to_date,
                    ),
                ],
            )
        )
        charge_master_data = factory.create_charge_master_data(
            spark,
            [
                factory.create_charge_master_data_row(
                    charge_type=e.ChargeType.SUBSCRIPTION,
                    from_date=from_date,
                    to_date=to_date,
                ),
            ],
        )
        charge_prices = factory.create_charge_prices(
            spark,
            [
                factory.create_charge_prices_row(
                    charge_time=time,
                    charge_price=price,
                    charge_type=e.ChargeType.SUBSCRIPTION,
                )
                for time, price in input_charge_time_and_price.items()
            ],
        )

        # Act
        actual_subscription = get_subscription_charges(
            charge_master_data,
            charge_prices,
            charge_link_metering_point_periods,
            time_zone=DEFAULT_TIME_ZONE,
        )

        # Assert
        actual_charge_times_and_price = {
            row[Colname.charge_time]: row[Colname.charge_price]
            for row in actual_subscription.orderBy(Colname.charge_time).collect()
        }
        assert actual_charge_times_and_price == expected_charge_time_and_price


class TestWhenChargeMasterPeriodStopsAndStartsAgain:
    def test__returns_expected_charge_times(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange
        first_period_from_date = datetime(2022, 1, 1, 23)
        first_period_to_date = datetime(2022, 1, 3, 23)
        second_period_from_date = datetime(2022, 1, 4, 23)
        second_period_to_date = datetime(2022, 1, 6, 23)
        expected_charge_times = [
            datetime(2022, 1, 1, 23),
            datetime(2022, 1, 2, 23),
            datetime(2022, 1, 4, 23),
            datetime(2022, 1, 5, 23),
        ]

        charge_master_data = factory.create_charge_master_data(
            spark,
            [
                factory.create_charge_master_data_row(
                    charge_type=e.ChargeType.SUBSCRIPTION,
                    from_date=first_period_from_date,
                    to_date=first_period_to_date,
                ),
                factory.create_charge_master_data_row(
                    charge_type=e.ChargeType.SUBSCRIPTION,
                    from_date=second_period_from_date,
                    to_date=second_period_to_date,
                ),
            ],
        )
        charge_link_metering_point_periods = (
            factory.create_charge_link_metering_point_periods(
                spark,
                [
                    factory.create_charge_link_metering_point_periods_row(
                        charge_type=e.ChargeType.SUBSCRIPTION,
                        from_date=first_period_from_date,
                        to_date=first_period_to_date,
                    ),
                    factory.create_charge_link_metering_point_periods_row(
                        charge_type=e.ChargeType.SUBSCRIPTION,
                        from_date=second_period_from_date,
                        to_date=second_period_to_date,
                    ),
                ],
            )
        )
        charge_period_prices = factory.create_charge_prices(
            spark,
            [
                factory.create_charge_prices_row(
                    charge_time=first_period_from_date,
                    charge_type=e.ChargeType.SUBSCRIPTION,
                ),
                factory.create_charge_prices_row(
                    charge_time=second_period_from_date,
                    charge_type=e.ChargeType.SUBSCRIPTION,
                ),
            ],
        )

        # Act
        actual_subscription = get_subscription_charges(
            charge_master_data,
            charge_period_prices,
            charge_link_metering_point_periods,
            time_zone=DEFAULT_TIME_ZONE,
        )

        # Assert
        actual_charge_times = set(
            row[0] for row in actual_subscription.select(Colname.charge_time).collect()
        )
        expected_charge_times_set = set(expected_charge_times)

        assert actual_charge_times == expected_charge_times_set


class TestWhenChargeLinkPeriodStopsAndStartsAgain:
    def test__returns_expected_charge_time_and_price(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange
        first_link_from_date = JAN_1ST
        first_link_to_date = JAN_3RD
        second_link_from_date = JAN_4TH
        second_link_to_date = JAN_6TH
        input_charge_time_and_price = {
            JAN_1ST: Decimal("1.000000"),
            JAN_3RD: Decimal("2.000000"),
        }
        expected_charge_time_and_price = {
            JAN_1ST: Decimal("1.000000"),
            JAN_2ND: Decimal("1.000000"),
            JAN_4TH: Decimal("2.000000"),
            JAN_5TH: Decimal("2.000000"),
        }

        charge_link_metering_point_periods = (
            factory.create_charge_link_metering_point_periods(
                spark,
                [
                    factory.create_charge_link_metering_point_periods_row(
                        charge_type=e.ChargeType.SUBSCRIPTION,
                        from_date=first_link_from_date,
                        to_date=first_link_to_date,
                    ),
                    factory.create_charge_link_metering_point_periods_row(
                        charge_type=e.ChargeType.SUBSCRIPTION,
                        from_date=second_link_from_date,
                        to_date=second_link_to_date,
                    ),
                ],
            )
        )
        charge_master_data = factory.create_charge_master_data(
            spark,
            [
                factory.create_charge_master_data_row(
                    charge_type=e.ChargeType.SUBSCRIPTION,
                    from_date=first_link_from_date,
                    to_date=second_link_to_date,
                ),
            ],
        )
        charge_prices = factory.create_charge_prices(
            spark,
            [
                factory.create_charge_prices_row(
                    charge_time=time,
                    charge_price=price,
                    charge_type=e.ChargeType.SUBSCRIPTION,
                )
                for time, price in input_charge_time_and_price.items()
            ],
        )

        # Act
        actual_subscription = get_subscription_charges(
            charge_master_data,
            charge_prices,
            charge_link_metering_point_periods,
            time_zone=DEFAULT_TIME_ZONE,
        )

        # Assert
        actual_charge_times_and_price = {
            row[Colname.charge_time]: row[Colname.charge_price]
            for row in actual_subscription.orderBy(Colname.charge_time).collect()
        }
        assert actual_charge_times_and_price == expected_charge_time_and_price


class TestWhenDaylightSavingTimeChanges:
    @pytest.mark.parametrize(
        "from_date, to_date, expected_first_charge_time, expected_last_charge_time, expected_day_count",
        [
            (  # Start of daylight saving time
                datetime(2020, 2, 29, 23),
                datetime(2020, 3, 31, 22),
                datetime(2020, 2, 29, 23),
                datetime(2020, 3, 30, 22),
                31,
            ),
            (  # End of daylight saving time
                datetime(2020, 9, 30, 22),
                datetime(2020, 10, 31, 23),
                datetime(2020, 9, 30, 22),
                datetime(2020, 10, 30, 23),
                31,
            ),
        ],
    )
    def test__returns_result_with_expected_start_and_end_charge_time(
        self,
        spark: SparkSession,
        from_date: datetime,
        to_date: datetime,
        expected_first_charge_time: datetime,
        expected_last_charge_time: datetime,
        expected_day_count: int,
    ) -> None:
        # Arrange
        charge_link_metering_point_periods = (
            factory.create_charge_link_metering_point_periods(
                spark,
                [
                    factory.create_charge_link_metering_point_periods_row(
                        charge_type=e.ChargeType.SUBSCRIPTION,
                        from_date=from_date,
                        to_date=to_date,
                    ),
                ],
            )
        )
        charge_master_data = factory.create_charge_master_data(
            spark,
            [
                factory.create_charge_master_data_row(
                    charge_type=e.ChargeType.SUBSCRIPTION,
                    from_date=from_date,
                    to_date=to_date,
                ),
            ],
        )

        charge_prices = factory.create_charge_prices(
            spark,
            [
                factory.create_charge_prices_row(
                    charge_type=e.ChargeType.SUBSCRIPTION,
                    charge_time=from_date,
                ),
            ],
        )

        # Act
        actual_subscription = get_subscription_charges(
            charge_master_data,
            charge_prices,
            charge_link_metering_point_periods,
            DEFAULT_TIME_ZONE,
        )
        actual_subscription.show(100)

        # Assert
        assert (
            actual_subscription.collect()[0][Colname.charge_time]
            == expected_first_charge_time
        )
        assert (
            actual_subscription.collect()[-1][Colname.charge_time]
            == expected_last_charge_time
        )
        assert actual_subscription.count() == expected_day_count
