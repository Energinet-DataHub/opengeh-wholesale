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
from datetime import datetime
from decimal import Decimal

import pytest
from pyspark.sql import Row, SparkSession

import package.codelists as e
import tests.calculation.charges_factory as factory
from calculation.preparation.transformations import (
    prepared_metering_point_time_series_factory,
)
from package.calculation.preparation.data_structures.prepared_tariffs import (
    prepared_tariffs_schema,
)
from package.calculation.preparation.transformations import (
    get_prepared_tariffs,
)
from package.constants import Colname

DEFAULT_TIME_ZONE = "Europe/Copenhagen"
FEB_1ST = datetime(2020, 1, 31, 23)
FEB_2ND = datetime(2020, 2, 1, 23)
FEB_3RD = datetime(2020, 2, 2, 23)
FEB_4TH = datetime(2020, 2, 3, 23)


def _create_expected_prepared_tariffs_row(
    charge_key: str = f"{factory.DefaultValues.CHARGE_CODE}-{factory.DefaultValues.CHARGE_OWNER}-{e.ChargeType.TARIFF.value}",
    charge_code: str = factory.DefaultValues.CHARGE_CODE,
    charge_owner: str = factory.DefaultValues.CHARGE_OWNER,
    charge_tax: bool = factory.DefaultValues.CHARGE_TAX,
    resolution: e.ChargeResolution = e.ChargeResolution.HOUR,
    charge_time: datetime = factory.DefaultValues.CHARGE_TIME_HOUR_0,
    charge_price: Decimal = factory.DefaultValues.CHARGE_PRICE,
    metering_point_id: str = factory.DefaultValues.METERING_POINT_ID,
    energy_supplier_id: str = factory.DefaultValues.ENERGY_SUPPLIER_ID,
    metering_point_type: e.MeteringPointType = factory.DefaultValues.METERING_POINT_TYPE,
    settlement_method: e.SettlementMethod = factory.DefaultValues.SETTLEMENT_METHOD,
    grid_area: str = factory.DefaultValues.GRID_AREA,
    quantity: Decimal = factory.DefaultValues.QUANTITY,
    qualities=None,
) -> Row:
    if qualities is None:
        qualities = [
            e.ChargeQuality.ESTIMATED.value,
            e.ChargeQuality.CALCULATED.value,
        ]
    row = {
        Colname.charge_key: charge_key,
        Colname.charge_code: charge_code,
        Colname.charge_type: e.ChargeType.TARIFF.value,
        Colname.charge_owner: charge_owner,
        Colname.charge_tax: charge_tax,
        Colname.resolution: resolution.value,
        Colname.charge_time: charge_time,
        Colname.charge_price: charge_price,
        Colname.metering_point_id: metering_point_id,
        Colname.energy_supplier_id: energy_supplier_id,
        Colname.metering_point_type: metering_point_type.value,
        Colname.settlement_method: settlement_method.value,
        Colname.grid_area_code: grid_area,
        Colname.quantity: quantity,
        Colname.qualities: qualities,
    }
    return Row(**row)


@pytest.mark.parametrize(
    "charge_resolution", [e.ChargeResolution.HOUR, e.ChargeResolution.DAY]
)
def test__get_prepared_tariffs__filters_on_resolution(
    spark: SparkSession, charge_resolution: e.ChargeResolution
) -> None:
    """
    Only charges with the given resolution are accepted.
    """
    # Arrange
    time_series_rows = [factory.create_time_series_row()]
    charge_price_information_rows = [
        factory.create_charge_price_information_row(
            charge_code="code_hour",
            resolution=e.ChargeResolution.HOUR,
        ),
        factory.create_charge_price_information_row(
            charge_code="code_day",
            resolution=e.ChargeResolution.DAY,
        ),
    ]
    charge_prices_rows = [
        factory.create_charge_prices_row(
            charge_code="code_hour",
        ),
        factory.create_charge_prices_row(
            charge_code="code_day",
        ),
    ]
    charge_link_metering_points_rows = [
        factory.create_charge_link_metering_point_periods_row(charge_code="code_hour"),
        factory.create_charge_link_metering_point_periods_row(charge_code="code_day"),
    ]

    charge_link_metering_point_periods = (
        factory.create_charge_link_metering_point_periods(
            spark, charge_link_metering_points_rows
        )
    )
    time_series = prepared_metering_point_time_series_factory.create(
        spark, time_series_rows
    )
    charge_price_information = factory.create_charge_price_information(
        spark, charge_price_information_rows
    )
    charge_prices = factory.create_charge_prices(spark, charge_prices_rows)

    # Act
    actual = get_prepared_tariffs(
        time_series,
        charge_price_information,
        charge_prices,
        charge_link_metering_point_periods,
        charge_resolution,
        DEFAULT_TIME_ZONE,
    )

    # Assert
    assert actual.df.count() == 1
    assert actual.df.collect()[0][Colname.resolution] == charge_resolution.value


