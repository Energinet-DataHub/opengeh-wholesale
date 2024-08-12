import pyspark.sql.types as t

nullable = True

active_calculations_v1 = t.StructType(
    [
        # UUID
        t.StructField("calculation_id", t.StringType(), not nullable),
        #
        # 'aggregation' | 'balance_fixing' | 'wholesale_fixing' | 'first_correction_settlement' |
        # 'second_correction_settlement' | 'third_correction_settlement'
        t.StructField("calculation_type", t.StringType(), not nullable),
        t.StructField("calculation_version", t.LongType(), not nullable),
        t.StructField("grid_area_code", t.StringType(), not nullable),
        t.StructField("date", t.TimestampType(), not nullable),
        t.StructField("active_from_date", t.TimestampType(), not nullable),
        t.StructField("active_to_date", t.TimestampType(), nullable),
    ]
)
