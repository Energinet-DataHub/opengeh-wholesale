from datetime import datetime
from decimal import Decimal

from pyspark.sql import Row, SparkSession

from geh_wholesale.calculation.preparation.data_structures.prepared_metering_point_time_series import (
    PreparedMeteringPointTimeSeries,
    prepared_metering_point_time_series_schema,
)
from geh_wholesale.codelists import (
    MeteringPointResolution,
    MeteringPointType,
    QuantityQuality,
    SettlementMethod,
)
from geh_wholesale.constants import Colname

DEFAULT_METERING_POINT_ID = "123456789012345678901234567"
DEFAULT_METERING_POINT_TYPE = MeteringPointType.PRODUCTION
DEFAULT_SETTLEMENT_METHOD = SettlementMethod.FLEX
DEFAULT_GRID_AREA = "805"
DEFAULT_RESOLUTION = MeteringPointResolution.HOUR
DEFAULT_ENERGY_SUPPLIER_ID = "9999999999999"
DEFAULT_BALANCE_RESPONSIBLE_ID = "1234567890123"
DEFAULT_OBSERVATION_TIME = datetime(2020, 1, 1, 0, 0)
DEFAULT_QUANTITY = Decimal(1.0)


def create_row(
    grid_area: str = DEFAULT_GRID_AREA,
    to_grid_area: str | None = None,
    from_grid_area: str | None = None,
    metering_point_id: str = DEFAULT_METERING_POINT_ID,
    metering_point_type: MeteringPointType = DEFAULT_METERING_POINT_TYPE,
    resolution: MeteringPointResolution = DEFAULT_RESOLUTION,
    observation_time: datetime = DEFAULT_OBSERVATION_TIME,
    quantity: Decimal = DEFAULT_QUANTITY,
    quality: QuantityQuality = QuantityQuality.MEASURED,
    energy_supplier_id: str | None = DEFAULT_ENERGY_SUPPLIER_ID,
    balance_responsible_id: str | None = DEFAULT_BALANCE_RESPONSIBLE_ID,
    settlement_method: SettlementMethod | None = DEFAULT_SETTLEMENT_METHOD,
) -> Row:
    row = {
        Colname.grid_area_code: grid_area,
        Colname.to_grid_area_code: to_grid_area,
        Colname.from_grid_area_code: from_grid_area,
        Colname.metering_point_id: metering_point_id,
        Colname.metering_point_type: metering_point_type.value,
        Colname.resolution: resolution.value,
        Colname.observation_time: observation_time,
        Colname.quantity: quantity,
        Colname.quality: quality.value,
        Colname.energy_supplier_id: energy_supplier_id,
        Colname.balance_responsible_party_id: balance_responsible_id,
        Colname.settlement_method: (settlement_method.value if settlement_method else None),
    }

    return Row(**row)


def create(spark: SparkSession, data: None | Row | list[Row] = None) -> PreparedMeteringPointTimeSeries:
    if data is None:
        data = [create_row()]
    elif isinstance(data, Row):
        data = [data]

    df = spark.createDataFrame(data, schema=prepared_metering_point_time_series_schema)
    return PreparedMeteringPointTimeSeries(df)
