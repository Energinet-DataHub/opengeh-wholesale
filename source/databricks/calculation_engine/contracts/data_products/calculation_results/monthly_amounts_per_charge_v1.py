import pyspark.sql.types as t

nullable = True

monthly_amounts_per_charge_v1 = t.StructType(
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
        # UUID
        t.StructField("result_id", t.StringType(), not nullable),
        #
        t.StructField("grid_area_code", t.StringType(), not nullable),
        #
        # EIC or GLN number
        t.StructField("energy_supplier_id", t.StringType(), not nullable),
        #
        t.StructField("charge_code", t.StringType(), not nullable),
        #
        # 'tariff' | 'subscription' | 'fee'
        t.StructField("charge_type", t.StringType(), not nullable),
        #
        # EIC or GLN number
        t.StructField("charge_owner_id", t.StringType(), not nullable),
        #
        # 'kWh' | 'pcs'
        t.StructField("quantity_unit", t.StringType(), not nullable),
        #
        t.StructField("is_tax", t.BooleanType(), not nullable),
        #
        # 'DKK'
        t.StructField("currency", t.StringType(), not nullable),
        #
        # UTC time
        t.StructField("time", t.TimestampType(), not nullable),
        #
        t.StructField("amount", t.DecimalType(18, 6), nullable),
    ]
)
