import pyspark.sql.types as t

energy_per_brp_ga_v1 = t.StructType(
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
        # UUID
        t.StructField("result_id", t.StringType(), False),
        t.StructField("grid_area_code", t.StringType(), False),
        # EIC or GLN number
        t.StructField("balance_responsible_party_id", t.StringType(), False),
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
