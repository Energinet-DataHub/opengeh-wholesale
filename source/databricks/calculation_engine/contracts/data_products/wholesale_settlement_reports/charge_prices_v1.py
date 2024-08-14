import pyspark.sql.types as t

nullable = True

price_point = t.StructType(
    [
        #
        t.StructField("time", t.TimestampType(), not nullable),
        #
        t.StructField("price", t.DecimalType(18, 6), not nullable),
    ]
)

charge_prices_v1 = t.StructType(
    [
        # UUID
        t.StructField("calculation_id", t.StringType(), not nullable),
        #
        # 'subscription' | 'fee' | 'tariff'
        t.StructField("charge_type", t.StringType(), not nullable),
        #
        t.StructField("charge_code", t.StringType(), not nullable),
        #
        # EIC or GLN number
        t.StructField("charge_owner_id", t.StringType(), not nullable),
        #
        # 'PT1H' | 'P1D' | 'P1M'
        t.StructField("resolution", t.StringType(), not nullable),
        #
        t.StructField("is_tax", t.BooleanType(), not nullable),
        #
        # UTC time
        t.StructField("start_date_time", t.TimestampType(), not nullable),
        #
        # [ (time, price) ]
        t.StructField(
            "price_points",
            t.ArrayType(price_point, not nullable),
            not nullable,
        ),
        #
        t.StructField("grid_area_code", t.StringType(), not nullable),
        #
        # EIC or GLN number
        t.StructField("energy_supplier_id", t.StringType(), nullable),
    ]
)
