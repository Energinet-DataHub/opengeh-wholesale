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

from package.calculation.preparation.charge_link_metering_point_periods import (
    ChargeLinkMeteringPointPeriods,
)
from package.calculation.preparation.charge_master_data import ChargeMasterData
from package.calculation.preparation.charge_prices import ChargePrices
from tests.helpers.test_schemas import (
    charges_flex_consumption_schema,
    charges_per_day_schema,
)

from package.codelists import MeteringPointType, SettlementMethod, ChargeType
from package.calculation.wholesale.subscription_calculators import (
    calculate_daily_subscription_price,
    calculate_price_per_day,
    filter_on_metering_point_type_and_settlement_method,
    get_count_of_charges_and_total_daily_charge_price,
)
from package.calculation.preparation.transformations import get_subscription_charges
from calendar import monthrange
import pytest
from package.constants import Colname


def test__calculate_daily_subscription_price__simple(
    spark,
    calculate_daily_subscription_price_factory,
    charge_master_data_factory,
    charge_prices_factory,
    charge_link_metering_points_factory,
):
    # Test that calculate_daily_subscription_price does as expected in with the most simple dataset
    # Arrange
    from_date = datetime(2020, 1, 1, 0, 0)
    to_date = datetime(2020, 1, 2, 0, 0)
    time = datetime(2020, 1, 1, 0, 0)
    charge_link_metering_point_periods = charge_link_metering_points_factory(
        charge_type=ChargeType.SUBSCRIPTION.value, from_date=from_date, to_date=to_date
    )
    charge_master_data = charge_master_data_factory(
        charge_type=ChargeType.SUBSCRIPTION.value,
        to_date=to_date,
        from_date=from_date,
    )
    charge_prices = charge_prices_factory(
        charge_type=ChargeType.SUBSCRIPTION.value,
        charge_time=time,
    )

    expected_date = datetime(2020, 1, 1, 0, 0)
    expected_charge_price = charge_prices.df.collect()[0][Colname.charge_price]
    expected_price_per_day = Decimal(
        expected_charge_price / monthrange(expected_date.year, expected_date.month)[1]
    )
    expected_subscription_count = 1

    # Act
    subscription_charges = get_subscription_charges(
        charge_master_data,
        charge_prices,
        charge_link_metering_point_periods,
    )
    result = calculate_daily_subscription_price(spark, subscription_charges)
    expected = calculate_daily_subscription_price_factory(
        expected_date,
        expected_price_per_day,
        expected_subscription_count,
        expected_price_per_day,
        charge_price=expected_charge_price,
    )

    # Assert
    assert result.collect() == expected.collect()


def test__calculate_daily_subscription_price__charge_price_change(
    spark,
    calculate_daily_subscription_price_factory,
    charge_master_data_factory,
    charge_prices_factory,
    charge_link_metering_points_factory,
):
    # Test that calculate_daily_subscription_price act as expected when charge price changes in a given period
    # Arrange
    from_date = datetime(2020, 1, 31, 0, 0)
    to_date = datetime(2020, 2, 2, 0, 0)

    charge_link_metering_point_periods = charge_link_metering_points_factory(
        charge_type=ChargeType.SUBSCRIPTION.value, from_date=from_date, to_date=to_date
    )

    subscription_1_charge_prices_charge_price = Decimal("3.124544")
    subscription_1_charge_prices_time = from_date
    subscription_1_charge_prices_df = charge_prices_factory(
        charge_time=subscription_1_charge_prices_time,
        charge_price=subscription_1_charge_prices_charge_price,
    ).df
    subscription_1_charge_master_data_df = charge_master_data_factory(
        from_date=from_date,
        to_date=to_date,
    ).df
    subscription_2_charge_prices_time = datetime(2020, 2, 1, 0, 0)
    subscription_2_charge_prices_df = charge_prices_factory(
        charge_time=subscription_2_charge_prices_time,
    ).df
    subscription_2_charge_master_data_df = charge_master_data_factory(
        from_date=from_date,
        to_date=to_date,
    ).df
    charge_prices_df = subscription_1_charge_prices_df.union(
        subscription_2_charge_prices_df
    )
    charge_master_data_df = subscription_1_charge_master_data_df.union(
        subscription_2_charge_master_data_df
    )

    expected_charge_price_subscription_1 = charge_prices_df.collect()[0][
        Colname.charge_price
    ]
    expected_price_per_day_subscription_1 = Decimal(
        expected_charge_price_subscription_1
        / monthrange(
            subscription_1_charge_prices_time.year,
            subscription_1_charge_prices_time.month,
        )[1]
    )
    expected_charge_price_subscription_2 = charge_prices_df.collect()[1][
        Colname.charge_price
    ]
    expected_price_per_day_subscription_2 = Decimal(
        expected_charge_price_subscription_2
        / monthrange(
            subscription_2_charge_prices_time.year,
            subscription_2_charge_prices_time.month,
        )[1]
    )
    expected_subscription_count = 1

    # Act
    subscription_charges = get_subscription_charges(
        ChargeMasterData(charge_master_data_df),
        ChargePrices(charge_prices_df),
        charge_link_metering_point_periods,
    )
    result = calculate_daily_subscription_price(spark, subscription_charges).orderBy(
        Colname.charge_time
    )

    expected_subscription_1 = calculate_daily_subscription_price_factory(
        subscription_1_charge_prices_time,
        expected_price_per_day_subscription_1,
        expected_subscription_count,
        expected_price_per_day_subscription_1,
        charge_price=expected_charge_price_subscription_1,
    )
    expected_subscription_2 = calculate_daily_subscription_price_factory(
        subscription_2_charge_prices_time,
        expected_price_per_day_subscription_2,
        expected_subscription_count,
        expected_price_per_day_subscription_2,
        charge_price=expected_charge_price_subscription_2,
    )
    expected = expected_subscription_1.union(expected_subscription_2)

    # Assert
    assert result.collect() == expected.collect()


