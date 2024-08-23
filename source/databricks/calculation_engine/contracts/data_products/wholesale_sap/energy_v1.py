import pyspark.sql.types as t

nullable = True

energy_v1 = t.StructType(
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
        t.StructField("calculation_succeeded_time", t.TimestampType(), not nullable),
        #
        # 'total' | 'es' | 'brp'
        t.StructField("aggregation_level", t.StringType(), not nullable),
        #
        t.StructField("grid_area_code", t.StringType(), not nullable),
        #
        # 'PT15M' | 'PT1H'
        t.StructField("resolution", t.StringType(), not nullable),
        #
        # EIC or GLN number
        t.StructField("energy_supplier_id", t.StringType(), nullable),
        #
        # EIC or GLN number
        t.StructField("balance_responsible_party_id", t.StringType(), nullable),
        #
        t.StructField("neighbor_grid_area_code", t.StringType(), nullable),
        #
        # 'production' | 'non_profiled_consumption' | 'flex_consumption' | 'net_exchange_per_ga' |
        # 'net_exchange_per_neighboring_ga' | 'total_consumption' | 'grid_loss' | 'temp_flex_consumption' |
        # 'temp_production' | 'negative_grid_loss' | 'positive_grid_loss'
        t.StructField("time_series_type", t.StringType(), not nullable),
        #
        # 'consumption' | 'production' | 'exchange' | NULL
        t.StructField("metering_point_type", t.StringType(), nullable),
        #
        # 'flex' | 'non_profiled' | NULL
        t.StructField("settlement_method", t.StringType(), nullable),
        #
        # UTC time
        t.StructField("time", t.TimestampType(), not nullable),
        #
        t.StructField("quantity", t.DecimalType(18, 3), not nullable),
        #
        # [ 'measured' | 'missing' | 'calculated' | 'estimated' ]
        # There is at least one element, and no element is included more than once.
        t.StructField("quantity_qualities", t.ArrayType(t.StringType()), not nullable),
        #
        # 'kWh'
        t.StructField("quantity_unit", t.StringType(), not nullable),
    ]
)
