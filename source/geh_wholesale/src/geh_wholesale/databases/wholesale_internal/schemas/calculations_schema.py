from pyspark.sql.types import (
    BooleanType,
    LongType,
    StringType,
    StructField,
    StructType,
    TimestampType,
)

from geh_wholesale.databases.table_column_names import TableColumnNames

calculations_schema = StructType(
    [
        StructField(TableColumnNames.calculation_id, StringType(), False),
        StructField(TableColumnNames.calculation_type, StringType(), False),
        StructField(TableColumnNames.calculation_period_start, TimestampType(), False),
        StructField(TableColumnNames.calculation_period_end, TimestampType(), False),
        StructField(TableColumnNames.calculation_version, LongType(), False),
        StructField(
            TableColumnNames.calculation_execution_time_start,
            TimestampType(),
            False,
        ),
        StructField(
            TableColumnNames.calculation_succeeded_time,
            TimestampType(),
            True,
        ),
        StructField(TableColumnNames.is_internal_calculation, BooleanType(), True),
    ]
)
