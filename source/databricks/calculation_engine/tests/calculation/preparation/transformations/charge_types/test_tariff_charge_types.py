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

import pytest
from datetime import datetime, timedelta
from decimal import Decimal

import pytz
from pyspark.sql import SparkSession, Row
from pyspark.sql.types import TimestampType
from pyspark.sql.window import Window
import pyspark.sql.functions as f
from package.calculation.preparation.transformations import (
    get_tariff_charges,
)
import package.codelists as e
from package.calculation.wholesale.schemas.tariffs_schema import tariff_schema
from package.calculation_input.schemas import (
    time_series_point_schema,
    metering_point_period_schema,
)
from package.calculation.wholesale.schemas.charges_schema import charges_schema
from package.constants import Colname

DEFAULT_GRID_AREA = "543"
DEFAULT_CHARGE_CODE = "4000"
DEFAULT_CHARGE_OWNER = "001"
DEFAULT_CHARGE_TAX = True
DEFAULT_CHARGE_TIME_HOUR_0 = datetime(2019, 12, 31, 23)
DEFAULT_CHARGE_PRICE = Decimal("2.000005")
DEFAULT_ENERGY_SUPPLIER_ID = "1234567890123"
DEFAULT_METERING_POINT_ID = "123456789012345678901234567"
DEFAULT_METERING_POINT_TYPE = e.MeteringPointType.CONSUMPTION
DEFAULT_SETTLEMENT_METHOD = e.SettlementMethod.FLEX
DEFAULT_QUANTITY = Decimal("1.005")
DEFAULT_QUALITY = e.ChargeQuality.CALCULATED
DEFAULT_PERIOD_START_DATETIME = datetime(2019, 12, 31, 23)


def _create_metering_point_row(
    metering_point_id: str = DEFAULT_METERING_POINT_ID,
    metering_point_type: e.MeteringPointType = DEFAULT_METERING_POINT_TYPE,
    calculation_type: str = "calculation_type",
    settlement_method: e.SettlementMethod = DEFAULT_SETTLEMENT_METHOD,
    grid_area: str = DEFAULT_GRID_AREA,
    resolution: e.MeteringPointResolution = e.MeteringPointResolution.HOUR,
    from_grid_area: str = "from_grid_area",
    to_grid_area: str = "to_grid_area",
    parent_metering_point_id: str = "parent_metering_point_id",
    energy_supplier_id: str = DEFAULT_ENERGY_SUPPLIER_ID,
    balance_responsible_id: str = "balance_responsible_id",
    from_date: datetime = datetime(2019, 12, 31, 23),
    to_date: datetime = datetime(2020, 1, 31, 23),
) -> Row:
    row = {
        Colname.metering_point_id: metering_point_id,
        Colname.metering_point_type: metering_point_type.value,
        Colname.calculation_type: calculation_type,
        Colname.settlement_method: settlement_method.value,
        Colname.grid_area: grid_area,
        Colname.resolution: resolution.value,
        Colname.from_grid_area: from_grid_area,
        Colname.to_grid_area: to_grid_area,
        Colname.parent_metering_point_id: parent_metering_point_id,
        Colname.energy_supplier_id: energy_supplier_id,
        Colname.balance_responsible_id: balance_responsible_id,
        Colname.from_date: from_date,
        Colname.to_date: to_date,
    }
    return Row(**row)


def _create_time_series_row(
    metering_point_id: str = DEFAULT_METERING_POINT_ID,
    quantity: Decimal = DEFAULT_QUANTITY,
    quality: e.QuantityQuality = e.QuantityQuality.CALCULATED,
    observation_time: datetime = datetime(2019, 12, 31, 23),
) -> Row:
    row = {
        Colname.metering_point_id: metering_point_id,
        Colname.quantity: quantity,
        Colname.quality: quality.value,
        Colname.observation_time: observation_time,
    }
    return Row(**row)


