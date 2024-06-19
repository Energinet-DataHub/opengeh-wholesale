import pyspark.sql.types as t

price_point = t.StructType(
    [
        t.StructField("time", t.TimestampType(), False),
        t.StructField("price", t.DecimalType(18, 6), False),
    ]
)

charge_prices_v1_schema = t.StructType(
    [
        t.StructField("calculation_id", t.StringType(), False),
        t.StructField("calculation_type", t.StringType(), False),
        t.StructField("calculation_version", t.LongType(), False),
        t.StructField("charge_type", t.StringType(), False),
        t.StructField("charge_code", t.StringType(), False),
        t.StructField("charge_owner_id", t.StringType(), False),
        t.StructField("resolution", t.StringType(), False),
        t.StructField("is_tax", t.BooleanType(), False),
        t.StructField("start_date_time", t.TimestampType(), False),
        t.StructField(
            "price_points",
            t.ArrayType(price_point, False),
            False,
        ),
        t.StructField("grid_area_code", t.StringType(), False),
        t.StructField("energy_supplier_id", t.StringType(), False),
    ]
)
