import pyspark.sql.types as t

nullable = True

latest_calculations_by_day_v1 = t.StructType(
    [
        # UUID
        t.StructField("calculation_id", t.StringType(), not nullable),
        #
        # 'aggregation' | 'balance_fixing' | 'wholesale_fixing' | 'first_correction_settlement' |
        # 'second_correction_settlement' | 'third_correction_settlement'
        t.StructField("calculation_type", t.StringType(), not nullable),
        #
        t.StructField("calculation_version", t.LongType(), not nullable),
        #
        t.StructField("grid_area_code", t.StringType(), not nullable),
        t.StructField("start_of_day", t.TimestampType(), not nullable),
    ]
)
