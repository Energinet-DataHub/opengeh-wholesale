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
from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.types import (
    DecimalType,
    StringType,
    StructField,
    StructType,
    TimestampType,
)
from package.calculation.preparation.transformations import (
    get_tariff_charges,
    get_fee_charges,
    get_subscription_charges,
)
from package.codelists import (
    ChargeQuality,
    ChargeType,
    ChargeResolution,
    MeteringPointType,
    SettlementMethod,
    MeteringPointResolution,
    TimeSeriesQuality,
)
from package.calculation_input.schemas import (
    time_series_point_schema,
    metering_point_period_schema,
)
import pytest
from package.constants import Colname

DEFAULT_GRID_AREA = "543"
DEFAULT_CHARGE_ID = "4000"
DEFAULT_CHARGE_OWNER = "001"
DEFAULT_CHARGE_TAX = True
DEFAULT_CHARGE_TIME_HOUR_0 = datetime(2020, 1, 1, 0)
DEFAULT_CHARGE_PRICE = Decimal("2.000005")
DEFAULT_ENERGY_SUPPLIER_ID = "1234567890123"
DEFAULT_METERING_POINT_ID = "123456789012345678901234567"
DEFAULT_METERING_POINT_TYPE = MeteringPointType.CONSUMPTION
DEFAULT_SETTLEMENT_METHOD = SettlementMethod.FLEX
DEFAULT_QUANTITY = Decimal("1.005")
DEFAULT_QUALITY = ChargeQuality.CALCULATED
DEFAULT_PERIOD_START_DATETIME = datetime(2019, 12, 31, 23)


