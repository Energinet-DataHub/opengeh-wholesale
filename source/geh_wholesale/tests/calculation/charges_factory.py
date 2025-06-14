from datetime import datetime, timezone
from decimal import Decimal

from pyspark.sql import Row, SparkSession

import geh_wholesale.codelists as e
from geh_wholesale.calculation.preparation.data_structures.charge_link_metering_point_periods import (
    ChargeLinkMeteringPointPeriods,
    charge_link_metering_point_periods_schema,
)
from geh_wholesale.calculation.preparation.data_structures.charge_price_information import (
    ChargePriceInformation,
    charge_price_information_schema,
)
from geh_wholesale.calculation.preparation.data_structures.charge_prices import (
    ChargePrices,
    charge_prices_schema,
)
from geh_wholesale.codelists import ChargeType
from geh_wholesale.constants import Colname
from tests.calculation.preparation.transformations import (
    prepared_metering_point_time_series_factory,
)


class DefaultValues:
    GRID_AREA = "543"
    CHARGE_TYPE = ChargeType.TARIFF
    CHARGE_CODE = "4000"
    CHARGE_OWNER = "001"
    CHARGE_TAX = True
    CHARGE_TIME_HOUR_0 = datetime(2019, 12, 31, 23, tzinfo=timezone.utc)
    CHARGE_PRICE = Decimal("2.000005")
    CHARGE_QUANTITY = 1
    ENERGY_SUPPLIER_ID = "1234567890123"
    METERING_POINT_ID = "123456789012345678901234567"
    METERING_POINT_TYPE = e.MeteringPointType.CONSUMPTION
    SETTLEMENT_METHOD = e.SettlementMethod.FLEX
    QUANTITY = Decimal("1.005")
    PERIOD_START_DATETIME = datetime(2019, 12, 31, 23, tzinfo=timezone.utc)
    FROM_DATE: datetime = datetime(2019, 12, 31, 23, tzinfo=timezone.utc)
    TO_DATE: datetime = datetime(2020, 1, 31, 23, tzinfo=timezone.utc)


def create_time_series_row(
    metering_point_id: str = DefaultValues.METERING_POINT_ID,
    quantity: Decimal = DefaultValues.QUANTITY,
    quality: e.QuantityQuality = e.QuantityQuality.CALCULATED,
    observation_time: datetime = datetime(2019, 12, 31, 23, tzinfo=timezone.utc),
) -> Row:
    return prepared_metering_point_time_series_factory.create_row(
        metering_point_id=metering_point_id,
        quantity=quantity,
        quality=quality,
        observation_time=observation_time,
    )


def create_charge_price_information_row(
    charge_code: str = DefaultValues.CHARGE_CODE,
    charge_type: ChargeType = DefaultValues.CHARGE_TYPE,
    charge_owner: str = DefaultValues.CHARGE_OWNER,
    charge_tax: bool = DefaultValues.CHARGE_TAX,
    resolution: e.ChargeResolution = e.ChargeResolution.HOUR,
    from_date: datetime = DefaultValues.FROM_DATE,
    to_date: datetime | None = DefaultValues.TO_DATE,
) -> Row:
    charge_key: str = f"{charge_code}-{charge_owner}-{charge_type.value}"

    row = {
        Colname.charge_key: charge_key,
        Colname.charge_code: charge_code,
        Colname.charge_type: charge_type.value,
        Colname.charge_owner: charge_owner,
        Colname.charge_tax: charge_tax,
        Colname.resolution: resolution.value,
        Colname.from_date: from_date,
        Colname.to_date: to_date,
    }

    return Row(**row)


def create_charge_prices_row(
    charge_code: str = DefaultValues.CHARGE_CODE,
    charge_type: ChargeType = DefaultValues.CHARGE_TYPE,
    charge_owner: str = DefaultValues.CHARGE_OWNER,
    charge_time: datetime = DefaultValues.CHARGE_TIME_HOUR_0,
    charge_price: Decimal = DefaultValues.CHARGE_PRICE,
) -> Row:
    charge_key: str = f"{charge_code}-{charge_owner}-{charge_type.value}"

    row = {
        Colname.charge_key: charge_key,
        Colname.charge_code: charge_code,
        Colname.charge_type: charge_type.value,
        Colname.charge_owner: charge_owner,
        Colname.charge_price: charge_price,
        Colname.charge_time: charge_time,
    }

    return Row(**row)


def create_charge_link_metering_point_periods_row(
    charge_type: e.ChargeType = DefaultValues.CHARGE_TYPE,
    charge_code: str = DefaultValues.CHARGE_CODE,
    charge_owner: str = DefaultValues.CHARGE_OWNER,
    metering_point_id: str = DefaultValues.METERING_POINT_ID,
    quantity: int = DefaultValues.CHARGE_QUANTITY,
    metering_point_type: e.MeteringPointType = DefaultValues.METERING_POINT_TYPE,
    settlement_method: e.SettlementMethod | None = DefaultValues.SETTLEMENT_METHOD,
    grid_area: str = DefaultValues.GRID_AREA,
    energy_supplier_id: str | None = DefaultValues.ENERGY_SUPPLIER_ID,
    from_date: datetime = DefaultValues.FROM_DATE,
    to_date: datetime | None = DefaultValues.TO_DATE,
) -> Row:
    charge_key: str = f"{charge_code}-{charge_owner}-{charge_type.value}"

    row = {
        Colname.charge_key: charge_key,
        Colname.charge_type: charge_type.value,
        Colname.metering_point_id: metering_point_id,
        Colname.quantity: quantity,
        Colname.from_date: from_date.astimezone(timezone.utc),
        Colname.to_date: to_date.astimezone(timezone.utc) if to_date else None,
        Colname.metering_point_type: metering_point_type.value,
        Colname.settlement_method: (settlement_method.value if settlement_method else None),
        Colname.grid_area_code: grid_area,
        Colname.energy_supplier_id: energy_supplier_id,
    }

    return Row(**row)


def create_charge_price_information(spark: SparkSession, data: None | Row | list[Row] = None) -> ChargePriceInformation:
    if data is None:
        data = [create_charge_price_information_row()]
    elif isinstance(data, Row):
        data = [data]
    df = spark.createDataFrame(data, charge_price_information_schema)
    return ChargePriceInformation(df)


def create_charge_prices(spark: SparkSession, data: None | Row | list[Row] = None) -> ChargePrices:
    if data is None:
        data = [create_charge_prices_row()]
    elif isinstance(data, Row):
        data = [data]
    df = spark.createDataFrame(data, charge_prices_schema)
    return ChargePrices(df)


def create_charge_link_metering_point_periods(
    spark: SparkSession, data: None | Row | list[Row] = None
) -> ChargeLinkMeteringPointPeriods:
    if data is None:
        data = [create_charge_link_metering_point_periods_row()]
    elif isinstance(data, Row):
        data = [data]
    df = spark.createDataFrame(data, charge_link_metering_point_periods_schema)
    return ChargeLinkMeteringPointPeriods(df)
