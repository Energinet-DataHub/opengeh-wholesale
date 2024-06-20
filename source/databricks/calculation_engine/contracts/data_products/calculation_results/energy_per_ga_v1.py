import pyspark.sql.types as t

nullable = True

energy_per_ga_v1 = t.StructType(
    [
        # UUID
        t.StructField("calculation_id", t.StringType(), not nullable),
        #
        # 'balance_fixing' | 'aggregation' | 'wholesale_fixing' | 'first_correction_settlement' |
        # 'second_correction_settlement' | 'third_correction_settlement'
        t.StructField("calculation_type", t.StringType(), not nullable),
        #
        # UTC time
        t.StructField("calculation_period_start", t.TimestampType(), False),
        # UTC time
        t.StructField("calculation_period_end", t.TimestampType(), False),
        t.StructField("calculation_version", t.LongType(), False),
        # UUID
        t.StructField("result_id", t.StringType(), False),
        t.StructField("grid_area_code", t.StringType(), False),
        # 'consumption' | 'production' | 'exchange'
        t.StructField("metering_point_type", t.StringType(), False),
        # 'flex' | 'non_profiled'
        t.StructField("settlement_method", t.StringType(), True),
        # 'PT15M' | 'PT1H'
        t.StructField("resolution", t.StringType(), False),
        # UTC time
        t.StructField("time", t.TimestampType(), False),
        t.StructField("quantity", t.DecimalType(18, 3), False),
        # 'kWh'
        t.StructField("quantity_unit", t.StringType(), False),
        # [ 'measured' | 'missing' | 'calculated' | 'estimated' ]
        # There is at least one element, and no element is included more than once.
        t.StructField("quantity_qualities", t.ArrayType(t.StringType()), False),
    ]
)