def test__get_prepared_tariffs__filters_on_tariff_charge_type(
    spark: SparkSession,
) -> None:
    """
    Only charges with the charge type TARIFF are accepted.
    """
    # Arrange
    charge_link_metering_points_rows = [
        factory.create_charge_link_metering_point_periods_row(
            charge_type=e.ChargeType.TARIFF,
        ),
        factory.create_charge_link_metering_point_periods_row(
            charge_type=e.ChargeType.FEE,
        ),
        factory.create_charge_link_metering_point_periods_row(
            charge_type=e.ChargeType.SUBSCRIPTION,
        ),
    ]
    time_series_rows = [factory.create_time_series_row()]
    charge_price_information_rows = [
        factory.create_charge_price_information_row(),
        factory.create_charge_price_information_row(
            charge_type=e.ChargeType.FEE, resolution=e.ChargeResolution.MONTH
        ),
        factory.create_charge_price_information_row(
            charge_type=e.ChargeType.SUBSCRIPTION, resolution=e.ChargeResolution.MONTH
        ),
    ]

    charge_prices_rows = [
        factory.create_charge_prices_row(),
        factory.create_charge_prices_row(charge_type=e.ChargeType.FEE),
        factory.create_charge_prices_row(charge_type=e.ChargeType.SUBSCRIPTION),
    ]

    charge_link_metering_point_periods = (
        factory.create_charge_link_metering_point_periods(
            spark, charge_link_metering_points_rows
        )
    )
    time_series = prepared_metering_point_time_series_factory.create(
        spark, time_series_rows
    )
    charge_price_information = factory.create_charge_price_information(
        spark, charge_price_information_rows
    )
    charge_prices = factory.create_charge_prices(spark, charge_prices_rows)

    # Act
    actual_tariff = get_prepared_tariffs(
        time_series,
        charge_price_information,
        charge_prices,
        charge_link_metering_point_periods,
        e.ChargeResolution.HOUR,
        DEFAULT_TIME_ZONE,
    )

    # Assert
    assert (
        actual_tariff.df.collect()[0][Colname.charge_type] == e.ChargeType.TARIFF.value
    )


@pytest.mark.parametrize(
    "from_date, to_date, expected_rows",
    [
        # charge time before metering point from date - not accepted
        (datetime(2019, 12, 31, 23), datetime(2020, 1, 1, 0), 0),
        # charge time equal to metering point from date - accepted
        (datetime(2020, 1, 1, 0), datetime(2020, 1, 1, 1), 1),
        # charge time between metering point from and to date - accepted
        (datetime(2020, 1, 1, 1), datetime(2020, 1, 1, 2), 1),
        # charge time equal to metering point to date - not accepted
        (datetime(2020, 1, 1, 2), datetime(2020, 1, 1, 3), 0),
    ],
)
def test__get_prepared_tariffs__only_accepts_charges_in_metering_point_period(
    spark: SparkSession, from_date: datetime, to_date: datetime, expected_rows: int
) -> None:
    """
    Only charges where charge time is greater than or equal to the metering point from
    date and less than the metering point to date are accepted.

                from_date                to_date
        |-----------|-----------|-----------|----------|
                    |------ _charge_time --|
    """
    # Arrange
    charge_link_metering_points_rows = [
        factory.create_charge_link_metering_point_periods_row(
            charge_type=e.ChargeType.TARIFF,
            from_date=datetime(2020, 1, 1, 0),
            to_date=datetime(2020, 1, 1, 2),
        )
    ]
    time_series_rows = [
        factory.create_time_series_row(observation_time=from_date),
    ]
    charge_price_information_rows = [
        factory.create_charge_price_information_row(
            from_date=from_date,
            to_date=to_date,
        )
    ]
    charge_prices_rows = [
        factory.create_charge_prices_row(
            charge_time=from_date,
        )
    ]

    charge_link_metering_point_periods = (
        factory.create_charge_link_metering_point_periods(
            spark, charge_link_metering_points_rows
        )
    )
    time_series = prepared_metering_point_time_series_factory.create(
        spark, time_series_rows
    )
    charge_price_information = factory.create_charge_price_information(
        spark, charge_price_information_rows
    )
    charge_prices = factory.create_charge_prices(spark, charge_prices_rows)

    # Act
    actual = get_prepared_tariffs(
        time_series,
        charge_price_information,
        charge_prices,
        charge_link_metering_point_periods,
        e.ChargeResolution.HOUR,
        DEFAULT_TIME_ZONE,
    )

    # Assert
    assert actual.df.count() == expected_rows


