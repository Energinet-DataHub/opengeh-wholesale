import pyspark.sql.types as t

nullable = True

charge_price_information_periods_v1 = t.StructType(
    [
        # UUID
        t.StructField("calculation_id", t.StringType(), not nullable),
        #
        # 'wholesale_fixing' | 'aggregation' |'wholesale_fixing' | 'first_correction_settlement' |
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
        t.StructField("resolution", t.StringType(), not nullable),
        #
        # Taxation
        t.StructField("is_tax", t.BooleanType(), not nullable),
        #
        # UTC time
        t.StructField("from_date", t.TimestampType(), not nullable),
        #
        # UTC time
        t.StructField("to_date", t.TimestampType(), nullable),
    ]
)
