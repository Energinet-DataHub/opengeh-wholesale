from datetime import datetime, timezone
from decimal import Decimal
from zoneinfo import ZoneInfo

import pytest
from pyspark.sql import SparkSession
from pyspark.sql import functions as f

import geh_wholesale.calculation.preparation.data_structures as d
import geh_wholesale.codelists as e
import tests.calculation.charges_factory as factory
from geh_wholesale.calculation.preparation.transformations import (
    get_prepared_subscriptions,
)
from geh_wholesale.constants import Colname

DEFAULT_TIME_ZONE = "Europe/Copenhagen"
JAN_1ST = datetime(2021, 12, 31, 23)
JAN_2ND = datetime(2022, 1, 1, 23)
JAN_3RD = datetime(2022, 1, 2, 23)
JAN_4TH = datetime(2022, 1, 3, 23)
JAN_5TH = datetime(2022, 1, 4, 23)
JAN_6TH = datetime(2022, 1, 5, 23)
FEB_1ST = datetime(2022, 1, 31, 23)
DEFAULT_DAYS_IN_MONTH = 31


def _create_default_charge_price_information(
    spark: SparkSession, from_date: datetime = JAN_1ST, to_date=FEB_1ST
) -> d.ChargePriceInformation:
    return factory.create_charge_price_information(
        spark,
        [
            factory.create_charge_price_information_row(
                charge_type=e.ChargeType.SUBSCRIPTION,
                resolution=e.ChargeResolution.MONTH,
                from_date=from_date,
                to_date=to_date,
            ),
        ],
    )


def _create_charge_price(spark: SparkSession, charge_time: datetime, charge_price: Decimal) -> d.ChargePrices:
    return factory.create_charge_prices(
        spark,
        [
            factory.create_charge_prices_row(
                charge_type=e.ChargeType.SUBSCRIPTION,
                charge_time=charge_time,
                charge_price=charge_price,
            ),
        ],
    )


def _create_charge_link_metering_point_periods(
    spark: SparkSession,
    from_date: datetime = JAN_1ST,
    to_date: datetime = FEB_1ST,
) -> d.ChargeLinkMeteringPointPeriods:
    charge_link_metering_points_rows = [
        factory.create_charge_link_metering_point_periods_row(
            charge_type=e.ChargeType.SUBSCRIPTION,
            from_date=from_date,
            to_date=to_date,
        ),
    ]

    return factory.create_charge_link_metering_point_periods(spark, charge_link_metering_points_rows)


