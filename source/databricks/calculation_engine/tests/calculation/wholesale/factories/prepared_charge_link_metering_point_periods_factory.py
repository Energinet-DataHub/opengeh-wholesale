from datetime import datetime

from pyspark import Row
from pyspark.sql import DataFrame, SparkSession
from pyspark.sql.types import (
    StructField,
    StringType,
    TimestampType,
    IntegerType,
    StructType,
)

from package.codelists import ChargeType, SettlementMethod, MeteringPointType
from package.constants import Colname


class DefaultValues:
    GRID_AREA_CODE = "805"
    CHARGE_TYPE = ChargeType.FEE
    CHARGE_CODE = "4000"
    CHARGE_OWNER = "001"
    ENERGY_SUPPLIER_ID = "9999999999999"
    METERING_POINT_TYPE = MeteringPointType.CONSUMPTION
    METERING_POINT_ID = "123456789012345678901234567"
    SETTLEMENT_METHOD = SettlementMethod.FLEX
    QUANTITY = 1
    FROM_DATE: datetime = datetime(2019, 12, 31, 23)
    TO_DATE: datetime = datetime(2020, 1, 31, 23)


def create_row(
    charge_type: ChargeType = DefaultValues.CHARGE_TYPE,
    charge_code: str = DefaultValues.CHARGE_CODE,
    charge_owner: str = DefaultValues.CHARGE_OWNER,
    quantity: int = DefaultValues.QUANTITY,
    metering_point_type: MeteringPointType = DefaultValues.METERING_POINT_TYPE,
    metering_point_id: str = DefaultValues.METERING_POINT_ID,
    settlement_method: SettlementMethod = DefaultValues.SETTLEMENT_METHOD,
    grid_area_code: str = DefaultValues.GRID_AREA_CODE,
    energy_supplier_id: str = DefaultValues.ENERGY_SUPPLIER_ID,
    from_date: datetime = DefaultValues.FROM_DATE,
    to_date: datetime = DefaultValues.TO_DATE,
) -> Row:
    charge_key: str = f"{charge_code}-{charge_owner}-{charge_type.value}"

    row = {
        Colname.charge_key: charge_key,
        Colname.charge_type: charge_type.value,
        Colname.metering_point_id: metering_point_id,
        Colname.quantity: quantity,
        Colname.from_date: from_date,
        Colname.to_date: to_date,
        Colname.metering_point_type: metering_point_type.value,
        Colname.settlement_method: settlement_method.value,
        Colname.grid_area_code: grid_area_code,
        Colname.energy_supplier_id: energy_supplier_id,
    }

    return Row(**row)


def create(spark: SparkSession, data: None | Row | list[Row] = None) -> DataFrame:
    if data is None:
        data = [create_row()]
    elif isinstance(data, Row):
        data = [data]
    return spark.createDataFrame(data, schema=_schema)


_schema = StructType(
    [
        StructField(Colname.charge_key, StringType(), False),
        StructField(Colname.charge_type, StringType(), False),
        StructField(Colname.metering_point_id, StringType(), False),
        StructField(Colname.quantity, IntegerType(), False),
        StructField(Colname.from_date, TimestampType(), True),
        StructField(Colname.to_date, TimestampType(), True),
        StructField(Colname.metering_point_type, StringType(), False),
        StructField(Colname.settlement_method, StringType(), True),
        StructField(Colname.grid_area_code, StringType(), False),
        StructField(Colname.energy_supplier_id, StringType(), False),
    ]
)