def test__get_prepared_tariffs__when_same_metering_point_and_resolution__sums_quantity(
    spark: SparkSession,
) -> None:
    """
    When there are multiple time series rows for the same metering point and resolution,
    with the same observation time, the quantity is summed.
    """
    # Arrange
    charge_link_metering_points_rows = [
        factory.create_charge_link_metering_point_periods_row(
            charge_type=e.ChargeType.TARIFF
        )
    ]
    time_series_rows = [
        factory.create_time_series_row(),
        factory.create_time_series_row(),
    ]
    charge_price_information_rows = [factory.create_charge_price_information_row()]
    charge_prices_rows = [factory.create_charge_prices_row()]

    charge_link_metering_point_periods = (
        factory.create_charge_link_metering_point_periods(
            spark, charge_link_metering_points_rows
        )
    )
    time_series = prepared_metering_point_time_series_factory.create(
        spark, time_series_rows
    )
    charge_price_information = factory.create_charge_price_information(
        spark, charge_price_information_rows
    )
    charge_prices = factory.create_charge_prices(spark, charge_prices_rows)

    # Act
    actual = get_prepared_tariffs(
        time_series,
        charge_price_information,
        charge_prices,
        charge_link_metering_point_periods,
        e.ChargeResolution.HOUR,
        DEFAULT_TIME_ZONE,
    )

    # Assert
    assert (
        actual.df.collect()[0][Colname.quantity] == 2 * factory.DefaultValues.QUANTITY
    )


def test__get_prepared_tariffs__when_no_matching_charge_resolution__returns_empty_tariffs(
    spark: SparkSession,
) -> None:
    # Arrange
    time_series_rows = [factory.create_time_series_row()]
    charge_price_information_rows = [
        factory.create_charge_price_information_row(resolution=e.ChargeResolution.DAY)
    ]
    charge_prices_rows = [factory.create_charge_prices_row()]
    charge_link_metering_points_rows = [
        factory.create_charge_link_metering_point_periods_row(
            charge_type=e.ChargeType.TARIFF
        )
    ]

    time_series = prepared_metering_point_time_series_factory.create(
        spark, time_series_rows
    )
    charge_link_metering_point_periods = (
        factory.create_charge_link_metering_point_periods(
            spark, charge_link_metering_points_rows
        )
    )
    charge_price_information = factory.create_charge_price_information(
        spark, charge_price_information_rows
    )
    charge_prices = factory.create_charge_prices(spark, charge_prices_rows)

    # Act
    actual = get_prepared_tariffs(
        time_series,
        charge_price_information,
        charge_prices,
        charge_link_metering_point_periods,
        e.ChargeResolution.HOUR,
        DEFAULT_TIME_ZONE,
    )

    # Assert
    assert actual.df.count() == 0


def test__get_prepared_tariffs__when_two_tariff_overlap__returns_both_tariffs(
    spark: SparkSession,
) -> None:
    # Arrange
    time_series_rows = [factory.create_time_series_row()]
    charge_price_information_rows = [
        factory.create_charge_price_information_row(charge_code="4000"),
        factory.create_charge_price_information_row(charge_code="3000"),
    ]
    charge_prices_rows = [
        factory.create_charge_prices_row(charge_code="4000"),
        factory.create_charge_prices_row(charge_code="3000"),
    ]
    charge_link_metering_points_rows = [
        factory.create_charge_link_metering_point_periods_row(
            charge_type=e.ChargeType.TARIFF, charge_code="4000"
        ),
        factory.create_charge_link_metering_point_periods_row(
            charge_type=e.ChargeType.TARIFF, charge_code="3000"
        ),
    ]

    time_series = prepared_metering_point_time_series_factory.create(
        spark, time_series_rows
    )
    charge_price_information = factory.create_charge_price_information(
        spark, charge_price_information_rows
    )
    charge_prices = factory.create_charge_prices(spark, charge_prices_rows)
    charge_link_metering_point_periods = (
        factory.create_charge_link_metering_point_periods(
            spark, charge_link_metering_points_rows
        )
    )

    # Act
    actual = get_prepared_tariffs(
        time_series,
        charge_price_information,
        charge_prices,
        charge_link_metering_point_periods,
        e.ChargeResolution.HOUR,
        DEFAULT_TIME_ZONE,
    )

    # Assert
    assert actual.df.count() == 2


