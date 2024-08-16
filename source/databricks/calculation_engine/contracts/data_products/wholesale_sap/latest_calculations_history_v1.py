import pyspark.sql.types as t

nullable = True

latest_calculations_history_v1 = t.StructType(
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
        #
        # The beginning of a day in the calculation period. Represents midnight in local time but given in UTC.
        # The date is inclusive.
        t.StructField("from_date", t.TimestampType(), not nullable),
        #
        # The end of a day in the calculation period. Represents midnight in local time but given in UTC.
        # This is exactly one day after the from_date. This date is exclusive.
        t.StructField("to_date", t.TimestampType(), not nullable),
        #
        # Time (UTC) from which the calculation's results are active. This time is inclusive.
        t.StructField("latest_from_time", t.TimestampType(), not nullable),
        #
        # Time (UTC) from which the calculation's results are no longer active. This time is exclusive.
        # NULL means that the calculation is still active.
        t.StructField("latest_to_time", t.TimestampType(), nullable),
    ]
)