def _create_tariff_charges_row(
    charge_code: str = DEFAULT_CHARGE_CODE,
    charge_owner: str = DEFAULT_CHARGE_OWNER,
    charge_tax: bool = DEFAULT_CHARGE_TAX,
    resolution: e.ChargeResolution = e.ChargeResolution.HOUR,
    charge_time: datetime = DEFAULT_CHARGE_TIME_HOUR_0,
    from_date: datetime = datetime(2019, 12, 31, 23),
    to_date: datetime = datetime(2020, 1, 1, 0),
    charge_price: Decimal = DEFAULT_CHARGE_PRICE,
    metering_point_id: str = DEFAULT_METERING_POINT_ID,
) -> Row:
    charge_key: str = f"{charge_code}-{charge_owner}-{e.ChargeType.TARIFF.value}"

    row = {
        Colname.charge_key: charge_key,
        Colname.charge_code: charge_code,
        Colname.charge_type: e.ChargeType.TARIFF.value,
        Colname.charge_owner: charge_owner,
        Colname.charge_tax: charge_tax,
        Colname.resolution: resolution.value,
        Colname.charge_time: charge_time,
        Colname.from_date: from_date,
        Colname.to_date: to_date,
        Colname.charge_price: charge_price,
        Colname.metering_point_id: metering_point_id,
    }
    return Row(**row)


def _create_expected_tariff_charges_row(
    charge_key: str = f"{DEFAULT_CHARGE_CODE}-{DEFAULT_CHARGE_OWNER}-{e.ChargeType.TARIFF.value}",
    charge_code: str = DEFAULT_CHARGE_CODE,
    charge_owner: str = DEFAULT_CHARGE_OWNER,
    charge_tax: bool = DEFAULT_CHARGE_TAX,
    resolution: e.ChargeResolution = e.ChargeResolution.HOUR,
    charge_time: datetime = DEFAULT_CHARGE_TIME_HOUR_0,
    charge_price: Decimal = DEFAULT_CHARGE_PRICE,
    metering_point_id: str = DEFAULT_METERING_POINT_ID,
    energy_supplier_id: str = DEFAULT_ENERGY_SUPPLIER_ID,
    metering_point_type: e.MeteringPointType = DEFAULT_METERING_POINT_TYPE,
    settlement_method: e.SettlementMethod = DEFAULT_SETTLEMENT_METHOD,
    grid_area: str = DEFAULT_GRID_AREA,
    quantity: Decimal = DEFAULT_QUANTITY,
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
        Colname.grid_area: grid_area,
        Colname.quantity: quantity,
        Colname.qualities: qualities,
    }
    return Row(**row)


@pytest.mark.parametrize(
    "charge_resolution", [e.ChargeResolution.HOUR, e.ChargeResolution.DAY]
)
def test__get_tariff_charges__filters_on_resolution(
    spark: SparkSession, charge_resolution: e.ChargeResolution
) -> None:
    """
    Only charges with the given resolution are accepted.
    """
    # Arrange
    metering_point_rows = [_create_metering_point_row()]
    time_series_rows = [_create_time_series_row()]
    charges_rows = [
        _create_tariff_charges_row(
            resolution=e.ChargeResolution.HOUR,
        ),
        _create_tariff_charges_row(
            resolution=e.ChargeResolution.DAY,
        ),
    ]

    metering_point = spark.createDataFrame(
        metering_point_rows, metering_point_period_schema
    )
    time_series = spark.createDataFrame(time_series_rows, time_series_point_schema)
    charges = spark.createDataFrame(charges_rows, charges_schema)

    # Act
    actual = get_tariff_charges(
        metering_point,
        time_series,
        charges,
        charge_resolution,
    )

    # Assert
    assert actual.count() == 1
    assert actual.collect()[0][Colname.resolution] == charge_resolution.value


def test__temp0(
    spark: SparkSession,
) -> None:
    # Define the data for the DataFrame
    period_start_utc = datetime(2020, 2, 29, 23, 0)
    period_end_utc = datetime(2020, 3, 31, 22, 0)

    # time_zone = "Europe/Copenhagen"
    # period_start_local_time = f.from_utc_timestamp(
    #     f.to_timestamp(f"{period_start}"), time_zone
    # ).cast("date")
    # period_end_local_time = f.from_utc_timestamp(
    #     f.to_timestamp("{period_end}"), time_zone
    # ).cast("date")

    # print(period_start_local_time)

    time_zone = "Europe/Copenhagen"
    local_tz = pytz.timezone(time_zone)

    data = [
        ("key1", period_start_utc, 100),
    ]

    # Define the schema for the DataFrame
    schema = ["charge_key", "charge_time", "charge_price"]

    # Create the DataFrame
    df = spark.createDataFrame(data, schema)

    df = df.withColumn(
        "local_time",
        f.explode(
            f.sequence(
                f.to_timestamp(
                    f.lit(
                        period_start_utc.astimezone(local_tz).strftime(
                            "%Y-%m-%d %H:%M:%S"
                        )
                    )
                ),
                f.to_timestamp(
                    f.lit(
                        period_end_utc.astimezone(local_tz).strftime(
                            "%Y-%m-%d %H:%M:%S"
                        )
                    )
                ),
                f.expr("INTERVAL 1 DAY"),
            )
        ),
    )

    df.show()