def test__calculate_daily_subscription_price__charge_price_change_with_two_different_charge_key(
    spark,
    charge_master_data_factory,
    charge_prices_factory,
    charge_link_metering_points_factory,
    calculate_daily_subscription_price_factory,
):
    # Test that calculate_daily_subscription_price act as expected when charge price changes in a given period for two different charge keys
    # Arrange
    from_date = datetime(2020, 1, 31, 0, 0)
    to_date = datetime(2020, 2, 2, 0, 0)
    charge_code = "charge_code_b"
    charge_links_metering_point_periods_df = charge_link_metering_points_factory(
        from_date=from_date, to_date=to_date
    ).df
    charge_links_metering_point_periods_df = (
        charge_links_metering_point_periods_df.union(
            charge_link_metering_points_factory(
                from_date=from_date, to_date=to_date, charge_code=charge_code
            ).df
        )
    )
    charge_links_metering_point_periods = ChargeLinkMeteringPointPeriods(
        charge_links_metering_point_periods_df
    )

    subscription_1_charge_prices_charge_price = Decimal("3.124544")
    subcription_2_charge_prices_time = datetime(2020, 2, 1, 0, 0)
    subcription_1_charge_prices_time = from_date

    subscription_1_charge_prices_df_with_charge_key_1 = charge_prices_factory(
        charge_time=subcription_1_charge_prices_time,
        charge_price=subscription_1_charge_prices_charge_price,
    ).df
    subscription_1_charge_master_data_df_with_charge_key_1 = charge_master_data_factory(
        from_date=from_date,
        to_date=to_date,
    ).df
    subscription_2_charge_prices_df_with_charge_key_1 = charge_prices_factory(
        charge_time=subcription_2_charge_prices_time,
    ).df
    subscription_2_charge_master_data_df_with_charge_key_1 = charge_master_data_factory(
        from_date=from_date,
        to_date=to_date,
    ).df
    charge_prices_df_with_charge_key_1 = (
        subscription_1_charge_prices_df_with_charge_key_1.union(
            subscription_2_charge_prices_df_with_charge_key_1
        )
    )
    charge_master_data_df_with_charge_key_1 = (
        subscription_1_charge_master_data_df_with_charge_key_1.union(
            subscription_2_charge_master_data_df_with_charge_key_1
        )
    )

    subscription_1_charge_prices_df_with_charge_key_2 = charge_prices_factory(
        charge_time=subcription_1_charge_prices_time,
        charge_price=subscription_1_charge_prices_charge_price,
        charge_code=charge_code,
    ).df
    subscription_1_charge_master_data_df_with_charge_key_2 = charge_master_data_factory(
        charge_code=charge_code,
        from_date=from_date,
        to_date=to_date,
    ).df
    subscription_2_charge_prices_df_with_charge_key_2 = charge_prices_factory(
        charge_time=subcription_2_charge_prices_time,
        charge_code=charge_code,
    ).df
    subscription_2_charge_master_data_df_with_charge_key_2 = charge_master_data_factory(
        charge_code=charge_code,
        from_date=from_date,
        to_date=to_date,
    ).df
    charge_prices_df_with_charge_key_2 = (
        subscription_1_charge_prices_df_with_charge_key_2.union(
            subscription_2_charge_prices_df_with_charge_key_2
        )
    )
    charge_master_data_df_with_charge_key_2 = (
        subscription_1_charge_master_data_df_with_charge_key_2.union(
            subscription_2_charge_master_data_df_with_charge_key_2
        )
    )

    charge_prices_df = charge_prices_df_with_charge_key_1.union(
        charge_prices_df_with_charge_key_2
    )
    charge_master_data_df = charge_master_data_df_with_charge_key_1.union(
        charge_master_data_df_with_charge_key_2
    )

    # Act
    subscription_charges = get_subscription_charges(
        ChargeMasterData(charge_master_data_df),
        ChargePrices(charge_prices_df),
        charge_links_metering_point_periods,
    )
    result = calculate_daily_subscription_price(spark, subscription_charges).orderBy(
        Colname.charge_time, Colname.charge_key
    )

    expected_price_per_day_subscription_1 = Decimal(
        charge_prices_df.collect()[0][Colname.charge_price]
        / monthrange(
            subcription_1_charge_prices_time.year,
            subcription_1_charge_prices_time.month,
        )[1]
    )
    expected_price_per_day_subscription_2 = Decimal(
        charge_prices_df.collect()[1][Colname.charge_price]
        / monthrange(
            subcription_2_charge_prices_time.year,
            subcription_2_charge_prices_time.month,
        )[1]
    )
    expected_subscription_count = 2
    expected_subscription_1_with_charge_key_1 = (
        calculate_daily_subscription_price_factory(
            subcription_1_charge_prices_time,
            expected_price_per_day_subscription_1,
            expected_subscription_count,
            expected_price_per_day_subscription_1 * expected_subscription_count,
            charge_price=charge_prices_df.collect()[0][Colname.charge_price],
        )
    )
    expected_subscription_2_with_charge_key_1 = (
        calculate_daily_subscription_price_factory(
            subcription_2_charge_prices_time,
            expected_price_per_day_subscription_2,
            expected_subscription_count,
            expected_price_per_day_subscription_2 * expected_subscription_count,
            charge_price=charge_prices_df.collect()[1][Colname.charge_price],
        )
    )

    expected_subscription_1_with_charge_key_2 = (
        calculate_daily_subscription_price_factory(
            subcription_1_charge_prices_time,
            expected_price_per_day_subscription_1,
            expected_subscription_count,
            expected_price_per_day_subscription_1 * expected_subscription_count,
            charge_price=charge_prices_df.collect()[2][Colname.charge_price],
            charge_code=charge_code,
        )
    )
    expected_subscription_2_with_charge_key_2 = (
        calculate_daily_subscription_price_factory(
            subcription_2_charge_prices_time,
            expected_price_per_day_subscription_2,
            expected_subscription_count,
            expected_price_per_day_subscription_2 * expected_subscription_count,
            charge_price=charge_prices_df.collect()[3][Colname.charge_price],
            charge_code=charge_code,
        )
    )

    expected_1 = expected_subscription_1_with_charge_key_1.union(
        expected_subscription_2_with_charge_key_1
    )
    expected_2 = expected_subscription_1_with_charge_key_2.union(
        expected_subscription_2_with_charge_key_2
    )
    expected = expected_1.union(expected_2).orderBy(
        Colname.charge_time, Colname.charge_key
    )

    # Assert
    assert result.collect() == expected.collect()


