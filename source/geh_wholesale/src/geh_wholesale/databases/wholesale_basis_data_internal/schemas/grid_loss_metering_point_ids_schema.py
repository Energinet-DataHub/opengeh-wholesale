from pyspark.sql.types import (
    StringType,
    StructField,
    StructType,
)

from geh_wholesale.databases.table_column_names import TableColumnNames

grid_loss_metering_point_ids_schema = StructType(
    [
        StructField(TableColumnNames.calculation_id, StringType(), False),
        StructField(TableColumnNames.metering_point_id, StringType(), False),
    ]
)