def test__temp1(
    spark: SparkSession,
) -> None:
    # Define the data for the DataFrame
    period_start_utc = datetime(2020, 2, 29, 23, 0)
    period_end_utc = datetime(2020, 3, 31, 22, 0)

    # time_zone = "Europe/Copenhagen"
    # period_start_local_time = f.from_utc_timestamp(
    #     f.to_timestamp(f"{period_start}"), time_zone
    # ).cast("date")
    # period_end_local_time = f.from_utc_timestamp(
    #     f.to_timestamp("{period_end}"), time_zone
    # ).cast("date")

    # print(period_start_local_time)

    time_zone = "Europe/Copenhagen"
    local_tz = pytz.timezone(time_zone)

    data = [
        ("key1", period_start_utc, 100),
    ]

    # Define the schema for the DataFrame
    schema = ["charge_key", "charge_time", "charge_price"]

    # Create the DataFrame
    df = spark.createDataFrame(data, schema)

    df = df.withColumn(
        "period_start_local",
        f.lit(period_start_utc.astimezone(local_tz)).cast(TimestampType()),
    ).withColumn(
        "period_start_end",
        f.lit(period_end_utc.astimezone(local_tz)).cast(TimestampType()),
    )

    df.show()


def test__temp(
    spark: SparkSession,
) -> None:
    # Prepare the data
    period_start_utc = datetime(2020, 2, 29, 23, 0)
    period_end_utc = datetime(2020, 3, 31, 22, 0)
    time_zone = "Europe/Copenhagen"

    data = [
        ("key1", period_start_utc, 100),
        ("key1", period_start_utc + timedelta(days=10), 200),
        ("key2", period_start_utc, 300),
    ]
    schema = ["charge_key", "charge_time", "charge_price"]
    df = spark.createDataFrame(data, schema)

    # Start the transformation

    local_tz = pytz.timezone(time_zone)
    period_start_local_time_str = period_start_utc.astimezone(local_tz).strftime(
        "%Y-%m-%d %H:%M:%S"
    )
    period_end_local_time_str = period_end_utc.astimezone(local_tz).strftime(
        "%Y-%m-%d %H:%M:%S"
    )

    all_dates_df = (
        df.select(Colname.charge_key)
        .dropDuplicates()
        .select(
            Colname.charge_key,
            f.explode(
                f.sequence(
                    f.to_timestamp(f.lit(period_start_local_time_str)),
                    f.to_timestamp(f.lit(period_end_local_time_str)),
                    f.expr("INTERVAL 1 DAY"),
                )
            ).alias("charge_time_local"),
        )
        .withColumn(
            "charge_time", f.to_utc_timestamp(f.col("charge_time_local"), time_zone)
        )
        .drop("charge_time_local")
    )

    all_dates_df.show(100)

    w = Window.partitionBy(Colname.charge_key).orderBy(Colname.charge_time)

    result = all_dates_df.join(
        df, [Colname.charge_key, Colname.charge_time], "left"
    ).select(
        Colname.charge_key,
        Colname.charge_time,
        *[
            f.last(f.col(c), ignorenulls=True).over(w).alias(c)
            for c in df.columns
            if c not in (Colname.charge_key, Colname.charge_time)
        ],
    )

    result.show(100)


