import pyspark.sql.types as t

nullable = True

current_balance_fixing_calculation_version_v1 = t.StructType(
    [
        # BitInt
        t.StructField("calculation_version", t.LongType(), not nullable),
    ]
)
