import pyspark.sql.types as t

amounts_per_charge_v1 = t.StructType(
    [
        # UUID
        t.StructField("calculation_id", t.StringType(), False),
        # 'wholesale_fixing' | 'first_correction_settlement' |
        # 'second_correction_settlement' | 'third_correction_settlement'
        t.StructField("calculation_type", t.StringType(), False),
        t.StructField("calculation_version", t.LongType(), False),
        # UUID
        t.StructField("result_id", t.StringType(), False),
        t.StructField("grid_area_code", t.StringType(), False),
        # EIC or GLN number
        t.StructField("energy_supplier_id", t.StringType(), False),
        t.StructField("charge_code", t.StringType(), False),
        # 'tariff' | 'subscription' | 'fee'
        t.StructField("charge_type", t.StringType(), False),
        # EIC or GLN number
        t.StructField("charge_owner_id", t.StringType(), False),
        # 'PT1H' | 'P1D'
        t.StructField("resolution", t.StringType(), False),
        # 'kWh' | 'pcs'
        t.StructField("quantity_unit", t.StringType(), False),
        # 'production' | 'consumption' | 've_production' | 'net_production' |
        # 'supply_to_grid' | 'consumption_from_grid' | 'wholesale_services_information' |
        # 'own_production' | 'net_from_grid' 'net_to_grid' | 'total_consumption' |
        # 'electrical_heating' | 'net_consumption' | 'effect_settlement'
        t.StructField("metering_point_type", t.StringType(), False),
        # 'flex' | 'non_profiled'
        t.StructField("settlement_method", t.StringType(), True),
        t.StructField("is_tax", t.BooleanType(), False),
        # 'DKK'
        t.StructField("currency", t.StringType(), False),
        # UTC time
        t.StructField("time", t.TimestampType(), False),
        t.StructField("quantity", t.DecimalType(18, 3), False),
        # [ 'measured' | 'missing' | 'calculated' | 'estimated' ]
        # If not NULL: There is at least one element, and no element is included more than once.
        t.StructField("quantity_qualities", t.ArrayType(t.StringType()), True),
        t.StructField("price", t.DecimalType(18, 6), True),
        t.StructField("amount", t.DecimalType(18, 6), True),
    ]
)