@pytest.mark.parametrize(
    "charge_resolution", [e.ChargeResolution.HOUR, e.ChargeResolution.DAY]
)
def test__get_prepared_tariffs__when_tariff_stops_and_starts_on_same_day__returns_expected_quantities(
    spark: SparkSession, charge_resolution: e.ChargeResolution
) -> None:
    """
    When the tariff stops and starts on the same day, the resulting quantities should behave as if there were just one period that crossed that day
    """
    # Arrange
    quantity_feb_1st = Decimal(1)
    quantity_feb_2nd = Decimal(2)
    quantity_feb_3rd = Decimal(3)
    time_series_rows = [
        factory.create_time_series_row(
            observation_time=FEB_1ST, quantity=quantity_feb_1st
        ),
        factory.create_time_series_row(
            observation_time=FEB_2ND, quantity=quantity_feb_2nd
        ),
        factory.create_time_series_row(
            observation_time=FEB_3RD, quantity=quantity_feb_3rd
        ),
    ]
    charge_price_information_rows = [
        factory.create_charge_price_information_row(
            from_date=FEB_1ST, to_date=FEB_2ND, resolution=charge_resolution
        ),
        factory.create_charge_price_information_row(
            from_date=FEB_2ND, to_date=FEB_3RD, resolution=charge_resolution
        ),
    ]
    charge_prices_rows = [
        factory.create_charge_prices_row(charge_time=FEB_1ST),
    ]
    charge_link_metering_points_rows = [
        factory.create_charge_link_metering_point_periods_row(
            charge_type=e.ChargeType.TARIFF, from_date=FEB_1ST, to_date=FEB_3RD
        ),
    ]

    time_series = prepared_metering_point_time_series_factory.create(
        spark, time_series_rows
    )
    charge_price_information = factory.create_charge_price_information(
        spark, charge_price_information_rows
    )
    charge_prices = factory.create_charge_prices(spark, charge_prices_rows)
    charge_link_metering_point_periods = (
        factory.create_charge_link_metering_point_periods(
            spark, charge_link_metering_points_rows
        )
    )

    # Act
    actual = get_prepared_tariffs(
        time_series,
        charge_price_information,
        charge_prices,
        charge_link_metering_point_periods,
        charge_resolution,
        DEFAULT_TIME_ZONE,
    )

    # Assert
    assert actual.df.count() == 2
    actual_df = actual.df.orderBy(Colname.charge_time).collect()
    assert actual_df[0][Colname.charge_time] == FEB_1ST
    assert actual_df[0][Colname.quantity] == quantity_feb_1st
    assert actual_df[1][Colname.charge_time] == FEB_2ND
    assert actual_df[1][Colname.quantity] == quantity_feb_2nd


@pytest.mark.parametrize(
    "charge_resolution", [e.ChargeResolution.HOUR, e.ChargeResolution.DAY]
)
def test__get_prepared_tariffs__when_tariff_stops_for_one_day__returns_expected_quantities(
    spark: SparkSession, charge_resolution: e.ChargeResolution
) -> None:
    """
    When the tariff stops and then starts one day later, then there should not be any result on the missing day.
    """
    # Arrange
    quantity_feb_1st = Decimal(1)
    quantity_feb_3rd = Decimal(3)

    time_series_rows = [
        factory.create_time_series_row(
            observation_time=FEB_1ST, quantity=quantity_feb_1st
        ),
        factory.create_time_series_row(
            observation_time=FEB_3RD, quantity=quantity_feb_3rd
        ),
    ]
    charge_price_information_rows = [
        factory.create_charge_price_information_row(
            from_date=FEB_1ST, to_date=FEB_2ND, resolution=charge_resolution
        ),
        factory.create_charge_price_information_row(
            from_date=FEB_3RD, to_date=FEB_4TH, resolution=charge_resolution
        ),
    ]
    charge_prices_rows = [
        factory.create_charge_prices_row(charge_time=FEB_1ST),
    ]
    charge_link_metering_points_rows = [
        factory.create_charge_link_metering_point_periods_row(
            charge_type=e.ChargeType.TARIFF, from_date=FEB_1ST, to_date=FEB_2ND
        ),
        factory.create_charge_link_metering_point_periods_row(
            charge_type=e.ChargeType.TARIFF, from_date=FEB_3RD, to_date=FEB_4TH
        ),
    ]

    time_series = prepared_metering_point_time_series_factory.create(
        spark, time_series_rows
    )
    charge_price_information = factory.create_charge_price_information(
        spark, charge_price_information_rows
    )
    charge_prices = factory.create_charge_prices(spark, charge_prices_rows)
    charge_link_metering_point_periods = (
        factory.create_charge_link_metering_point_periods(
            spark, charge_link_metering_points_rows
        )
    )

    # Act
    actual = get_prepared_tariffs(
        time_series,
        charge_price_information,
        charge_prices,
        charge_link_metering_point_periods,
        charge_resolution,
        DEFAULT_TIME_ZONE,
    )

    # Assert
    assert actual.df.count() == 2
    actual_df = actual.df.orderBy(Colname.charge_time).collect()
    assert actual_df[0][Colname.charge_time] == FEB_1ST
    assert actual_df[0][Colname.quantity] == quantity_feb_1st
    assert actual_df[1][Colname.charge_time] == FEB_3RD
    assert actual_df[1][Colname.quantity] == quantity_feb_3rd


