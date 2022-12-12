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
from geh_stream.codelists import ChargeType
from tests.helpers.dataframe_creators.charges_creator import charges_factory, charge_links_factory, charge_prices_factory
from tests.helpers.dataframe_creators.metering_point_creator import metering_point_factory
from tests.helpers.dataframe_creators.market_roles_creator import market_roles_factory
from tests.helpers.dataframe_creators.calculate_fee_charge_price_creator import calculate_fee_charge_price_factory
from tests.helpers.test_schemas import charges_flex_settled_consumption_schema
from geh_stream.codelists import Colname
from geh_stream.wholesale_utils.calculators.fee_calculators import calculate_fee_charge_price, filter_on_metering_point_type_and_settlement_method, get_count_of_charges_and_total_daily_charge_price
from geh_stream.wholesale_utils.wholesale_initializer import get_fee_charges
import pytest
import pandas as pd


def test__calculate_fee_charge_price__simple(
    spark,
    charges_factory,
    charge_links_factory,
    charge_prices_factory,
    metering_point_factory,
    market_roles_factory,
    calculate_fee_charge_price_factory
):
    # Test that calculate_fee_charge_price does as expected in with the most simple dataset
    # Arrange
    from_date = datetime(2020, 1, 1, 0, 0)
    to_date = datetime(2020, 1, 2, 0, 0)
    time = datetime(2020, 1, 1, 0, 0)
    charges_df = charges_factory(from_date, to_date, charge_type=ChargeType.fee)
    charge_links_df = charge_links_factory(from_date, to_date)
    charge_prices_df = charge_prices_factory(time)
    metering_point_df = metering_point_factory(from_date, to_date)
    market_roles_df = market_roles_factory(from_date, to_date)

    expected_time = datetime(2020, 1, 1, 0, 0)
    expected_charge_price = charge_prices_df.collect()[0][Colname.charge_price]
    expected_total_daily_charge_price = expected_charge_price
    expected_charge_count = 1

    # Act
    fee_charges = get_fee_charges(charges_df, charge_prices_df, charge_links_df, metering_point_df, market_roles_df)
    result = calculate_fee_charge_price(spark, fee_charges)
    expected = calculate_fee_charge_price_factory(
        expected_time,
        expected_charge_count,
        expected_total_daily_charge_price,
        charge_price=expected_charge_price)

    # Assert
    assert result.collect() == expected.collect()


def test__calculate_fee_charge_price__two_fees(
    spark,
    charges_factory,
    charge_links_factory,
    charge_prices_factory,
    metering_point_factory,
    market_roles_factory,
    calculate_fee_charge_price_factory
):
    # Test that calculate_fee_charge_price does as expected with two fees on the same day
    # Arrange
    from_date = datetime(2020, 1, 1, 0, 0)
    to_date = datetime(2020, 1, 2, 0, 0)
    time = datetime(2020, 1, 1, 0, 0)
    charges_df = charges_factory(from_date, to_date, charge_type=ChargeType.fee)
    charge_links_df = charge_links_factory(from_date, to_date)
    metering_point_df = metering_point_factory(from_date, to_date)
    market_roles_df = market_roles_factory(from_date, to_date)

    fee_1_charge_prices_charge_price = Decimal("3.124544")
    fee_1_charge_prices_df = charge_prices_factory(time, charge_price=fee_1_charge_prices_charge_price)
    fee_2_charge_prices_df = charge_prices_factory(time)
    charge_prices_df = fee_1_charge_prices_df.union(fee_2_charge_prices_df)

    expected_time = datetime(2020, 1, 1, 0, 0)
    expected_charge_price_fee_1 = charge_prices_df.collect()[0][Colname.charge_price]
    expected_charge_price_fee_2 = charge_prices_df.collect()[1][Colname.charge_price]
    expected_total_daily_charge_price = expected_charge_price_fee_1 + expected_charge_price_fee_2
    expected_charge_count = 2

    # Act
    fee_charges = get_fee_charges(charges_df, charge_prices_df, charge_links_df, metering_point_df, market_roles_df)
    result = calculate_fee_charge_price(spark, fee_charges).orderBy(Colname.charge_price)
    expected_fee_1 = calculate_fee_charge_price_factory(
        expected_time,
        expected_charge_count,
        expected_total_daily_charge_price,
        charge_price=expected_charge_price_fee_1)
    expected_fee_2 = calculate_fee_charge_price_factory(
        expected_time,
        expected_charge_count,
        expected_total_daily_charge_price,
        charge_price=expected_charge_price_fee_2)
    expected = expected_fee_1.union(expected_fee_2).orderBy(Colname.charge_price)

    # Assert
    assert result.collect() == expected.collect()


