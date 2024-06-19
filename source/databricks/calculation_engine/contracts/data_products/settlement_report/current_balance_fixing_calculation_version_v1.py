import pyspark.sql.types as t

current_balance_fixing_calculation_version_v1 = t.StructType(
    [
        t.StructField("calculation_version", t.LongType(), False),
    ]
)
