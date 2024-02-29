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
    calculate_daily_subscription_amount,
    _add_count_of_charges_and_total_daily_charge_price,
)
from package.calculation.preparation.transformations import get_subscription_charges
from calendar import monthrange
import pytest
from package.constants import Colname

DEFAULT_TIME_ZONE = "Europe/Copenhagen"
DEFAULT_CALCULATION_PERIOD_START = datetime(2020, 1, 31, 23, 0)
DEFAULT_CALCULATION_PERIOD_END = datetime(2020, 2, 29, 23, 0)


class DefaultValues:
    GRID_AREA = "543"
    CHARGE_CODE = "4000"
    CHARGE_OWNER = "001"
    CHARGE_TIME_HOUR_0 = datetime(2019, 12, 31, 23)
    CHARGE_PRICE = Decimal("2.000005")
    CHARGE_QUANTITY = 1
    ENERGY_SUPPLIER_ID = "1234567890123"
    METERING_POINT_ID = "123456789012345678901234567"
    METERING_POINT_TYPE = e.MeteringPointType.CONSUMPTION
    SETTLEMENT_METHOD = e.SettlementMethod.FLEX
    QUANTITY = Decimal("1.005")
    QUALITY = e.ChargeQuality.CALCULATED
    PERIOD_START_DATETIME = datetime(2019, 12, 31, 23)
    BALANCE_RESPONSIBLE_ID = "1234567890123"
    FROM_GRID_AREA = None
    TO_GRID_AREA = None
    FROM_DATE: datetime = datetime(2019, 12, 31, 23)
    TO_DATE: datetime = datetime(2020, 1, 31, 23)
    PARENT_METERING_POINT_ID = None
    CALCULATION_TYPE = None


def _create_subscription_row(
    charge_key: str | None = None,
    charge_code: str = DefaultValues.CHARGE_CODE,
    charge_owner: str = DefaultValues.CHARGE_OWNER,
    charge_time: datetime = DefaultValues.CHARGE_TIME_HOUR_0,
    charge_price: Decimal | None = DefaultValues.CHARGE_PRICE,
    charge_quantity: int = DefaultValues.CHARGE_QUANTITY,
    energy_supplier_id: str = DefaultValues.ENERGY_SUPPLIER_ID,
    metering_point_type: MeteringPointType = DefaultValues.METERING_POINT_TYPE,
    grid_area: str = DefaultValues.GRID_AREA,
    quantity: Decimal = DEFAULT_QUANTITY,
    quality: Quality = DEFAULT_QUALITY,
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
        Colname.energy_supplier_id: energy_supplier_id,
        Colname.metering_point_type: metering_point_type.value,
        Colname.grid_area: grid_area,
        Colname.charge_quantity: quantity,
        Colname.qualities: [quality.value],
    }

    return Row(**row)


subscription_master_data_and_prices[Colname.charge_key],
subscription_master_data_and_prices[Colname.charge_type],
subscription_master_data_and_prices[Colname.charge_owner],
subscription_master_data_and_prices[Colname.charge_code],
subscription_master_data_and_prices[Colname.charge_time],
subscription_master_data_and_prices[Colname.charge_price],
subscription_master_data_and_prices[Colname.charge_tax],
subscription_links[Colname.charge_quantity],
subscription_links[Colname.metering_point_type],
subscription_links[Colname.settlement_method],
subscription_links[Colname.grid_area],
subscription_links[Colname.energy_supplier_id],


