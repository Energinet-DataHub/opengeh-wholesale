import pyspark.sql.types as t

nullable = True

wholesale_results_v1 = t.StructType(
    [
        # UUID
        t.StructField("calculation_id", t.StringType(), not nullable),
        #
        #
        # 'wholesale_fixing' | 'first_correction_settlement' |
        # 'second_correction_settlement' | 'third_correction_settlement'
        t.StructField("calculation_type", t.StringType(), not nullable),
        #
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
        # UTC time
        t.StructField("time", t.TimestampType(), not nullable),
        #
        # ‘PT1H' | ‘P1D’
        t.StructField("resolution", t.StringType(), not nullable),
        #
        # ‘production' | ‘consumption' | 'exchange' | 've_production' |
        # 'net_production' | 'supply_to_grid' 'consumption_from_grid' |
        # 'wholesale_services_information’ | 'own_production’ | 'net_from_grid' |
        # 'net_to_grid' | 'total_consumption' | 'electrical_heating' |
        # 'net_consumption' | 'effect_settlement'
        t.StructField("metering_point_type", t.StringType(), not nullable),
        #
        # ‘flex' | ’non_profiled'
        t.StructField("settlement_method", t.StringType(), nullable),
        #
        # 'kWh' | 'pcs'
        t.StructField("quantity_unit", t.StringType(), not nullable),
        #
        # 'DKK'
        t.StructField("currency", t.StringType(), not nullable),
        #
        t.StructField("quantity", t.DecimalType(18, 3), not nullable),
        #
        t.StructField("price", t.DecimalType(18, 6), not nullable),
        #
        t.StructField("amount", t.DecimalType(18, 6), nullable),
        #
        # 'subscription' | 'fee' | 'tariff'
        t.StructField("charge_type", t.StringType(), nullable),
        #
        t.StructField("charge_code", t.StringType(), nullable),
        #
        # EIC or GLN number
        t.StructField("charge_owner_id", t.StringType(), nullable),
    ]
)
