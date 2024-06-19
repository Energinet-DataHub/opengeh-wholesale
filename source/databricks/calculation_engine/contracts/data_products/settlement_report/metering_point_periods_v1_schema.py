import pyspark.sql.types as t

metering_point_periods_v1_schema = t.StructType(
    [
        t.StructField("calculation_id", t.StringType(), False),
        t.StructField("calculation_type", t.StringType(), False),
        t.StructField("calculation_version", t.LongType(), False),
        t.StructField("metering_point_id", t.StringType(), False),
        t.StructField("from_date", t.TimestampType(), False),
        t.StructField("to_date", t.TimestampType(), True),
        t.StructField("grid_area_code", t.StringType(), False),
        t.StructField("from_grid_area_code", t.StringType(), True),
        t.StructField("to_grid_area_code", t.StringType(), True),
        t.StructField("metering_point_type", t.StringType(), False),
        t.StructField("settlement_method", t.StringType(), True),
        t.StructField("energy_supplier_id", t.StringType(), True),
    ]
)