def test__get_prepared_tariffs__returns_expected_tariff_values(
    spark: SparkSession,
) -> None:
    # Arrange
    charge_link_metering_points_rows = [
        factory.create_charge_link_metering_point_periods_row()
    ]
    time_series_rows = [
        factory.create_time_series_row(quality=e.QuantityQuality.CALCULATED),
        factory.create_time_series_row(quality=e.QuantityQuality.ESTIMATED),
    ]
    charge_price_information_rows = [factory.create_charge_price_information_row()]
    charge_prices_rows = [factory.create_charge_prices_row()]

    charge_link_metering_point_periods = (
        factory.create_charge_link_metering_point_periods(
            spark, charge_link_metering_points_rows
        )
    )
    time_series = prepared_metering_point_time_series_factory.create(
        spark, time_series_rows
    )
    charge_price_information = factory.create_charge_price_information(
        spark, charge_price_information_rows
    )
    charge_prices = factory.create_charge_prices(spark, charge_prices_rows)
    expected_row = [
        _create_expected_prepared_tariffs_row(
            quantity=2 * factory.DefaultValues.QUANTITY
        )
    ]
    expected = spark.createDataFrame(expected_row, prepared_tariffs_schema)

    # Act
    actual = get_prepared_tariffs(
        time_series,
        charge_price_information,
        charge_prices,
        charge_link_metering_point_periods,
        e.ChargeResolution.HOUR,
        DEFAULT_TIME_ZONE,
    )

    # Assert
    assert actual.df.collect() == expected.collect()


@pytest.mark.parametrize(
    "charge_resolution, expected_rows, expected_quantity",
    [
        (
            e.ChargeResolution.HOUR,
            48,
            factory.DefaultValues.QUANTITY,
        ),
        (
            e.ChargeResolution.DAY,
            2,
            24 * factory.DefaultValues.QUANTITY,
        ),
    ],
)
def test__get_prepared_tariffs__when_charges_with_specific_charge_resolution_and_time_series_hour__returns_expected(
    spark: SparkSession,
    charge_resolution: e.ChargeResolution,
    expected_rows: int,
    expected_quantity,
) -> None:
    """
    Only charges where charge time is greater than or equal to the metering point from
    date and less than the metering point to date are accepted.
    """
    # Arrange
    charge_link_metering_points_rows = [
        factory.create_charge_link_metering_point_periods_row(
            from_date=datetime(2020, 1, 1, 23),
            to_date=datetime(2020, 1, 3, 23),
        )
    ]
    time_series_rows = []
    charge_price_information_rows = []
    charge_prices_rows = []
    charge_price_information_rows.append(
        factory.create_charge_price_information_row(
            resolution=charge_resolution,
            from_date=datetime(2020, 1, 1, 23),
            to_date=datetime(2020, 1, 3, 23),
        )
    )
    for day in range(1, 4):
        for hour in range(0, 24):
            time_series_rows.append(
                factory.create_time_series_row(
                    observation_time=datetime(2020, 1, day, hour)
                )
            )
            charge_prices_rows.append(
                factory.create_charge_prices_row(
                    charge_time=datetime(2020, 1, day, hour),
                )
            )

    charge_link_metering_point_periods = (
        factory.create_charge_link_metering_point_periods(
            spark, charge_link_metering_points_rows
        )
    )
    time_series = prepared_metering_point_time_series_factory.create(
        spark, time_series_rows
    )
    charge_price_information = factory.create_charge_price_information(
        spark, charge_price_information_rows
    )
    charge_prices = factory.create_charge_prices(spark, charge_prices_rows)

    # Act
    actual = get_prepared_tariffs(
        time_series,
        charge_price_information,
        charge_prices,
        charge_link_metering_point_periods,
        charge_resolution,
        DEFAULT_TIME_ZONE,
    )

    # Assert
    assert actual.df.count() == expected_rows
    assert actual.df.collect()[0][Colname.quantity] == expected_quantity


