import pyspark.sql.types as t

nullable = True

succeeded_external_calculations_v1 = t.StructType(
    [
        # UUID
        t.StructField("calculation_id", t.StringType(), not nullable),
        #
        # 'balance_fixing' | 'aggregation' | 'wholesale_fixing' | 'first_correction_settlement' |
        # 'second_correction_settlement' | 'third_correction_settlement'
        t.StructField("calculation_type", t.StringType(), not nullable),
        #
        # UTC time
        t.StructField("calculation_period_start", t.TimestampType(), not nullable),
        #
        # UTC time. The time is exclusive.
        t.StructField("calculation_period_end", t.TimestampType(), not nullable),
        #
        # Number series per calculation type. Starts from number 1.
        t.StructField("calculation_version", t.LongType(), not nullable),
    ]
)
