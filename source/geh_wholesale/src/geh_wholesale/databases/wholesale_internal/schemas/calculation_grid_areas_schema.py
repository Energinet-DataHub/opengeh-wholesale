from pyspark.sql.types import (
    StringType,
    StructField,
    StructType,
)

from geh_wholesale.databases.table_column_names import TableColumnNames

calculation_grid_areas_schema = StructType(
    [
        StructField(TableColumnNames.calculation_id, StringType(), False),
        StructField(TableColumnNames.grid_area_code, StringType(), False),
    ]
)