@pytest.mark.parametrize(
    "charge_resolution, expected_rows, expected_quantity",
    [
        (
            e.ChargeResolution.HOUR,
            48,
            4 * factory.DefaultValues.QUANTITY,
        ),
        (
            e.ChargeResolution.DAY,
            2,
            96 * factory.DefaultValues.QUANTITY,
        ),
    ],
)
def test__get_prepared_tariffs__when_specific_charge_resolution_and_time_series_quarter__returns_expected(
    spark: SparkSession,
    charge_resolution: e.ChargeResolution,
    expected_rows: int,
    expected_quantity: int,
) -> None:
    """
    Only charges where charge time is greater than or equal to the metering point from
    date and less than the metering point to date are accepted.
    """
    # Arrange
    charge_link_metering_points_rows = [
        factory.create_charge_link_metering_point_periods_row(
            from_date=datetime(2020, 1, 1, 23),
            to_date=datetime(2020, 1, 3, 23),
        )
    ]
    time_series_rows = []
    charge_price_information_rows = []
    charge_prices_rows = []
    charge_price_information_rows.append(
        factory.create_charge_price_information_row(
            resolution=charge_resolution,
            from_date=datetime(2020, 1, 1, 23),
            to_date=datetime(2020, 1, 3, 23),
        )
    )
    for day in range(1, 4):
        for hour in range(0, 24):
            for minute in range(0, 4):
                time_series_rows.append(
                    factory.create_time_series_row(
                        observation_time=datetime(2020, 1, day, hour, minute * 15)
                    )
                )
            charge_prices_rows.append(
                factory.create_charge_prices_row(
                    charge_time=datetime(2020, 1, day, hour),
                )
            )

    charge_link_metering_point_periods = (
        factory.create_charge_link_metering_point_periods(
            spark, charge_link_metering_points_rows
        )
    )
    time_series = prepared_metering_point_time_series_factory.create(
        spark, time_series_rows
    )
    charge_price_information = factory.create_charge_price_information(
        spark, charge_price_information_rows
    )
    charge_prices = factory.create_charge_prices(spark, charge_prices_rows)

    # Act
    actual = get_prepared_tariffs(
        time_series,
        charge_price_information,
        charge_prices,
        charge_link_metering_point_periods,
        charge_resolution,
        DEFAULT_TIME_ZONE,
    )

    # Assert
    assert actual.df.count() == expected_rows
    assert actual.df.collect()[0][Colname.quantity] == expected_quantity