def test__get_tariff_charges__filters_on_tariff_charge_type(
    spark: SparkSession,
) -> None:
    # Arrange
    metering_point_rows = [_create_metering_point_row()]
    time_series_rows = [_create_time_series_row()]
    charges_rows = [
        _create_tariff_charges_row(),
    ]

    metering_point = spark.createDataFrame(
        metering_point_rows, metering_point_period_schema
    )
    time_series = spark.createDataFrame(time_series_rows, time_series_point_schema)
    charges = spark.createDataFrame(charges_rows, charges_schema)

    # Act
    actual_tariff = get_tariff_charges(
        metering_point,
        time_series,
        charges,
        e.ChargeResolution.HOUR,
    )

    # Assert
    assert actual_tariff.collect()[0][Colname.charge_type] == e.ChargeType.TARIFF.value


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
def test__get_tariff_charges__only_accepts_charges_in_metering_point_period(
    spark: SparkSession, from_date: datetime, to_date: datetime, expected_rows: int
) -> None:
    """
    Only charges where charge time is greater than or equal to the metering point from date and
    less than the metering point to date are accepted.
    """
    # Arrange
    metering_point_rows = [
        _create_metering_point_row(
            from_date=datetime(2020, 1, 1, 0), to_date=datetime(2020, 1, 1, 2)
        ),
    ]
    time_series_rows = [
        _create_time_series_row(observation_time=from_date),
    ]
    charges_rows = [
        _create_tariff_charges_row(
            from_date=from_date,
            to_date=to_date,
            charge_time=from_date,
        )
    ]

    metering_point = spark.createDataFrame(
        metering_point_rows, metering_point_period_schema
    )
    time_series = spark.createDataFrame(time_series_rows, time_series_point_schema)
    charges = spark.createDataFrame(charges_rows, charges_schema)

    # Act
    actual = get_tariff_charges(
        metering_point,
        time_series,
        charges,
        e.ChargeResolution.HOUR,
    )

    # Assert
    assert actual.count() == expected_rows


def test__get_tariff_charges__when_same_metering_point_and_resolution__sums_quantity(
    spark: SparkSession,
) -> None:
    """
    When there are multiple time series rows for the same metering point and resolution,
    with the same observation time, the quantity is summed.
    """
    # Arrange
    metering_point_rows = [_create_metering_point_row()]
    time_series_rows = [_create_time_series_row(), _create_time_series_row()]
    charges_rows = [_create_tariff_charges_row()]

    metering_point = spark.createDataFrame(
        metering_point_rows, metering_point_period_schema
    )
    time_series = spark.createDataFrame(time_series_rows, time_series_point_schema)
    charges = spark.createDataFrame(charges_rows, charges_schema)

    # Act
    actual = get_tariff_charges(
        metering_point,
        time_series,
        charges,
        e.ChargeResolution.HOUR,
    )

    # Assert
    assert actual.collect()[0][Colname.sum_quantity] == 2 * DEFAULT_QUANTITY


def test__get_tariff_charges__when_no_matching_charge_resolution__returns_empty_tariffs(
    spark: SparkSession,
) -> None:
    # Arrange
    metering_point_rows = [_create_metering_point_row()]
    time_series_rows = [_create_time_series_row()]
    charges_rows = [_create_tariff_charges_row(resolution=e.ChargeResolution.DAY)]

    metering_point = spark.createDataFrame(
        metering_point_rows, metering_point_period_schema
    )
    time_series = spark.createDataFrame(time_series_rows, time_series_point_schema)
    charges = spark.createDataFrame(charges_rows, charges_schema)

    # Act
    actual = get_tariff_charges(
        metering_point,
        time_series,
        charges,
        e.ChargeResolution.HOUR,
    )

    # Assert
    assert actual.count() == 0


def test__get_tariff_charges__when_two_tariff_overlap__returns_both_tariffs(
    spark: SparkSession,
) -> None:
    # Arrange
    metering_point_rows = [_create_metering_point_row()]
    time_series_rows = [_create_time_series_row()]
    charges_rows = [
        _create_tariff_charges_row(charge_code="4000"),
        _create_tariff_charges_row(charge_code="3000"),
    ]

    metering_point = spark.createDataFrame(
        metering_point_rows, metering_point_period_schema
    )
    time_series = spark.createDataFrame(time_series_rows, time_series_point_schema)
    charges = spark.createDataFrame(charges_rows, charges_schema)

    # Act
    actual = get_tariff_charges(
        metering_point,
        time_series,
        charges,
        e.ChargeResolution.HOUR,
    )

    # Assert
    assert actual.count() == 2


def test__get_tariff_charges__returns_df_with_expected_values(
    spark: SparkSession,
) -> None:
    # Arrange
    metering_point_rows = [_create_metering_point_row()]
    time_series_rows = [
        _create_time_series_row(quality=e.QuantityQuality.CALCULATED),
        _create_time_series_row(quality=e.QuantityQuality.ESTIMATED),
    ]
    charges_rows = [_create_tariff_charges_row()]

    expected_tariff_charges_row = [
        _create_expected_tariff_charges_row(quantity=2 * DEFAULT_QUANTITY)
    ]

    metering_point = spark.createDataFrame(
        metering_point_rows, metering_point_period_schema
    )
    time_series = spark.createDataFrame(time_series_rows, time_series_point_schema)
    charges = spark.createDataFrame(charges_rows, charges_schema)

    expected_tariff_charges = spark.createDataFrame(
        expected_tariff_charges_row, tariff_schema
    )

    # Act
    actual = get_tariff_charges(
        metering_point,
        time_series,
        charges,
        e.ChargeResolution.HOUR,
    )

    # Assert
    assert actual.collect() == expected_tariff_charges.collect()


