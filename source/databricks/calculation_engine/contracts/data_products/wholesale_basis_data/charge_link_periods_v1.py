import pyspark.sql.types as t

nullable = True

charge_link_periods_v1 = t.StructType(
    [
        # UUID
        t.StructField("calculation_id", t.StringType(), not nullable),
        #
        # 'wholesale_fixing' | 'first_correction_settlement' |
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
        # GSRN number
        t.StructField("metering_point_id", t.StringType(), not nullable),
        #
        # The original type is integer, but in some contexts the quantity type is decimal.
        t.StructField("quantity", t.IntegerType(), not nullable),
        #
        # UTC time
        t.StructField("from_date", t.TimestampType(), not nullable),
        #
        # UTC time
        t.StructField("to_date", t.TimestampType(), nullable),
    ]
)