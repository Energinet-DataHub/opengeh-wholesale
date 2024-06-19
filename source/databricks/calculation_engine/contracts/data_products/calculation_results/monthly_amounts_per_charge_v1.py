import pyspark.sql.types as t

monthly_amounts_per_charge_v1 = t.StructType(
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
        # 'kWh' | 'pcs'
        t.StructField("quantity_unit", t.StringType(), False),
        t.StructField("is_tax", t.BooleanType(), False),
        # 'DKK'
        t.StructField("currency", t.StringType(), False),
        # UTC time
        t.StructField("time", t.TimestampType(), False),
        t.StructField("amount", t.DecimalType(18, 6), True),
    ]
)