subscription_charges_dataset_1 = [
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        Decimal("200.50"),
        datetime(2020, 1, 1, 0, 0),
        MeteringPointType.CONSUMPTION.value,
        SettlementMethod.FLEX.value,
        1,
        1,
    )
]
subscription_charges_dataset_2 = [
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        Decimal("200.50"),
        datetime(2020, 2, 1, 0, 0),
        MeteringPointType.PRODUCTION.value,
        SettlementMethod.FLEX.value,
        1,
        1,
    )
]
subscription_charges_dataset_3 = [
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        Decimal("200.50"),
        datetime(2020, 2, 1, 0, 0),
        MeteringPointType.CONSUMPTION.value,
        "001",
        1,
        1,
    )
]
subscription_charges_dataset_4 = [
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        Decimal("200.50"),
        datetime(2020, 2, 1, 0, 0),
        MeteringPointType.PRODUCTION.value,
        SettlementMethod.FLEX.value,
        1,
        1,
    )
]


@pytest.mark.parametrize(
    "subscription_charges,expected",
    [
        (subscription_charges_dataset_1, 1),
        (subscription_charges_dataset_2, 0),
        (subscription_charges_dataset_3, 0),
        (subscription_charges_dataset_4, 0),
    ],
)
def test__filter_on_metering_point_type_and_settlement_method__filters_on_consumption_and_flex(
    spark, subscription_charges, expected
):
    # Arrange
    subscription_charges = spark.createDataFrame(
        subscription_charges, schema=charges_flex_consumption_schema
    )  # subscription_charges and charges_flex_consumption has the same schema
    # Act
    result = filter_on_metering_point_type_and_settlement_method(subscription_charges)

    # Assert
    assert result.count() == expected