fee_charges_dataset_1 = [("001-D01-001", "001", "D01", "001", Decimal("200.50"), datetime(2020, 1, 1, 0, 0), "E17", "D01", "001", 1, 1)]
fee_charges_dataset_2 = [("001-D01-001", "001", "D01", "001", Decimal("200.50"), datetime(2020, 2, 1, 0, 0), "E18", "D01", "001", 1, 1)]
fee_charges_dataset_3 = [("001-D01-001", "001", "D01", "001", Decimal("200.50"), datetime(2020, 2, 1, 0, 0), "E17", "D02", "001", 1, 1)]
fee_charges_dataset_4 = [("001-D01-001", "001", "D01", "001", Decimal("200.50"), datetime(2020, 2, 1, 0, 0), "E18", "D02", "001", 1, 1)]


@pytest.mark.parametrize("fee_charges,expected", [
    (fee_charges_dataset_1, 1),
    (fee_charges_dataset_2, 0),
    (fee_charges_dataset_3, 0),
    (fee_charges_dataset_4, 0)
])
def test__filter_on_metering_point_type_and_settlement_method__filters_on_E17_and_D01(spark, fee_charges, expected):
    # Arrange
    fee_charges = spark.createDataFrame(fee_charges, schema=charges_flex_settled_consumption_schema)  # fee_charges and charges_flex_settled_consumption has the same schema

    # Act
    result = filter_on_metering_point_type_and_settlement_method(fee_charges)

    # Assert
    assert result.count() == expected


charges_flex_settled_consumption_dataset_1 = [("001-D01-001", "001", "D01", "001", Decimal("100.10"), datetime(2020, 1, 1, 0, 0), "E17", "D01", "001", 1, 1)]
charges_flex_settled_consumption_dataset_2 = [("001-D01-001", "001", "D01", "001", Decimal("100.10"), datetime(2020, 1, 1, 0, 0), "E17", "D01", "001", 1, 1),
                                              ("001-D01-001", "001", "D01", "001", Decimal("100.10"), datetime(2020, 1, 1, 0, 0), "E17", "D01", "001", 1, 1)]
charges_flex_settled_consumption_dataset_3 = [("001-D01-001", "001", "D01", "001", Decimal("100.10"), datetime(2020, 1, 1, 0, 0), "E17", "D01", "001", 1, 1),
                                              ("001-D01-001", "001", "D01", "001", Decimal("100.10"), datetime(2020, 1, 2, 0, 0), "E17", "D01", "001", 1, 1)]


@pytest.mark.parametrize("charges_flex_settled_consumption,expected_charge_count,expected_total_daily_charge_price", [
    (charges_flex_settled_consumption_dataset_1, 1, Decimal("100.10")),
    (charges_flex_settled_consumption_dataset_2, 2, Decimal("200.20")),
    (charges_flex_settled_consumption_dataset_3, 1, Decimal("100.10"))
])
def test__get_count_of_charges_and_total_daily_charge_price__counts_and_sums_up_amount_per_day(
    spark,
    charges_flex_settled_consumption,
    expected_charge_count,
    expected_total_daily_charge_price
):
    # Arrange
    charges_flex_settled_consumption = spark.createDataFrame(charges_flex_settled_consumption, schema=charges_flex_settled_consumption_schema)

    # Act
    result = get_count_of_charges_and_total_daily_charge_price(charges_flex_settled_consumption)

    # Assert
    assert result.collect()[0][Colname.charge_count] == expected_charge_count
    assert result.collect()[0][Colname.total_daily_charge_price] == expected_total_daily_charge_price
