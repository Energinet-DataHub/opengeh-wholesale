from datetime import datetime

from pyspark import Row
from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.types import (
    StringType,
    StructType,
    StructField,
    IntegerType,
    TimestampType,
)

import package.codelists as e
from package.constants import Colname


class DefaultValues:
    CHARGE_CODE = "4000"
    CHARGE_TYPE = e.ChargeType.FEE
    CHARGE_OWNER = "001"
    CHARGE_TIME_HOUR_0 = datetime(2020, 1, 1, 0)
    CHARGE_QUANTITY = 1
    ENERGY_SUPPLIER_ID = "1234567890123"
    METERING_POINT_ID = "123456789012345678901234567"
    FROM_DATE = datetime(2019, 12, 31, 23)
    TO_DATE = datetime(2020, 1, 31, 23)


def create_row(
    charge_code: str = DefaultValues.CHARGE_CODE,
    charge_type: e.ChargeType = DefaultValues.CHARGE_TYPE,
    charge_owner: str = DefaultValues.CHARGE_OWNER,
    metering_point_id: str = DefaultValues.METERING_POINT_ID,
    quantity: int = DefaultValues.CHARGE_QUANTITY,
    from_date: datetime = DefaultValues.FROM_DATE,
    to_date: datetime | None = DefaultValues.TO_DATE,
) -> Row:
    charge_key: str = f"{charge_code}-{charge_owner}-{charge_type.value}"

    row = {
        Colname.charge_code: charge_code,
        Colname.charge_type: charge_type.value,
        Colname.charge_owner: charge_owner,
        Colname.metering_point_id: metering_point_id,
        Colname.quantity: quantity,
        Colname.from_date: from_date,
        Colname.to_date: to_date,
        Colname.charge_key: charge_key,
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
        StructField("charge_code", StringType(), False),
        StructField("charge_type", StringType(), False),
        StructField("charge_owner_id", StringType(), False),
        StructField("metering_point_id", StringType(), False),
        StructField("quantity", IntegerType(), False),
        StructField("from_date", TimestampType(), False),
        StructField("to_date", TimestampType(), True),
        StructField("charge_key", StringType(), False),
    ]
)
