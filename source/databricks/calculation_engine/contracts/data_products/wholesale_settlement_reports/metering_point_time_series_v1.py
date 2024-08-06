import pyspark.sql.types as t

nullable = True


metering_point_time_series_v1 = t.StructType(
    [
        # UUID
        t.StructField("calculation_id", t.StringType(), not nullable),
        #
        # 'balance_fixing' | 'wholesale_fixing' | 'first_correction_settlement' |
        # 'second_correction_settlement' | 'third_correction_settlement'
        t.StructField("calculation_type", t.StringType(), not nullable),
        #
        t.StructField("calculation_version", t.LongType(), not nullable),
        #
        # GSRN number
        t.StructField("metering_point_id", t.StringType(), not nullable),
        #
        # 'production' | 'consumption' | 'exchange'
        # When wholesale calculations types also:
        # 've_production' | 'net_production' | 'supply_to_grid' 'consumption_from_grid' |
        # 'wholesale_services_information' | 'own_production' | 'net_from_grid' 'net_to_grid' |
        # 'total_consumption' | 'electrical_heating' | 'net_consumption' | 'effect_settlement'
        t.StructField("metering_point_type", t.StringType(), not nullable),
        #
        # 'PT15M' | 'PT1H'
        t.StructField("resolution", t.StringType(), not nullable),
        #
        t.StructField("grid_area_code", t.StringType(), not nullable),
        #
        # EIC or GLN number
        t.StructField("energy_supplier_id", t.StringType(), nullable),
        #
        # UTC time
        t.StructField(
            "observation_time",
            t.TimestampType(),
            not nullable,
        ),
        t.StructField("quantity", t.DecimalType(18, 3), not nullable),
    ]
)
