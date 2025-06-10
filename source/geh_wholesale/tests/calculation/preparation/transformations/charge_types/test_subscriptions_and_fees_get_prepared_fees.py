from datetime import datetime, timedelta
from decimal import Decimal

import pytest
from pyspark.sql import SparkSession

import geh_wholesale.calculation.preparation.data_structures as d
import geh_wholesale.codelists as e
import tests.calculation.charges_factory as factory
from geh_wholesale.calculation.preparation.transformations.charge_types import (
    get_prepared_fees,
)
from geh_wholesale.constants import Colname

DEFAULT_TIME_ZONE = "Europe/Copenhagen"

# Variables names below refer to local time
JAN_1ST = datetime(2021, 12, 31, 23)
JAN_2ND = datetime(2022, 1, 1, 23)
JAN_3RD = datetime(2022, 1, 2, 23)
JAN_4TH = datetime(2022, 1, 3, 23)
JAN_5TH = datetime(2022, 1, 4, 23)
FEB_1ST = datetime(2022, 1, 31, 23)


def _create_default_charge_price_information(
    spark: SparkSession,
) -> d.ChargePriceInformation:
    return factory.create_charge_price_information(
        spark,
        [
            factory.create_charge_price_information_row(
                charge_type=e.ChargeType.FEE,
                resolution=e.ChargeResolution.MONTH,
                from_date=JAN_1ST,
                to_date=FEB_1ST,
            ),
        ],
    )


def _create_charge_price(spark: SparkSession, charge_time: datetime, charge_price: Decimal) -> d.ChargePrices:
    return factory.create_charge_prices(
        spark,
        [
            factory.create_charge_prices_row(
                charge_type=e.ChargeType.FEE,
                charge_time=charge_time,
                charge_price=charge_price,
            ),
        ],
    )


def _create_charge_link_metering_point_periods(
    spark: SparkSession, charge_link_from_date: datetime
) -> d.ChargeLinkMeteringPointPeriods:
    charge_link_metering_points_rows = [
        factory.create_charge_link_metering_point_periods_row(
            charge_type=e.ChargeType.FEE,
            from_date=charge_link_from_date,
            to_date=charge_link_from_date + timedelta(days=1),
        ),
    ]

    return factory.create_charge_link_metering_point_periods(spark, charge_link_metering_points_rows)


class TestWhenChargeTimeIsWithinOrBeforeLinkPeriod:
    @pytest.mark.parametrize(
        ("charge_time", "charge_link_from_date"),
        [
            (JAN_1ST, JAN_1ST),  # charge time and link period overlap
            (
                JAN_3RD,
                JAN_3RD,
            ),  # charge time and link overlap, but not at first day of month
            (JAN_1ST, JAN_3RD),  # link starts after charge time
            (
                JAN_3RD,
                JAN_5TH,
            ),  # link starts after charge time, but not at first day of month
        ],
    )
    def test__returns_expected_price(
        self,
        spark: SparkSession,
        charge_time: datetime,
        charge_link_from_date: datetime,
    ) -> None:
        # Arrange
        charge_price = Decimal("1.123456")
        charge_price_information = _create_default_charge_price_information(spark)
        charge_prices = _create_charge_price(spark, charge_time, charge_price)
        charge_link_metering_point_periods = _create_charge_link_metering_point_periods(spark, charge_link_from_date)

        # Act
        actual = get_prepared_fees(
            charge_price_information,
            charge_prices,
            charge_link_metering_point_periods,
            DEFAULT_TIME_ZONE,
        )

        # Assert
        assert actual.df.count() == 1
        assert actual.df.collect()[0][Colname.charge_price] == charge_price