@pytest.mark.parametrize(
    "charge_resolution, expected_rows, expected_sum_quantity",
    [
        (
            e.ChargeResolution.HOUR,
            48,
            DEFAULT_QUANTITY,
        ),
        (
            e.ChargeResolution.DAY,
            2,
            24 * DEFAULT_QUANTITY,
        ),
    ],
)
def test__get_tariff_charges_with_specific_charge_resolution_and_time_series_hour_returns_expected(
    spark: SparkSession,
    charge_resolution: e.ChargeResolution,
    expected_rows: int,
    expected_sum_quantity: int,
) -> None:
    """
    Only charges where charge time is greater than or equal to the metering point from date and
    less than the metering point to date are accepted.
    """
    # Arrange
    metering_point_rows = [
        _create_metering_point_row(
            from_date=datetime(2020, 1, 1, 0),
            to_date=datetime(2020, 1, 3, 0),
            resolution=e.MeteringPointResolution.HOUR,
        )
    ]
    time_series_rows = []
    charges_rows = []
    for j in range(1, 4):
        for i in range(0, 24):
            time_series_rows.append(
                _create_time_series_row(observation_time=datetime(2020, 1, j, i))
            )
            charges_rows.append(
                _create_tariff_charges_row(
                    charge_time=datetime(2020, 1, j, i),
                    resolution=charge_resolution,
                )
            )

    metering_point = spark.createDataFrame(
        metering_point_rows, metering_point_period_schema
    )
    time_series = spark.createDataFrame(time_series_rows, time_series_point_schema)
    charges = spark.createDataFrame(charges_rows, charges_schema)

    # Act
    actual = get_tariff_charges(
        metering_point,
        time_series,
        charges,
        charge_resolution,
    )
    actual.show()
    # Assert
    assert actual.count() == expected_rows
    assert actual.collect()[0][Colname.sum_quantity] == expected_sum_quantity


@pytest.mark.parametrize(
    "charge_resolution, expected_rows, expected_sum_quantity",
    [
        (
            e.ChargeResolution.HOUR,
            48,
            4 * DEFAULT_QUANTITY,
        ),
        (
            e.ChargeResolution.DAY,
            2,
            96 * DEFAULT_QUANTITY,
        ),
    ],
)
def test__get_tariff_charges_with_specific_charge_resolution_and_time_series_quarter_returns_expected(
    spark: SparkSession,
    charge_resolution: e.ChargeResolution,
    expected_rows: int,
    expected_sum_quantity: int,
) -> None:
    """
    Only charges where charge time is greater than or equal to the metering point from date and
    less than the metering point to date are accepted.
    """
    # Arrange
    metering_point_rows = [
        _create_metering_point_row(
            from_date=datetime(2020, 1, 1, 0),
            to_date=datetime(2020, 1, 3, 0),
            resolution=e.MeteringPointResolution.QUARTER,
        )
    ]
    time_series_rows = []
    charges_rows = []
    for j in range(1, 4):
        for i in range(0, 24):
            for k in range(0, 4):
                time_series_rows.append(
                    _create_time_series_row(
                        observation_time=datetime(2020, 1, j, i, k * 15)
                    )
                )
            charges_rows.append(
                _create_tariff_charges_row(
                    charge_time=datetime(2020, 1, j, i),
                    resolution=charge_resolution,
                )
            )

    metering_point = spark.createDataFrame(
        metering_point_rows, metering_point_period_schema
    )
    time_series = spark.createDataFrame(time_series_rows, time_series_point_schema)
    charges = spark.createDataFrame(charges_rows, charges_schema)

    # Act
    actual = get_tariff_charges(
        metering_point,
        time_series,
        charges,
        charge_resolution,
    )
    actual.show()
    # Assert
    assert actual.count() == expected_rows
    assert actual.collect()[0][Colname.sum_quantity] == expected_sum_quantity
