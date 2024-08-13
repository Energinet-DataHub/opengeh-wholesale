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
        #
        t.StructField("calculation_version", t.LongType(), not nullable),
        #
        t.StructField("grid_area_code", t.StringType(), not nullable),
        #
        # The beginning of a day in the calculation period. Represents midnight in local time but given in UTC.
        t.StructField("from_date", t.TimestampType(), not nullable),
        #
        # The end of a day in the calculation period. Represents midnight in local time but given in UTC.
        # This is exactly one day after the from_date.
        t.StructField("to_date", t.TimestampType(), not nullable),
        #
        # Time (UTC) from which the calculation's results are active
        t.StructField("active_from_time", t.TimestampType(), not nullable),
        #
        # Time (UTC) from which the calculation's results are no longer active. NULL if the calculation is still active.
        t.StructField("active_to_time", t.TimestampType(), nullable),
    ]
)
