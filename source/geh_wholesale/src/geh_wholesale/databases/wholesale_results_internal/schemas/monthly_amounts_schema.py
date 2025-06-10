from pyspark.sql.types import (
    BooleanType,
    DecimalType,
    StringType,
    StructField,
    StructType,
    TimestampType,
)

from geh_wholesale.databases.table_column_names import TableColumnNames

# Note: The order of the columns must match the order of the columns in the Delta table
monthly_amounts_schema_uc = StructType(
    [
        StructField(TableColumnNames.calculation_id, StringType(), False),
        StructField(TableColumnNames.result_id, StringType(), False),
        StructField(TableColumnNames.grid_area_code, StringType(), False),
        StructField(TableColumnNames.energy_supplier_id, StringType(), False),
        StructField(TableColumnNames.quantity_unit, StringType(), False),
        StructField(TableColumnNames.time, TimestampType(), False),
        StructField(TableColumnNames.amount, DecimalType(18, 6), True),
        StructField(TableColumnNames.is_tax, BooleanType(), False),
        StructField(TableColumnNames.charge_code, StringType(), False),
        StructField(TableColumnNames.charge_type, StringType(), False),
        StructField(TableColumnNames.charge_owner_id, StringType(), False),
    ]
)
