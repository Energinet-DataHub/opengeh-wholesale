from pyspark.sql.types import (
    DecimalType,
    StringType,
    StructField,
    StructType,
    TimestampType,
)

from geh_wholesale.databases.table_column_names import TableColumnNames

# Note: The order of the columns must match the order of the columns in the Delta table
total_monthly_amounts_schema_uc = StructType(
    [
        StructField(TableColumnNames.calculation_id, StringType(), False),
        StructField(TableColumnNames.result_id, StringType(), False),
        StructField(TableColumnNames.grid_area_code, StringType(), False),
        StructField(TableColumnNames.energy_supplier_id, StringType(), True),
        StructField(TableColumnNames.time, TimestampType(), False),
        StructField(TableColumnNames.amount, DecimalType(18, 6), True),
        StructField(TableColumnNames.charge_owner_id, StringType(), True),
    ]
)
