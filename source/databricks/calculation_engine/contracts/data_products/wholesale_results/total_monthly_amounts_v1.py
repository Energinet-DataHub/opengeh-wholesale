import pyspark.sql.types as t

nullable = True

total_monthly_amounts_v1 = t.StructType(
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
        # EIC or GLN number
        t.StructField("charge_owner_id", t.StringType(), nullable),
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
