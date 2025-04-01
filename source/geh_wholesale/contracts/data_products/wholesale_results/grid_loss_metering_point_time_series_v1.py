import pyspark.sql.types as t

nullable = True

grid_loss_metering_point_time_series_v1 = t.StructType(
    [
        # UUID
        t.StructField("calculation_id", t.StringType(), not nullable),
        #
        # 'aggregation' | 'balance_fixing' | 'wholesale_fixing' | 'first_correction_settlement' |
        # 'second_correction_settlement' | 'third_correction_settlement'
        t.StructField("calculation_type", t.StringType(), not nullable),
        #
        # UTC time
        t.StructField("calculation_period_start", t.TimestampType(), not nullable),
        #
        # UTC time
        t.StructField("calculation_period_end", t.TimestampType(), not nullable),
        #
        t.StructField("calculation_version", t.LongType(), not nullable),
        #
        # GSRN number
        t.StructField("metering_point_id", t.StringType(), not nullable),
        #
        # 'consumption' | 'production'
        t.StructField("metering_point_type", t.StringType(), not nullable),
        #
        # 'PT15M' | 'PT1H'
        t.StructField("resolution", t.StringType(), not nullable),
        #
        # 'kWh'
        t.StructField("quantity_unit", t.StringType(), not nullable),
        #
        # UTC time
        t.StructField("time", t.TimestampType(), not nullable),
        #
        t.StructField("quantity", t.DecimalType(18, 3), not nullable),
    ]
)
