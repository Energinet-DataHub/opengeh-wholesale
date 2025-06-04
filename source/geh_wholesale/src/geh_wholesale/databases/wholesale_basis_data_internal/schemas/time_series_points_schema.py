from pyspark.sql.types import (
    DecimalType,
    StringType,
    StructField,
    StructType,
    TimestampType,
)

from geh_wholesale.databases.table_column_names import TableColumnNames

time_series_points_schema = StructType(
    [
        StructField(TableColumnNames.calculation_id, StringType(), False),
        StructField(TableColumnNames.metering_point_id, StringType(), False),
        StructField(TableColumnNames.quantity, DecimalType(18, 3), False),
        StructField(TableColumnNames.quality, StringType(), False),
        StructField(TableColumnNames.observation_time, TimestampType(), False),
        StructField(TableColumnNames.metering_point_type, StringType(), True),
        StructField(TableColumnNames.resolution, StringType(), True),
        StructField(TableColumnNames.grid_area_code, StringType(), True),
        StructField(TableColumnNames.energy_supplier_id, StringType(), True),
    ]
)
