import pyspark.sql.types as t

nullable = True

exchange_per_neighbor_v1 = t.StructType(
    [
        # UUID
        t.StructField("calculation_id", t.StringType(), not nullable),
        #
        # 'balance_fixing' | 'aggregation'
        t.StructField("calculation_type", t.StringType(), not nullable),
        #
        # UTC time
        t.StructField("calculation_period_start", t.TimestampType(), not nullable),
        #
        # UTC time
        t.StructField("calculation_period_end", t.TimestampType(), not nullable),
        #
        t.StructField("grid_area_code", t.StringType(), not nullable),
        #
        t.StructField("neighbor_grid_area_code", t.StringType(), not nullable),
        #
        # 'PT15M' | 'PT1H'
        t.StructField("resolution", t.StringType(), not nullable),
        #
        # UTC time
        t.StructField("time", t.TimestampType(), not nullable),
        #
        t.StructField("quantity", t.DecimalType(18, 3), not nullable),
        #
        # 'kWh'
        t.StructField("quantity_unit", t.StringType(), not nullable),
        #
        # [ 'measured' | 'missing' | 'calculated' | 'estimated' ]
        # There is at least one element, and no element is included more than once.
        t.StructField("quantity_qualities", t.ArrayType(t.StringType()), not nullable),
    ]
)
