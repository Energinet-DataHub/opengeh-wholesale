import pyspark.sql.types as t

element = t.StructType(
    [
        t.StructField("observation_time", t.TimestampType(), False),
        t.StructField("quantity", t.DecimalType(18, 3), False),
    ]
)

metering_point_time_series_v1 = t.StructType(
    [
        t.StructField("calculation_id", t.StringType(), False),
        t.StructField("calculation_type", t.StringType(), False),
        t.StructField("calculation_version", t.LongType(), False),
        t.StructField("metering_point_id", t.StringType(), False),
        t.StructField("metering_point_type", t.StringType(), False),
        t.StructField("resolution", t.StringType(), False),
        t.StructField("grid_area_code", t.StringType(), False),
        t.StructField("energy_supplier_id", t.StringType(), True),
        t.StructField(
            "start_date_time",
            t.TimestampType(),
            False,
        ),
        t.StructField(
            "quantities",
            t.ArrayType(element, False),
            False,
        ),
    ]
)
