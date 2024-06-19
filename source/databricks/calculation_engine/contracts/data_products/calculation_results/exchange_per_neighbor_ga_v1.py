import pyspark.sql.types as t

exchange_per_neighbor_ga_v1 = t.StructType(
    [
        # UUID
        t.StructField("calculation_id", t.StringType(), False),
        # 'balance_fixing' | 'aggregation'
        t.StructField("calculation_type", t.StringType(), False),
        # UTC time
        t.StructField("calculation_period_start", t.TimestampType(), False),
        # UTC time
        t.StructField("calculation_period_end", t.TimestampType(), False),
        t.StructField("calculation_version", t.LongType(), False),
        t.StructField("in_grid_area_code", t.StringType(), False),
        t.StructField("out_grid_area_code", t.StringType(), False),
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
