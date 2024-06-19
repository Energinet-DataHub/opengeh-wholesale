import pyspark.sql.types as t

total_monthly_amounts_v1_schema = t.StructType(
    [
        t.StructField("calculation_id", t.StringType(), False),
        t.StructField("calculation_type", t.StringType(), False),
        t.StructField("calculation_version", t.LongType(), False),
        t.StructField("result_id", t.StringType(), False),
        t.StructField("grid_area_code", t.StringType(), False),
        t.StructField("energy_supplier_id", t.StringType(), False),
        t.StructField("charge_owner_id", t.StringType(), False),
        t.StructField("currency", t.StringType(), False),
        t.StructField("time", t.TimestampType(), False),
        t.StructField("amount", t.DecimalType(18, 6), False),
    ]
)