@pytest.mark.parametrize(
    "date_time_1, date_time_2, expected_rows",
    [
        (
            datetime(2019, 12, 31, 23),
            datetime(2020, 1, 1, 23),
            2,
        ),
        (
            datetime(2019, 12, 31, 23),
            datetime(2020, 1, 2, 23),
            1,
        ),
        (
            datetime(2020, 1, 1, 23),
            datetime(2020, 1, 2, 23),
            1,
        ),
        (
            datetime(2019, 12, 30, 23),
            datetime(2020, 1, 3, 23),
            0,
        ),
    ],
)
def test__get_prepared_tariffs__per_day_only_accepts_time_series_and_change_times_within_metering_point_period(
    spark: SparkSession,
    date_time_1: datetime,
    date_time_2: datetime,
    expected_rows: int,
) -> None:
    """
    Only tariff charges where observation/charge time is greater than or equal to the
    metering point from date and less than the metering point to date are accepted.
    OC = Observation/Charge time, MMP = Metering Point Period

              31.12       01.01       01.02       01.03      01.04
    |-----------|-----------|-----------|-----------|----------|
    MPP                     |----------------------|
    ES1                     |----------|
    ES2                                 |----------|
    Test1                   OC          OC                          Expected: 2
    Test2                   OC                     OC               Expected: 1
    Test3                               OC         OC               Expected: 1
    Test4       OC                      OC                     OC   Expected: 0
    """
    # Arrange
    charge_link_metering_points_rows = [
        factory.create_charge_link_metering_point_periods_row(
            from_date=datetime(2019, 12, 31, 23), to_date=datetime(2020, 1, 1, 23)
        ),
        factory.create_charge_link_metering_point_periods_row(
            from_date=datetime(2020, 1, 1, 23),
            to_date=datetime(2020, 1, 2, 23),
            energy_supplier_id="123",
        ),
    ]
    time_series_rows = [
        factory.create_time_series_row(
            observation_time=date_time_1, quality=e.QuantityQuality.MISSING
        ),
        factory.create_time_series_row(observation_time=date_time_2),
    ]
    charge_price_information_rows = [
        factory.create_charge_price_information_row(
            resolution=e.ChargeResolution.DAY,
            from_date=datetime(2019, 12, 31, 23),
            to_date=datetime(2020, 1, 5, 23),
        ),
    ]
    charge_prices_rows = [
        factory.create_charge_prices_row(
            charge_time=date_time_1,
        ),
        factory.create_charge_prices_row(
            charge_time=date_time_2,
        ),
    ]

    charge_link_metering_point_periods = (
        factory.create_charge_link_metering_point_periods(
            spark, charge_link_metering_points_rows
        )
    )
    time_series = prepared_metering_point_time_series_factory.create(
        spark, time_series_rows
    )
    charge_price_information = factory.create_charge_price_information(
        spark, charge_price_information_rows
    )
    charge_prices = factory.create_charge_prices(spark, charge_prices_rows)
    # Act
    actual = get_prepared_tariffs(
        time_series,
        charge_price_information,
        charge_prices,
        charge_link_metering_point_periods,
        e.ChargeResolution.DAY,
        DEFAULT_TIME_ZONE,
    )

    # Assert
    assert actual.df.count() == expected_rows


def test__get_prepared_tariffs__can_handle_missing_charge_prices(
    spark: SparkSession,
) -> None:
    # Arrange
    time_series_rows = [
        factory.create_time_series_row(observation_time=datetime(2019, 12, 31, 23)),
        factory.create_time_series_row(observation_time=datetime(2020, 1, 1, 0)),
    ]
    charge_price_information_rows = [
        factory.create_charge_price_information_row(
            from_date=datetime(2019, 12, 31, 23), to_date=datetime(2020, 1, 1, 0)
        ),
    ]
    charge_prices_rows = [
        factory.create_charge_prices_row(charge_time=datetime(2019, 12, 31, 23)),
    ]
    charge_link_metering_points_rows = [
        factory.create_charge_link_metering_point_periods_row(
            charge_type=e.ChargeType.TARIFF,
        ),
    ]

    time_series = prepared_metering_point_time_series_factory.create(
        spark, time_series_rows
    )
    charge_price_information = factory.create_charge_price_information(
        spark, charge_price_information_rows
    )
    charge_prices = factory.create_charge_prices(spark, charge_prices_rows)
    charge_link_metering_point_periods = (
        factory.create_charge_link_metering_point_periods(
            spark, charge_link_metering_points_rows
        )
    )

    # Act
    actual = get_prepared_tariffs(
        time_series,
        charge_price_information,
        charge_prices,
        charge_link_metering_point_periods,
        e.ChargeResolution.HOUR,
        DEFAULT_TIME_ZONE,
    )

    actual_df = actual.df.orderBy(Colname.charge_time)

    # Assert
    assert actual_df.count() == 2
    assert (
        actual_df.collect()[0][Colname.charge_price]
        == factory.DefaultValues.CHARGE_PRICE
    )
    assert actual_df.collect()[1][Colname.charge_price] is None


def test__get_prepared_tariffs__can_handle_missing_all_charges_prices(
    spark: SparkSession,
) -> None:
    # Arrange
    time_series_rows = [
        factory.create_time_series_row(observation_time=datetime(2019, 12, 31, 23)),
        factory.create_time_series_row(observation_time=datetime(2020, 1, 1, 0)),
    ]
    charge_price_information_rows = [
        factory.create_charge_price_information_row(
            from_date=datetime(2019, 12, 31, 23), to_date=datetime(2020, 1, 1, 0)
        ),
    ]
    charge_link_metering_points_rows = [
        factory.create_charge_link_metering_point_periods_row(
            charge_type=e.ChargeType.TARIFF,
        ),
    ]

    time_series = prepared_metering_point_time_series_factory.create(
        spark, time_series_rows
    )
    charge_price_information = factory.create_charge_price_information(
        spark, charge_price_information_rows
    )
    charge_prices = factory.create_charge_prices(spark, [])
    charge_link_metering_point_periods = (
        factory.create_charge_link_metering_point_periods(
            spark, charge_link_metering_points_rows
        )
    )

    # Act
    actual = get_prepared_tariffs(
        time_series,
        charge_price_information,
        charge_prices,
        charge_link_metering_point_periods,
        e.ChargeResolution.HOUR,
        DEFAULT_TIME_ZONE,
    )

    # Assert
    assert actual.df.count() == 2
    assert actual.df.collect()[0][Colname.charge_price] is None
    assert actual.df.collect()[1][Colname.charge_price] is None


