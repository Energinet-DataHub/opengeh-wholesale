from datetime import datetime, timezone
from decimal import Decimal

from pyspark.sql import Row, SparkSession

from geh_wholesale.calculation.preparation.data_structures.prepared_tariffs import (
    PreparedTariffs,
    prepared_tariffs_schema,
)
from geh_wholesale.codelists import (
    ChargeQuality,
    ChargeResolution,
    ChargeType,
    MeteringPointType,
    SettlementMethod,
)
from geh_wholesale.constants import Colname


class DefaultValues:
    GRID_AREA = "543"
    CHARGE_CODE = "4000"
    CHARGE_OWNER = "001"
    CHARGE_TAX = True
    CHARGE_TIME_HOUR_0 = datetime(2020, 1, 1, 0)
    CHARGE_PRICE = Decimal("2.000005")
    ENERGY_SUPPLIER_ID = "1234567890123"
    METERING_POINT_ID = "123456789012345678901234567"
    METERING_POINT_TYPE = MeteringPointType.CONSUMPTION
    SETTLEMENT_METHOD = SettlementMethod.FLEX
    QUANTITY = Decimal("1.005")
    QUALITY = ChargeQuality.CALCULATED
    PERIOD_START_DATETIME = datetime(2019, 12, 31, 23, tzinfo=timezone.utc)


def create_row(
    charge_key: str | None = None,
    charge_code: str = DefaultValues.CHARGE_CODE,
    charge_owner: str = DefaultValues.CHARGE_OWNER,
    resolution: ChargeResolution = ChargeResolution.HOUR,
    charge_time: datetime = DefaultValues.CHARGE_TIME_HOUR_0,
    charge_price: Decimal | None = DefaultValues.CHARGE_PRICE,
    energy_supplier_id: str = DefaultValues.ENERGY_SUPPLIER_ID,
    metering_point_id: str = DefaultValues.METERING_POINT_ID,
    metering_point_type: MeteringPointType = DefaultValues.METERING_POINT_TYPE,
    settlement_method: SettlementMethod | None = DefaultValues.SETTLEMENT_METHOD,
    grid_area: str = DefaultValues.GRID_AREA,
    quantity: Decimal = DefaultValues.QUANTITY,
    quality: ChargeQuality = DefaultValues.QUALITY,
) -> Row:
    row = {
        Colname.charge_key: charge_key or f"{charge_code}-{ChargeType.TARIFF.value}-{charge_owner}",
        Colname.charge_code: charge_code,
        Colname.charge_type: ChargeType.TARIFF.value,
        Colname.charge_owner: charge_owner,
        Colname.charge_tax: DefaultValues.CHARGE_TAX,
        Colname.resolution: resolution.value,
        Colname.charge_time: charge_time,
        Colname.charge_price: charge_price,
        Colname.metering_point_id: metering_point_id,
        Colname.energy_supplier_id: energy_supplier_id,
        Colname.metering_point_type: metering_point_type.value,
        Colname.settlement_method: (settlement_method.value if settlement_method else None),
        Colname.grid_area_code: grid_area,
        Colname.quantity: quantity,
        Colname.qualities: [quality.value],
    }

    return Row(**row)


def create(spark: SparkSession, data: None | Row | list[Row] = None) -> PreparedTariffs:
    if data is None:
        data = [create_row()]
    elif isinstance(data, Row):
        data = [data]
    df = spark.createDataFrame(data, prepared_tariffs_schema)
    return PreparedTariffs(df)
