from datetime import datetime

from pyspark.sql import DataFrame, Row, SparkSession

from geh_wholesale.codelists import CalculationType
from geh_wholesale.databases.table_column_names import TableColumnNames
from geh_wholesale.databases.wholesale_internal.schemas import (
    calculations_schema,
)


class DefaultValues:
    CALCULATION_ID = "ff3b3b3b-3b3b-3b3b-3b3b-3b3b3b3b3b3b"
    CALCULATION_TYPE = CalculationType.BALANCE_FIXING
    CALCULATION_PERIOD_START_DATETIME = datetime(2019, 12, 31, 23)
    CALCULATION_PERIOD_END_DATETIME = datetime(2020, 1, 31, 23)
    CALCULATION_EXECUTION_TIME_START = datetime(2020, 2, 1, 11)
    CREATED_BY_USER_ID = "aa3b3b3b-3b3b-3b3b-3b3b-3b3b3b3b3b3b"
    VERSION = 7


def create_calculation_row(
    calculation_id: str = DefaultValues.CALCULATION_ID,
    calculation_type: CalculationType = DefaultValues.CALCULATION_TYPE,
    calculation_period_start_datetime: datetime = DefaultValues.CALCULATION_PERIOD_START_DATETIME,
    calculation_period_end_datetime: datetime = DefaultValues.CALCULATION_PERIOD_END_DATETIME,
    calculation_execution_time_start: datetime = DefaultValues.CALCULATION_EXECUTION_TIME_START,
    version: int = DefaultValues.VERSION,
) -> Row:
    calculation = {
        TableColumnNames.calculation_id: calculation_id,
        TableColumnNames.calculation_type: calculation_type.value,
        TableColumnNames.calculation_period_start: calculation_period_start_datetime,
        TableColumnNames.calculation_period_end: calculation_period_end_datetime,
        TableColumnNames.calculation_version: version,
        TableColumnNames.calculation_execution_time_start: calculation_execution_time_start,
        TableColumnNames.calculation_succeeded_time: None,
        TableColumnNames.is_internal_calculation: False,
    }

    return Row(**calculation)


def create_calculations(spark: SparkSession, data: None | Row | list[Row] = None) -> DataFrame:
    if data is None:
        data = [create_calculation_row()]
    elif isinstance(data, Row):
        data = [data]
    return spark.createDataFrame(data=data, schema=calculations_schema)


def create_empty_calculations(spark: SparkSession) -> DataFrame:
    return spark.createDataFrame(data=[], schema=calculations_schema)