class TestWhenChargeTimeIsAfterLinkPeriod:
    def test__returns_resul_with_price_equals_none(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange
        charge_price = Decimal("1.123456")
        charge_price_information = _create_default_charge_price_information(spark)
        charge_prices = _create_charge_price(spark, JAN_3RD, charge_price)
        charge_link_metering_point_periods = _create_charge_link_metering_point_periods(spark, JAN_1ST)

        # Act
        actual = get_prepared_fees(
            charge_price_information,
            charge_prices,
            charge_link_metering_point_periods,
            DEFAULT_TIME_ZONE,
        )

        # Assert
        assert actual.df.count() == 1
        assert actual.df.collect()[0][Colname.charge_price] is None


class TestWhenHavingTwoLinksThatDoNotOverlapWithChargeTime:
    def test__returns_expected_price(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange
        from_date_link_1 = JAN_1ST
        from_date_link_2 = JAN_3RD
        metering_point_id_1 = "1"
        metering_point_id_2 = "2"
        charge_time = JAN_1ST
        charge_price = Decimal("1.123456")

        charge_price_information = _create_default_charge_price_information(spark)
        charge_prices = _create_charge_price(spark, charge_time, charge_price)
        charge_link_metering_points_rows = [
            factory.create_charge_link_metering_point_periods_row(
                charge_type=e.ChargeType.FEE,
                metering_point_id=metering_point_id_1,
                from_date=from_date_link_1,
                to_date=from_date_link_1 + timedelta(days=1),
            ),
            factory.create_charge_link_metering_point_periods_row(
                charge_type=e.ChargeType.FEE,
                metering_point_id=metering_point_id_2,
                from_date=from_date_link_2,
                to_date=from_date_link_2 + timedelta(days=1),
            ),
        ]

        charge_link_metering_point_periods = factory.create_charge_link_metering_point_periods(
            spark, charge_link_metering_points_rows
        )

        # Act
        actual = get_prepared_fees(
            charge_price_information,
            charge_prices,
            charge_link_metering_point_periods,
            DEFAULT_TIME_ZONE,
        )

        # Assert
        actual_df = actual.df.orderBy(Colname.charge_time)
        assert actual_df.count() == 2
        assert actual_df.collect()[0][Colname.charge_time] == from_date_link_1
        assert actual_df.collect()[0][Colname.charge_price] == charge_price
        assert actual_df.collect()[0][Colname.metering_point_id] == metering_point_id_1
        assert actual_df.collect()[1][Colname.charge_time] == from_date_link_2
        assert actual_df.collect()[1][Colname.charge_price] == charge_price
        assert actual_df.collect()[1][Colname.metering_point_id] == metering_point_id_2


class TestWhenValidInput:
    def test__filters_on_fee_charge_type(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange
        charge_link_metering_points_rows = [
            factory.create_charge_link_metering_point_periods_row(charge_type=e.ChargeType.FEE),
            factory.create_charge_link_metering_point_periods_row(charge_type=e.ChargeType.SUBSCRIPTION),
            factory.create_charge_link_metering_point_periods_row(charge_type=e.ChargeType.TARIFF),
        ]
        charge_price_information_rows = [
            factory.create_charge_price_information_row(
                charge_type=e.ChargeType.FEE, resolution=e.ChargeResolution.MONTH
            ),
            factory.create_charge_price_information_row(
                charge_type=e.ChargeType.SUBSCRIPTION,
                resolution=e.ChargeResolution.MONTH,
            ),
            factory.create_charge_price_information_row(),
        ]
        charge_prices_rows = [
            factory.create_charge_prices_row(
                charge_type=e.ChargeType.FEE,
            ),
            factory.create_charge_prices_row(
                charge_type=e.ChargeType.SUBSCRIPTION,
            ),
            factory.create_charge_prices_row(),
        ]

        charge_link_metering_point_periods = factory.create_charge_link_metering_point_periods(
            spark, charge_link_metering_points_rows
        )
        charge_price_information = factory.create_charge_price_information(spark, charge_price_information_rows)
        charge_prices = factory.create_charge_prices(spark, charge_prices_rows)

        # Act
        actual = get_prepared_fees(
            charge_price_information,
            charge_prices,
            charge_link_metering_point_periods,
            DEFAULT_TIME_ZONE,
        )

        # Assert
        assert actual.df.collect()[0][Colname.charge_type] == e.ChargeType.FEE.value
