import pyspark.sql.types as t

nullable = True

charge_price_points_v1 = t.StructType(
    [
        # UUID
        t.StructField("calculation_id", t.StringType(), not nullable),
        #
        # 'wholesale_fixing' | 'aggregation' | 'first_correction_settlement' |
        # 'second_correction_settlement' | 'third_correction_settlement'
        t.StructField("calculation_type", t.StringType(), not nullable),
        #
        t.StructField("calculation_version", t.LongType(), not nullable),
        #
        t.StructField("charge_key", t.StringType(), not nullable),
        #
        t.StructField("charge_code", t.StringType(), not nullable),
        #
        # 'subscription' | 'fee' | 'tariff'
        t.StructField("charge_type", t.StringType(), not nullable),
        #
        # EIC or GLN number
        t.StructField("charge_owner_id", t.StringType(), not nullable),
        #
        # The original type is integer, but in some contexts the quantity type is decimal.
        t.StructField("charge_price", t.DecimalType(), not nullable),
        #
        # UTC time
        t.StructField("charge_time", t.TimestampType(), not nullable),
    ]
)