class TestWhenValidInput:
    @pytest.mark.parametrize(
        ("charge_time", "from_date", "to_date", "expected_day_count"),
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
        charge_price_information = _create_default_charge_price_information(spark, from_date, to_date)
        charge_prices = _create_charge_price(spark, charge_time, Decimal("1.123456"))
        charge_link_metering_point_periods = _create_charge_link_metering_point_periods(spark, from_date, to_date)

        # Act
        actual_subscription = get_prepared_subscriptions(
            charge_price_information,
            charge_prices,
            charge_link_metering_point_periods,
            DEFAULT_TIME_ZONE,
        )

        # Assert
        assert actual_subscription.df.count() == expected_day_count


class TestWhenNoPricesForPeriod:
    def test__returns_rows_with_price_equal_none(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange
        charge_price_information = _create_default_charge_price_information(spark)
        charge_link_metering_point_periods = _create_charge_link_metering_point_periods(spark)
        charge_prices_empty = factory.create_charge_prices(spark, [])

        # Act
        actual_subscription = get_prepared_subscriptions(
            charge_price_information,
            charge_prices_empty,
            charge_link_metering_point_periods,
            time_zone=DEFAULT_TIME_ZONE,
        )

        # Assert
        actual_prices = actual_subscription.df.select(Colname.charge_price)
        assert actual_prices.count() == DEFAULT_DAYS_IN_MONTH
        assert actual_prices.filter(f.col(Colname.charge_price).isNotNull()).count() == 0


class TestWhenInputContainsOtherChargeTypes:
    def test__returns_only_subscription_charge_type(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange
        charge_price_information = factory.create_charge_price_information(
            spark,
            [
                factory.create_charge_price_information_row(charge_type=e.ChargeType.FEE),
                factory.create_charge_price_information_row(charge_type=e.ChargeType.SUBSCRIPTION),
                factory.create_charge_price_information_row(charge_type=e.ChargeType.TARIFF),
            ],
        )

        charge_link_metering_point_periods = factory.create_charge_link_metering_point_periods(
            spark,
            [
                factory.create_charge_link_metering_point_periods_row(charge_type=e.ChargeType.FEE),
                factory.create_charge_link_metering_point_periods_row(charge_type=e.ChargeType.SUBSCRIPTION),
                factory.create_charge_link_metering_point_periods_row(charge_type=e.ChargeType.TARIFF),
            ],
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
        actual_subscription = get_prepared_subscriptions(
            charge_price_information,
            charge_prices,
            charge_link_metering_point_periods,
            time_zone=DEFAULT_TIME_ZONE,
        )

        # Assert
        assert actual_subscription.df.collect()[0][Colname.charge_type] == e.ChargeType.SUBSCRIPTION.value


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
        charge_link_metering_point_periods = _create_charge_link_metering_point_periods(spark, from_date, to_date)
        charge_price_information = _create_default_charge_price_information(spark)

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
        actual_subscription = get_prepared_subscriptions(
            charge_price_information,
            charge_prices,
            charge_link_metering_point_periods,
            time_zone=DEFAULT_TIME_ZONE,
        )

        # Assert
        actual_charge_times_and_price = {
            row[Colname.charge_time]: row[Colname.charge_price]
            for row in actual_subscription.df.orderBy(Colname.charge_time).collect()
        }
        assert actual_charge_times_and_price == expected_charge_time_and_price


class TestWhenChargePriceInformationPeriodStopsAndStartsAgain:
    def test__when_charge_resumes_after_one__returns_expected_charge_times(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange
        first_period_from_date = JAN_1ST
        first_period_to_date = JAN_3RD
        second_period_from_date = JAN_4TH
        second_period_to_date = JAN_6TH
        expected_charge_times = [
            JAN_1ST,
            JAN_2ND,
            JAN_4TH,
            JAN_5TH,
        ]

        charge_price_information = factory.create_charge_price_information(
            spark,
            [
                factory.create_charge_price_information_row(
                    charge_type=e.ChargeType.SUBSCRIPTION,
                    from_date=first_period_from_date,
                    to_date=first_period_to_date,
                ),
                factory.create_charge_price_information_row(
                    charge_type=e.ChargeType.SUBSCRIPTION,
                    from_date=second_period_from_date,
                    to_date=second_period_to_date,
                ),
            ],
        )
        charge_link_metering_point_periods = factory.create_charge_link_metering_point_periods(
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
        actual_subscription = get_prepared_subscriptions(
            charge_price_information,
            charge_period_prices,
            charge_link_metering_point_periods,
            time_zone=DEFAULT_TIME_ZONE,
        )

        # Assert
        actual_charge_times = set(row[0] for row in actual_subscription.df.select(Colname.charge_time).collect())
        expected_charge_times_set = set(expected_charge_times)

        assert actual_charge_times == expected_charge_times_set

    def test__when_charge_resumes_immediately__returns_expected_charge_times(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange
        first_period_from_date = JAN_1ST
        first_period_to_date = JAN_3RD
        second_period_from_date = JAN_3RD
        second_period_to_date = JAN_5TH
        expected_charge_times = [
            JAN_1ST,
            JAN_2ND,
            JAN_3RD,
            JAN_4TH,
        ]
        expected_length = len(expected_charge_times)

        charge_price_information = factory.create_charge_price_information(
            spark,
            [
                factory.create_charge_price_information_row(
                    charge_type=e.ChargeType.SUBSCRIPTION,
                    from_date=first_period_from_date,
                    to_date=first_period_to_date,
                ),
                factory.create_charge_price_information_row(
                    charge_type=e.ChargeType.SUBSCRIPTION,
                    from_date=second_period_from_date,
                    to_date=second_period_to_date,
                ),
            ],
        )
        charge_link_metering_point_periods = factory.create_charge_link_metering_point_periods(
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
        actual = get_prepared_subscriptions(
            charge_price_information,
            charge_period_prices,
            charge_link_metering_point_periods,
            time_zone=DEFAULT_TIME_ZONE,
        )

        # Assert
        actual_rows = actual.df.orderBy(Colname.charge_time).collect()
        assert len(actual_rows) == expected_length
        for i in range(expected_length):
            assert actual_rows[i][Colname.charge_time] == expected_charge_times[i]


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

        charge_price_information = _create_default_charge_price_information(spark)

        charge_link_metering_point_periods = factory.create_charge_link_metering_point_periods(
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
        actual_subscription = get_prepared_subscriptions(
            charge_price_information,
            charge_prices,
            charge_link_metering_point_periods,
            time_zone=DEFAULT_TIME_ZONE,
        )

        # Assert
        actual_charge_times_and_price = {
            row[Colname.charge_time]: row[Colname.charge_price]
            for row in actual_subscription.df.orderBy(Colname.charge_time).collect()
        }
        assert actual_charge_times_and_price == expected_charge_time_and_price


class TestWhenDaylightSavingTimeChanges:
    @pytest.mark.parametrize(
        ("from_date", "to_date", "expected_first_charge_time", "expected_last_charge_time", "expected_day_count"),
        [
            (  # Start of daylight saving time
                datetime(2020, 2, 29, 23, tzinfo=timezone.utc),
                datetime(2020, 3, 31, 22, tzinfo=timezone.utc),
                datetime(2020, 2, 29, 23, tzinfo=timezone.utc),
                datetime(2020, 3, 30, 22, tzinfo=timezone.utc),
                31,
            ),
            (  # End of daylight saving time
                datetime(2020, 9, 30, 22, tzinfo=timezone.utc),
                datetime(2020, 10, 31, 23, tzinfo=timezone.utc),
                datetime(2020, 9, 30, 22, tzinfo=timezone.utc),
                datetime(2020, 10, 30, 23, tzinfo=timezone.utc),
                31,
            ),
        ],
    )
    def test__returns_result_with_expected_first_and_last_charge_time(
        self,
        spark: SparkSession,
        from_date: datetime,
        to_date: datetime,
        expected_first_charge_time: datetime,
        expected_last_charge_time: datetime,
        expected_day_count: int,
    ) -> None:
        # Arrange
        charge_link_metering_point_periods = factory.create_charge_link_metering_point_periods(
            spark,
            [
                factory.create_charge_link_metering_point_periods_row(
                    charge_type=e.ChargeType.SUBSCRIPTION,
                    from_date=from_date,
                    to_date=to_date,
                ),
            ],
        )
        charge_price_information = factory.create_charge_price_information(
            spark,
            [
                factory.create_charge_price_information_row(
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
        actual_subscription = get_prepared_subscriptions(
            charge_price_information,
            charge_prices,
            charge_link_metering_point_periods,
            DEFAULT_TIME_ZONE,
        )

        # Assert
        actual_charge_times = actual_subscription.df.orderBy(Colname.charge_time).collect()

        assert actual_subscription.df.count() == expected_day_count
        assert actual_charge_times[0][Colname.charge_time].astimezone(
            ZoneInfo(DEFAULT_TIME_ZONE)
        ) == expected_first_charge_time.astimezone(ZoneInfo(DEFAULT_TIME_ZONE))
        assert actual_charge_times[expected_day_count - 1][Colname.charge_time].astimezone(
            ZoneInfo(DEFAULT_TIME_ZONE)
        ) == expected_last_charge_time.astimezone(ZoneInfo(DEFAULT_TIME_ZONE))
