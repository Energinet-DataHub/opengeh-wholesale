import pyspark.sql.types as t

nullable = True

monthly_amounts_per_charge_v1 = t.StructType(
    [
        # UUID
        t.StructField("calculation_id", t.StringType(), not nullable),
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
        # 'kWh' | 'pcs'
        t.StructField("quantity_unit", t.StringType(), not nullable),
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
        #
        # Taxation
        t.StructField("is_tax", t.BooleanType(), not nullable),
    ]
)
