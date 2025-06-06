from pyspark.sql.types import (
    StringType,
    StructField,
    StructType,
)

grid_loss_metering_point_ids_schema = StructType(
    [
        StructField("metering_point_id", StringType(), False),
    ]
)
