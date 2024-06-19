import pyspark.sql.types as t

charge_link_periods_v1 = t.StructType(
    [
        t.StructField("calculation_id", t.StringType(), False),
        t.StructField("calculation_type", t.StringType(), False),
        t.StructField("calculation_version", t.LongType(), False),
        t.StructField("metering_point_id", t.StringType(), False),
        t.StructField("metering_point_type", t.StringType(), False),
        t.StructField("charge_type", t.StringType(), False),
        t.StructField("charge_code", t.StringType(), False),
        t.StructField("charge_owner_id", t.StringType(), False),
        t.StructField("charge_link_quantity", t.IntegerType(), False),
        t.StructField("from_date", t.TimestampType(), False),
        t.StructField("to_date", t.TimestampType(), True),
        t.StructField("grid_area_code", t.StringType(), False),
        # The business logic is that the field should not be null,
        # but the field can be null in the database.
        t.StructField("energy_supplier_id", t.StringType(), False),
    ]
)
