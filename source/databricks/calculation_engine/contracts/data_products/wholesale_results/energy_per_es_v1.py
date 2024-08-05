import pyspark.sql.types as t

nullable = True

energy_per_es_v1 = t.StructType(
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
        # UTC time
        t.StructField("calculation_period_end", t.TimestampType(), not nullable),
        t.StructField("calculation_version", t.LongType(), not nullable),
        #
        # UUID
        t.StructField("result_id", t.StringType(), not nullable),
        #
        t.StructField("grid_area_code", t.StringType(), not nullable),
        #
        # EIC or GLN number
        t.StructField("energy_supplier_id", t.StringType(), not nullable),
        #
        # EIC or GLN number
        t.StructField("balance_responsible_party_id", t.StringType(), not nullable),
        #
        # 'consumption' | 'production'
        t.StructField("metering_point_type", t.StringType(), not nullable),
        #
        # 'flex' | 'non_profiled'
        t.StructField("settlement_method", t.StringType(), nullable),
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