charges_flex_consumption_dataset_1 = [
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        Decimal("100.10"),
        datetime(2020, 1, 1, 0, 0),
        MeteringPointType.CONSUMPTION.value,
        SettlementMethod.FLEX.value,
        1,
        1,
    )
]
charges_flex_consumption_dataset_2 = [
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        Decimal("200.50"),
        datetime(2020, 2, 1, 0, 0),
        MeteringPointType.CONSUMPTION.value,
        SettlementMethod.FLEX.value,
        1,
        1,
    )
]


@pytest.mark.parametrize(
    "charges_flex_consumption,expected",
    [
        (charges_flex_consumption_dataset_1, Decimal("3.22903226")),
        (charges_flex_consumption_dataset_2, Decimal("6.91379310")),
    ],
)
def test__calculate_price_per_day__divides_charge_price_by_days_in_month(
    spark, charges_flex_consumption, expected
):
    # Arrange
    charges_flex_consumption = spark.createDataFrame(
        charges_flex_consumption, schema=charges_flex_consumption_schema
    )

    # Act
    result = calculate_price_per_day(charges_flex_consumption)

    # Assert
    assert result.collect()[0][Colname.price_per_day] == expected


charges_per_day_dataset_1 = [
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        Decimal("100.10"),
        datetime(2020, 1, 1, 0, 0),
        MeteringPointType.CONSUMPTION.value,
        SettlementMethod.FLEX.value,
        1,
        1,
        Decimal("3.22903226"),
    )
]
charges_per_day_dataset_2 = [
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        Decimal("100.10"),
        datetime(2020, 1, 1, 0, 0),
        MeteringPointType.CONSUMPTION.value,
        SettlementMethod.FLEX.value,
        1,
        1,
        Decimal("3.22903226"),
    ),
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        Decimal("100.10"),
        datetime(2020, 1, 1, 0, 0),
        MeteringPointType.CONSUMPTION.value,
        SettlementMethod.FLEX.value,
        1,
        1,
        Decimal("3.22903226"),
    ),
]
charges_per_day_dataset_3 = [
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        Decimal("100.10"),
        datetime(2020, 1, 1, 0, 0),
        MeteringPointType.CONSUMPTION.value,
        SettlementMethod.FLEX.value,
        1,
        1,
        Decimal("3.22903226"),
    ),
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        Decimal("100.10"),
        datetime(2020, 1, 2, 0, 0),
        MeteringPointType.CONSUMPTION.value,
        SettlementMethod.FLEX.value,
        1,
        1,
        Decimal("3.22903226"),
    ),
]
charges_per_day_dataset_4 = [
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        Decimal("100.10"),
        datetime(2020, 1, 1, 0, 0),
        MeteringPointType.CONSUMPTION.value,
        SettlementMethod.FLEX.value,
        1,
        1,
        Decimal("3.22903226"),
    ),
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        Decimal("100.10"),
        datetime(2021, 1, 1, 0, 0),
        MeteringPointType.CONSUMPTION.value,
        SettlementMethod.FLEX.value,
        1,
        1,
        Decimal("3.22903226"),
    ),
]


@pytest.mark.parametrize(
    "charges_per_day,expected_charge_count,expected_total_daily_charge_price",
    [
        (charges_per_day_dataset_1, 1, Decimal("3.22903226")),
        (charges_per_day_dataset_2, 2, Decimal("6.45806452")),
        (charges_per_day_dataset_3, 1, Decimal("3.22903226")),
        (charges_per_day_dataset_4, 1, Decimal("3.22903226")),
    ],
)
def test__get_count_of_charges_and_total_daily_charge_price__counts_and_sums_up_amount_per_day(
    spark, charges_per_day, expected_charge_count, expected_total_daily_charge_price
):
    # Arrange
    charges_per_day = spark.createDataFrame(
        charges_per_day, schema=charges_per_day_schema
    )

    # Act
    result = get_count_of_charges_and_total_daily_charge_price(charges_per_day)

    # Assert
    result_collect = result.collect()
    assert result_collect[0][Colname.charge_count] == expected_charge_count
    assert (
        result_collect[0][Colname.total_daily_charge_price]
        == expected_total_daily_charge_price
    )
