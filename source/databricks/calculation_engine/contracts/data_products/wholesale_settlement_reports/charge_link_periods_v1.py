import pyspark.sql.types as t

nullable = True

charge_link_periods_v1 = t.StructType(
    [
        # UUID
        t.StructField("calculation_id", t.StringType(), not nullable),
        #
        # 'wholesale_fixing' | 'first_correction_settlement' |
        # 'second_correction_settlement' | 'third_correction_settlement'
        t.StructField("calculation_type", t.StringType(), not nullable),
        #
        t.StructField("calculation_version", t.LongType(), not nullable),
        #
        # GSRN number
        t.StructField("metering_point_id", t.StringType(), not nullable),
        #
        # 'production' | 'consumption' | 'exchange' | 've_production' |
        # 'net_production' | 'supply_to_grid' 'consumption_from_grid' |
        # 'wholesale_services_information' | 'own_production' | 'net_from_grid' |
        # 'net_to_grid' | 'total_consumption' | 'electrical_heating' |
        # 'net_consumption' | 'capacity_settlement'
        t.StructField("metering_point_type", t.StringType(), not nullable),
        #
        # 'subscription' | 'fee' | 'tariff'
        t.StructField("charge_type", t.StringType(), not nullable),
        #
        t.StructField("charge_code", t.StringType(), not nullable),
        #
        # EIC or GLN number
        t.StructField("charge_owner_id", t.StringType(), not nullable),
        #
        t.StructField("charge_link_quantity", t.IntegerType(), not nullable),
        #
        # UTC time
        t.StructField("from_date", t.TimestampType(), not nullable),
        #
        # UTC time
        t.StructField("to_date", t.TimestampType(), nullable),
        #
        t.StructField("grid_area_code", t.StringType(), not nullable),
        #
        # EIC or GLN number
        t.StructField("energy_supplier_id", t.StringType(), nullable),
        #
        # Taxation
        t.StructField("is_tax", t.BooleanType(), not nullable),
    ]
)
