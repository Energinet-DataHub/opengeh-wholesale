from pyspark.sql.types import (
    IntegerType,
    StringType,
    StructField,
    StructType,
    TimestampType,
)

from geh_wholesale.databases.table_column_names import TableColumnNames

charge_link_periods_schema = StructType(
    [
        StructField(TableColumnNames.calculation_id, StringType(), False),
        StructField(TableColumnNames.charge_key, StringType(), False),
        StructField(TableColumnNames.charge_code, StringType(), False),
        StructField(TableColumnNames.charge_type, StringType(), False),
        StructField(TableColumnNames.charge_owner_id, StringType(), False),
        StructField(TableColumnNames.metering_point_id, StringType(), False),
        StructField(TableColumnNames.quantity, IntegerType(), False),
        StructField(TableColumnNames.from_date, TimestampType(), False),
        StructField(TableColumnNames.to_date, TimestampType(), False),
    ]
)
