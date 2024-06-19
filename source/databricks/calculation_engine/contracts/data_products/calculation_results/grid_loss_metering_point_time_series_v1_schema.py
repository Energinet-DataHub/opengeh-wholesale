import pyspark.sql.types as t

grid_loss_metering_point_time_series_v1_schema = t.StructType(
    [
        t.StructField("calculation_id", t.StringType(), False),
        t.StructField("calculation_type", t.StringType(), False),
        t.StructField("calculation_period_start", t.TimestampType(), False),
        t.StructField("calculation_period_end", t.TimestampType(), False),
        t.StructField("calculation_version", t.LongType(), False),
        t.StructField("metering_point_id", t.StringType(), False),
        t.StructField("metering_point_type", t.StringType(), False),
        t.StructField("resolution", t.StringType(), False),
        t.StructField("quantity_unit", t.StringType(), False),
        t.StructField("time", t.TimestampType(), False),
        t.StructField("quantity", t.DecimalType(18, 3), False),
    ]
)
