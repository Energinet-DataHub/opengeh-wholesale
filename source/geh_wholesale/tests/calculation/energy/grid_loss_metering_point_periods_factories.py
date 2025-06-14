from datetime import UTC, datetime

from pyspark.sql import Row, SparkSession

from geh_wholesale.calculation.preparation.data_structures.grid_loss_metering_point_periods import (
    GridLossMeteringPointPeriods,
    grid_loss_metering_point_periods_schema,
)
from geh_wholesale.codelists import MeteringPointType
from geh_wholesale.constants import Colname

DEFAULT_METERING_POINT_ID = "1234567890123"
DEFAULT_GRID_AREA = "100"
DEFAULT_FROM_DATE = datetime(2019, 12, 31, 23, 0, tzinfo=UTC)
"""Midnight the 1st of January 2020 assuming local time zone is Europe/copenhagen"""
DEFAULT_TO_DATE = datetime(2020, 1, 1, 23, 0, tzinfo=UTC)
"""Midnight the 2st of January 2020 assuming local time zone is Europe/copenhagen"""
DEFAULT_METERING_POINT_TYPE = MeteringPointType.CONSUMPTION
DEFAULT_ENERGY_SUPPLIER_ID = "1234567890123"
DEFAULT_BALANCE_RESPONSIBLE_ID = "2345678901234"


def create_row(
    metering_point_id: str = DEFAULT_METERING_POINT_ID,
    grid_area: str = DEFAULT_GRID_AREA,
    from_date: datetime = DEFAULT_FROM_DATE,
    to_date: datetime | None = DEFAULT_TO_DATE,
    metering_point_type: MeteringPointType = DEFAULT_METERING_POINT_TYPE,
    energy_supplier_id: str = DEFAULT_ENERGY_SUPPLIER_ID,
    balance_responsible_id: str = DEFAULT_BALANCE_RESPONSIBLE_ID,
) -> Row:
    row = {
        Colname.metering_point_id: metering_point_id,
        Colname.grid_area_code: grid_area,
        Colname.from_date: from_date,
        Colname.to_date: to_date,
        Colname.metering_point_type: metering_point_type.value,
        Colname.energy_supplier_id: energy_supplier_id,
        Colname.balance_responsible_party_id: balance_responsible_id,
    }

    return Row(**row)


def create(spark: SparkSession, data: None | Row | list[Row] = None) -> GridLossMeteringPointPeriods:
    if data is None:
        data = [create_row()]
    elif isinstance(data, Row):
        data = [data]
    df = spark.createDataFrame(data, schema=grid_loss_metering_point_periods_schema)
    return GridLossMeteringPointPeriods(df)