def test__calculate_daily_subscription_price__simple(
    spark,
    calculate_daily_subscription_price_factory,
    charge_master_data_factory,
    charge_prices_factory,
    charge_link_metering_points_factory,
):
    # Test that calculate_daily_subscription_price does as expected in with the most simple dataset
    # Arrange
    calculation_period_start = datetime(2020, 1, 31, 23, 0)
    calculation_period_end = datetime(2020, 2, 29, 23, 0)
    from_date = datetime(2020, 2, 1, 0, 0)
    to_date = datetime(2020, 2, 2, 0, 0)
    time = datetime(2020, 2, 1, 0, 0)
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

    expected_date = datetime(2020, 2, 1, 0, 0)
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
        DEFAULT_TIME_ZONE,
    )
    result = calculate_daily_subscription_amount(
        subscription_charges,
        calculation_period_start,
        calculation_period_end,
        DEFAULT_TIME_ZONE,
    )
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
    from_date = datetime(2020, 2, 1, 23, 0)
    to_date = datetime(2020, 2, 3, 23, 0)
    calculation_period_start = datetime(2020, 1, 31, 23, 0)
    calculation_period_end = datetime(2020, 2, 29, 23, 0)

    charge_link_metering_point_periods = charge_link_metering_points_factory(
        charge_type=ChargeType.SUBSCRIPTION.value,
        from_date=from_date,
        to_date=to_date,
    )
    charge_master_data_df = charge_master_data_factory(
        charge_type=ChargeType.SUBSCRIPTION.value,
        from_date=from_date,
        to_date=to_date,
    ).df

    subscription_1_charge_prices_charge_price = Decimal("3.124544")
    subscription_1_charge_prices_time = from_date
    subscription_1_charge_prices_df = charge_prices_factory(
        charge_time=subscription_1_charge_prices_time,
        charge_price=subscription_1_charge_prices_charge_price,
    ).df
    subscription_2_charge_prices_time = datetime(2020, 2, 2, 23, 0)
    subscription_2_charge_prices_df = charge_prices_factory(
        charge_time=subscription_2_charge_prices_time,
    ).df
    charge_prices_df = subscription_1_charge_prices_df.union(
        subscription_2_charge_prices_df
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
        DEFAULT_TIME_ZONE,
    )
    result = calculate_daily_subscription_amount(
        subscription_charges,
        calculation_period_start,
        calculation_period_end,
        DEFAULT_TIME_ZONE,
    ).orderBy(Colname.charge_time)

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
    from_date = datetime(2020, 2, 1, 23, 0)
    to_date = datetime(2020, 2, 3, 23, 0)
    calculation_period_start = datetime(2020, 1, 31, 23, 0)
    calculation_period_end = datetime(2020, 2, 29, 23, 0)
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
    subcription_2_charge_prices_time = datetime(2020, 2, 2, 23, 0)
    subcription_1_charge_prices_time = from_date

    subscription_1_charge_prices_df_with_charge_key_1 = charge_prices_factory(
        charge_time=subcription_1_charge_prices_time,
        charge_price=subscription_1_charge_prices_charge_price,
    ).df
    charge_master_data_df_with_charge_key_1 = charge_master_data_factory(
        from_date=from_date,
        to_date=to_date,
    ).df

    subscription_2_charge_prices_df_with_charge_key_1 = charge_prices_factory(
        charge_time=subcription_2_charge_prices_time,
    ).df
    charge_prices_df_with_charge_key_1 = (
        subscription_1_charge_prices_df_with_charge_key_1.union(
            subscription_2_charge_prices_df_with_charge_key_1
        )
    )

    subscription_1_charge_prices_df_with_charge_key_2 = charge_prices_factory(
        charge_time=subcription_1_charge_prices_time,
        charge_price=subscription_1_charge_prices_charge_price,
        charge_code=charge_code,
    ).df
    charge_master_data_df_with_charge_key_2 = charge_master_data_factory(
        charge_code=charge_code,
        from_date=from_date,
        to_date=to_date,
    ).df
    subscription_2_charge_prices_df_with_charge_key_2 = charge_prices_factory(
        charge_time=subcription_2_charge_prices_time,
        charge_code=charge_code,
    ).df
    charge_prices_df_with_charge_key_2 = (
        subscription_1_charge_prices_df_with_charge_key_2.union(
            subscription_2_charge_prices_df_with_charge_key_2
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
        DEFAULT_TIME_ZONE,
    )
    result = calculate_daily_subscription_amount(
        subscription_charges,
        calculation_period_start,
        calculation_period_end,
        DEFAULT_TIME_ZONE,
    ).orderBy(Colname.charge_time, Colname.charge_key)

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


charges_flex_consumption_dataset_1 = [
    (
        "001-D01-001",
        "001",
        "D01",
        "001",
        Decimal("100.10"),
        datetime(2020, 2, 1, 0, 0),
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
    result = calculate_daily_subscription_amount(
        charges_flex_consumption,
        DEFAULT_CALCULATION_PERIOD_START,
        DEFAULT_CALCULATION_PERIOD_END,
        DEFAULT_TIME_ZONE,
    )

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
def test__add_count_of_charges_and_total_daily_charge_price__counts_and_sums_up_amount_per_day(
    spark, charges_per_day, expected_charge_count, expected_total_daily_charge_price
):
    # Arrange
    charges_per_day = spark.createDataFrame(
        charges_per_day, schema=charges_per_day_schema
    )

    # Act
    result = _add_count_of_charges_and_total_daily_charge_price(charges_per_day)

    # Assert
    result_collect = result.collect()
    assert result_collect[0][Colname.charge_count] == expected_charge_count
    assert (
        result_collect[0][Colname.total_daily_charge_price]
        == expected_total_daily_charge_price
    )
