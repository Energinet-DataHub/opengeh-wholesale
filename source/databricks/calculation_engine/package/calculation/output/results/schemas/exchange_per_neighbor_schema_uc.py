import pyspark.sql.types as t

from package.constants import EnergyResultColumnNames as cname

nullable = True

exchange_per_neighbor_schema_uc = t.StructType(
    [
        t.StructField(cname.calculation_id, t.StringType(), not nullable),
        t.StructField(cname.result_id, t.StringType(), not nullable),
        t.StructField(cname.grid_area_code, t.StringType(), not nullable),
        t.StructField(cname.neighbor_grid_area_code, t.StringType(), not nullable),
        t.StructField(cname.resolution, t.StringType(), not nullable),
        t.StructField(cname.time, t.TimestampType(), not nullable),
        t.StructField(cname.quantity, t.DecimalType(18, 3), not nullable),
        t.StructField(
            cname.quantity_qualities, t.ArrayType(t.StringType()), not nullable
        ),
    ]
)
