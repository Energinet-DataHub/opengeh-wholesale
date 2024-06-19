import pyspark.sql.types as t

energy_result_points_per_ga_v1_schema = t.StructType(
    [
        t.StructField("calculation_id", t.StringType(), False),
        t.StructField("calculation_type", t.StringType(), False),
        t.StructField("calculation_version", t.LongType(), False),
        t.StructField("result_id", t.StringType(), False),
        t.StructField("grid_area_code", t.StringType(), False),
        t.StructField("metering_point_type", t.StringType(), False),
        t.StructField("settlement_method", t.StringType(), True),
        t.StructField("resolution", t.StringType(), False),
        t.StructField("time", t.TimestampType(), False),
        t.StructField("quantity", t.DecimalType(18, 3), False),
    ]
)
