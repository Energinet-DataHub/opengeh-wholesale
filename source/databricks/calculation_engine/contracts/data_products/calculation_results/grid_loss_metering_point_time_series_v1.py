import pyspark.sql.types as t

grid_loss_metering_point_time_series_v1 = t.StructType(
    [
        # UUID
        t.StructField("calculation_id", t.StringType(), False),
        # 'balance_fixing' | 'aggregation'
        t.StructField("calculation_type", t.StringType(), False),
        # UTC time
        t.StructField("calculation_period_start", t.TimestampType(), False),
        # UTC time
        t.StructField("calculation_period_end", t.TimestampType(), False),
        t.StructField("calculation_version", t.LongType(), False),
        # GSRN number
        t.StructField("metering_point_id", t.StringType(), False),
        # 'consumption' | 'production'
        t.StructField("metering_point_type", t.StringType(), False),
        # 'PT15M' | 'PT1H'
        t.StructField("resolution", t.StringType(), False),
        # UTC time
        t.StructField("time", t.TimestampType(), False),
        t.StructField("quantity", t.DecimalType(18, 3), False),
        # 'kWh'
        t.StructField("quantity_unit", t.StringType(), False),
    ]
)