def _create_metering_point_row(
    metering_point_id: str = DEFAULT_METERING_POINT_ID,
    metering_point_type: MeteringPointType = DEFAULT_METERING_POINT_TYPE,
    calculation_type: str = "calculation_type",
    settlement_method: str = "settlement_method",
    grid_area: str = "grid_area",
    resolution: MeteringPointResolution = MeteringPointResolution.HOUR,
    from_grid_area: str = "from_grid_area",
    to_grid_area: str = "to_grid_area",
    parent_metering_point_id: str = "parent_metering_point_id",
    energy_supplier_id: str = DEFAULT_ENERGY_SUPPLIER_ID,
    balance_responsible_id: str = "balance_responsible_id",
    from_date: datetime = datetime(2020, 1, 1, 0),
    to_date: datetime = datetime(2020, 2, 1, 0),
) -> dict:
    row = {
        Colname.metering_point_id: metering_point_id,
        Colname.metering_point_type: metering_point_type.value,
        Colname.calculation_type: calculation_type,
        Colname.settlement_method: settlement_method,
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
    return row


def _create_time_series_row(
    metering_point_id: str = DEFAULT_METERING_POINT_ID,
    quantity: Decimal = DEFAULT_QUANTITY,
    quality: TimeSeriesQuality = TimeSeriesQuality.CALCULATED,
    observation_time: datetime = datetime(2020, 1, 1, 0),
) -> dict:
    row = {
        Colname.metering_point_id: metering_point_id,
        Colname.quantity: quantity,
        Colname.quality: quality.value,
        Colname.observation_time: observation_time,
    }
    return row


def _create_charges_row(
    charge_key: str = f"{DEFAULT_CHARGE_ID}-{DEFAULT_CHARGE_OWNER}-{ChargeType.TARIFF.value}",
    charge_id: str = DEFAULT_CHARGE_ID,
    charge_type: ChargeType = ChargeType.TARIFF,
    charge_owner: str = DEFAULT_CHARGE_OWNER,
    charge_tax: bool = DEFAULT_CHARGE_TAX,
    charge_resolution: ChargeResolution = ChargeResolution.HOUR,
    charge_time: datetime = DEFAULT_CHARGE_TIME_HOUR_0,
    from_date: datetime = datetime(2020, 1, 1, 0),
    to_date: datetime = datetime(2020, 1, 1, 1),
    charge_price: Decimal = DEFAULT_CHARGE_PRICE,
    metering_point_id: str = DEFAULT_METERING_POINT_ID,
) -> dict:
    row = {
        Colname.charge_key: charge_key,
        Colname.charge_id: charge_id,
        Colname.charge_type: charge_type.value,
        Colname.charge_owner: charge_owner,
        Colname.charge_tax: charge_tax,
        Colname.charge_resolution: charge_resolution.value,
        Colname.charge_time: charge_time,
        Colname.from_date: from_date,
        Colname.to_date: to_date,
        Colname.charge_price: charge_price,
        Colname.metering_point_id: metering_point_id,
    }
    return row


def _create_dataframe_from_rows(
    spark: SparkSession, rows: list, schema: StructType
) -> DataFrame:
    return spark.createDataFrame(rows, schema=schema)


charges_schema = StructType(
    [
        StructField(Colname.charge_key, StringType(), False),
        StructField(Colname.charge_id, StringType(), False),
        StructField(Colname.charge_type, StringType(), False),
        StructField(Colname.charge_owner, StringType(), False),
        StructField(Colname.charge_tax, StringType(), False),
        StructField(Colname.charge_resolution, StringType(), False),
        StructField(Colname.charge_time, TimestampType(), False),
        StructField(Colname.from_date, TimestampType(), False),
        StructField(Colname.to_date, TimestampType(), False),
        StructField(Colname.charge_price, DecimalType(), False),
        StructField(Colname.metering_point_id, StringType(), False),
    ]
)


@pytest.mark.parametrize(
    "charge_resolution", [ChargeResolution.HOUR, ChargeResolution.DAY]
)
def test__get_tariff_charges__filters_on_resolution_hour_or_day(
    spark: SparkSession, charge_resolution: ChargeResolution
) -> None:
    metering_point_rows = [_create_metering_point_row()]
    time_series_rows = [_create_time_series_row()]
    charges_rows = [
        _create_charges_row(
            charge_resolution=ChargeResolution.HOUR,
        ),
        _create_charges_row(
            charge_resolution=ChargeResolution.DAY,
        ),
    ]

    metering_point = _create_dataframe_from_rows(
        spark, metering_point_rows, metering_point_period_schema
    )
    time_series = _create_dataframe_from_rows(
        spark, time_series_rows, time_series_point_schema
    )
    charges = _create_dataframe_from_rows(spark, charges_rows, charges_schema)

    actual = get_tariff_charges(
        metering_point,
        time_series,
        charges,
        charge_resolution,
    )

    assert actual.collect()[0][Colname.charge_resolution] == charge_resolution.value


def test__get_charges_functions__filters_on_correct_charge_type(
    spark: SparkSession,
) -> None:
    metering_point_rows = [_create_metering_point_row()]
    time_series_rows = [_create_time_series_row()]
    charges_rows = [
        _create_charges_row(
            charge_type=ChargeType.TARIFF,
        ),
        _create_charges_row(
            charge_type=ChargeType.FEE,
        ),
        _create_charges_row(
            charge_type=ChargeType.SUBSCRIPTION,
        ),
    ]

    metering_point = _create_dataframe_from_rows(
        spark, metering_point_rows, metering_point_period_schema
    )
    time_series = _create_dataframe_from_rows(
        spark, time_series_rows, time_series_point_schema
    )
    charges = _create_dataframe_from_rows(spark, charges_rows, charges_schema)

    actual_tariff = get_tariff_charges(
        metering_point,
        time_series,
        charges,
        ChargeResolution.HOUR,
    )
    actual_fee = get_fee_charges(charges, metering_point)
    actual_subscription = get_subscription_charges(charges, metering_point)

    assert actual_tariff.collect()[0][Colname.charge_type] == ChargeType.TARIFF.value
    assert actual_fee.collect()[0][Colname.charge_type] == ChargeType.FEE.value
    assert (
        actual_subscription.collect()[0][Colname.charge_type]
        == ChargeType.SUBSCRIPTION.value
    )


def test__get_subscription_charges__split_into_days_between_from_and_to_date(
    spark: SparkSession,
) -> None:
    metering_point_rows = [_create_metering_point_row()]
    charges_rows = [
        _create_charges_row(
            from_date=datetime(2020, 1, 1, 0),
            to_date=datetime(2020, 2, 1, 0),
            charge_type=ChargeType.SUBSCRIPTION,
        ),
    ]

    metering_point = _create_dataframe_from_rows(
        spark, metering_point_rows, metering_point_period_schema
    )

    charges = _create_dataframe_from_rows(spark, charges_rows, charges_schema)

    actual_subscription = get_subscription_charges(charges, metering_point)
    assert actual_subscription.count() == 31


def test__get_tariff_charges__joins_on_metering_point_id_and_time_is_between_from_and_to_date(
    spark: SparkSession,
) -> None:
    metering_point_rows = [
        _create_metering_point_row(
            from_date=datetime(2020, 1, 1, 0), to_date=datetime(2020, 1, 1, 2)
        ),
    ]
    time_series_rows = [
        _create_time_series_row(observation_time=datetime(2020, 1, 1, 0)),
        _create_time_series_row(observation_time=datetime(2020, 1, 1, 1)),
        _create_time_series_row(observation_time=datetime(2020, 1, 1, 2)),
    ]
    charges_rows = [
        _create_charges_row(
            from_date=datetime(2020, 1, 1, 0),
            to_date=datetime(2020, 1, 1, 1),
            charge_time=datetime(2020, 1, 1, 0),
        ),
        _create_charges_row(
            from_date=datetime(2020, 1, 1, 1),
            to_date=datetime(2020, 1, 1, 2),
            charge_time=datetime(2020, 1, 1, 1),
        ),
        _create_charges_row(
            from_date=datetime(2020, 1, 1, 2),
            to_date=datetime(2020, 1, 1, 3),
            charge_time=datetime(2020, 1, 1, 2),
        ),
    ]

    metering_point = _create_dataframe_from_rows(
        spark, metering_point_rows, metering_point_period_schema
    )
    time_series = _create_dataframe_from_rows(
        spark, time_series_rows, time_series_point_schema
    )
    charges = _create_dataframe_from_rows(spark, charges_rows, charges_schema)

    actual = get_tariff_charges(
        metering_point,
        time_series,
        charges,
        ChargeResolution.HOUR,
    )

    assert actual.count() == 2


def test__get_tariff_charges__sums_quantity_when_same_metering_point_and_resolution(
    spark: SparkSession,
) -> None:
    metering_point_rows = [_create_metering_point_row()]
    time_series_rows = [_create_time_series_row(), _create_time_series_row()]
    charges_rows = [_create_charges_row()]

    metering_point = _create_dataframe_from_rows(
        spark, metering_point_rows, metering_point_period_schema
    )
    time_series = _create_dataframe_from_rows(
        spark, time_series_rows, time_series_point_schema
    )
    charges = _create_dataframe_from_rows(spark, charges_rows, charges_schema)

    actual = get_tariff_charges(
        metering_point,
        time_series,
        charges,
        ChargeResolution.HOUR,
    )

    assert actual.collect()[0][Colname.quantity] == 2 * DEFAULT_QUANTITY


def test__get_tariff_charges__when_no_matching_charge_resolution__returns_empty_tariffs(
    spark: SparkSession,
) -> None:
    metering_point_rows = [_create_metering_point_row()]
    time_series_rows = [_create_time_series_row()]
    charges_rows = [_create_charges_row(charge_resolution=ChargeResolution.DAY)]

    metering_point = _create_dataframe_from_rows(
        spark, metering_point_rows, metering_point_period_schema
    )
    time_series = _create_dataframe_from_rows(
        spark, time_series_rows, time_series_point_schema
    )
    charges = _create_dataframe_from_rows(spark, charges_rows, charges_schema)

    actual = get_tariff_charges(
        metering_point,
        time_series,
        charges,
        ChargeResolution.HOUR,
    )

    assert actual.count() == 0
