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
from datetime import timedelta, datetime
from decimal import Decimal

import pytest
from pyspark.sql import SparkSession

import tests.calculation.charges_factory as factory
from package.calculation.preparation.transformations.charge_types import (
    get_fee_charges,
)
import package.codelists as e
from package.constants import Colname

DEFAULT_TIME_ZONE = "Europe/Copenhagen"

# Variables names below refer to local time
JAN_1ST = datetime(2021, 12, 31, 23)
JAN_2ND = datetime(2022, 1, 1, 23)
JAN_3RD = datetime(2022, 1, 2, 23)
JAN_4TH = datetime(2022, 1, 3, 23)
JAN_5TH = datetime(2022, 1, 4, 23)
FEB_1ST = datetime(2022, 1, 31, 23)


class TestWhenOneLinkAndOnePrice:
    @pytest.mark.parametrize(
        "charge_time, charge_link_from_date",
        [
            (JAN_1ST, JAN_1ST),  # both price and link are at first day of month
            (JAN_1ST, JAN_3RD),  # only link is first day of month
            (JAN_3RD, JAN_1ST),  # price comes after link
            (JAN_3RD, JAN_3RD),  # on same day, but not at first day of month
            (JAN_3RD, JAN_5TH),  # prices after link and neither on first day of month
        ],
    )
    def test__returns_expected_price(
        self,
        spark: SparkSession,
        charge_link_from_date: datetime,
        charge_time: datetime,
    ) -> None:
        # Arrange
        charge_price = Decimal("1.123456")
        charge_link_metering_points_rows = [
            factory.create_charge_link_metering_point_periods_row(
                charge_type=e.ChargeType.FEE,
                from_date=charge_link_from_date,
                to_date=charge_link_from_date + timedelta(days=1),
            ),
        ]
        charge_master_data_rows = [
            factory.create_charge_master_data_row(
                charge_type=e.ChargeType.FEE,
                resolution=e.ChargeResolution.MONTH,
                from_date=JAN_1ST,
                to_date=FEB_1ST,
            ),
        ]
        charge_prices_rows = [
            factory.create_charge_prices_row(
                charge_type=e.ChargeType.FEE,
                charge_time=charge_time,
                charge_price=charge_price,
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
        actual = get_fee_charges(
            charge_master_data,
            charge_prices,
            charge_link_metering_point_periods,
            DEFAULT_TIME_ZONE,
        )

        # Assert
        assert actual.df.count() == 1
        assert actual.df.collect()[0][Colname.charge_price] == charge_price


class TestWhenTwoLinksAndOnePrice:
    def test__returns_expected_price(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange
        charge_time = JAN_1ST
        charge_price = Decimal("1.123456")
        from_date_link_1 = JAN_1ST
        from_date_link_2 = JAN_3RD
        metering_point_id_1 = "1"
        metering_point_id_2 = "2"
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
        charge_master_data_rows = [
            factory.create_charge_master_data_row(
                charge_type=e.ChargeType.FEE,
                resolution=e.ChargeResolution.MONTH,
                from_date=JAN_1ST,
                to_date=FEB_1ST,
            ),
        ]
        charge_prices_rows = [
            factory.create_charge_prices_row(
                charge_type=e.ChargeType.FEE,
                charge_time=charge_time,
                charge_price=charge_price,
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
        actual = get_fee_charges(
            charge_master_data,
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


def test__get_fee_charges__filters_on_fee_charge_type(
    spark: SparkSession,
) -> None:
    # Arrange
    charge_link_metering_points_rows = [
        factory.create_charge_link_metering_point_periods_row(
            charge_type=e.ChargeType.FEE
        ),
        factory.create_charge_link_metering_point_periods_row(
            charge_type=e.ChargeType.SUBSCRIPTION
        ),
        factory.create_charge_link_metering_point_periods_row(
            charge_type=e.ChargeType.TARIFF
        ),
    ]
    charge_master_data_rows = [
        factory.create_charge_master_data_row(
            charge_type=e.ChargeType.FEE, resolution=e.ChargeResolution.MONTH
        ),
        factory.create_charge_master_data_row(
            charge_type=e.ChargeType.SUBSCRIPTION, resolution=e.ChargeResolution.MONTH
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
        factory.create_charge_prices_row(),
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
    actual_fee = get_fee_charges(
        charge_master_data, charge_prices, charge_link_metering_point_periods
    )

    # Assert
    assert actual_fee.collect()[0][Colname.charge_type] == e.ChargeType.FEE.value