def test__get_prepared_tariffs__can_handle_missing_charge_links(
    spark: SparkSession,
) -> None:
    # Arrange
    time_series_rows = [
        factory.create_time_series_row(observation_time=datetime(2019, 12, 31, 23)),
        factory.create_time_series_row(observation_time=datetime(2020, 1, 1, 0)),
        factory.create_time_series_row(observation_time=datetime(2020, 1, 1, 1)),
    ]
    charge_price_information_rows = [
        factory.create_charge_price_information_row(
            from_date=datetime(2019, 12, 31, 23), to_date=datetime(2020, 1, 1, 1)
        ),
    ]
    charge_prices_rows = [
        factory.create_charge_prices_row(charge_time=datetime(2019, 12, 31, 23)),
        factory.create_charge_prices_row(charge_time=datetime(2020, 1, 1, 0)),
        factory.create_charge_prices_row(charge_time=datetime(2020, 1, 1, 1)),
    ]
    charge_link_metering_points_rows = [
        factory.create_charge_link_metering_point_periods_row(
            charge_type=e.ChargeType.TARIFF,
            from_date=datetime(2019, 12, 31, 23),
            to_date=datetime(2020, 1, 1, 1),
        ),
    ]

    time_series = prepared_metering_point_time_series_factory.create(
        spark, time_series_rows
    )
    charge_price_information = factory.create_charge_price_information(
        spark, charge_price_information_rows
    )
    charge_prices = factory.create_charge_prices(spark, charge_prices_rows)
    charge_link_metering_point_periods = (
        factory.create_charge_link_metering_point_periods(
            spark, charge_link_metering_points_rows
        )
    )

    # Act
    actual = get_prepared_tariffs(
        time_series,
        charge_price_information,
        charge_prices,
        charge_link_metering_point_periods,
        e.ChargeResolution.HOUR,
        DEFAULT_TIME_ZONE,
    )

    # Assert
    assert actual.df.count() == 2


@pytest.mark.parametrize(
    "date_time_1, date_time_2",
    [
        (
            datetime(2020, 3, 28, 23),
            datetime(2020, 3, 29, 22),
        ),
        (
            datetime(2020, 10, 24, 22),
            datetime(2020, 10, 25, 23),
        ),
    ],
)
def test__get_prepared_tariffs__can_handle_daylight_saving_time(
    date_time_1: datetime,
    date_time_2: datetime,
    spark: SparkSession,
) -> None:
    # Arrange
    time_series_rows = [
        factory.create_time_series_row(observation_time=date_time_1),
        factory.create_time_series_row(observation_time=date_time_2),
    ]
    charge_price_information_rows = [
        factory.create_charge_price_information_row(
            from_date=date_time_1,
            to_date=date_time_2,
            resolution=e.ChargeResolution.DAY,
        ),
    ]
    charge_prices_rows = [
        factory.create_charge_prices_row(
            charge_time=date_time_1,
        ),
        factory.create_charge_prices_row(
            charge_time=date_time_2,
        ),
    ]
    charge_link_metering_points_rows = [
        factory.create_charge_link_metering_point_periods_row(
            charge_type=e.ChargeType.TARIFF,
            to_date=datetime(2020, 12, 31, 23),
        ),
    ]

    time_series = prepared_metering_point_time_series_factory.create(
        spark, time_series_rows
    )
    charge_price_information = factory.create_charge_price_information(
        spark, charge_price_information_rows
    )
    charge_prices = factory.create_charge_prices(spark, charge_prices_rows)
    charge_link_metering_point_periods = (
        factory.create_charge_link_metering_point_periods(
            spark, charge_link_metering_points_rows
        )
    )

    # Act
    actual = get_prepared_tariffs(
        time_series,
        charge_price_information,
        charge_prices,
        charge_link_metering_point_periods,
        e.ChargeResolution.DAY,
        DEFAULT_TIME_ZONE,
    )

    # Assert
    assert actual.df.count() == 2
