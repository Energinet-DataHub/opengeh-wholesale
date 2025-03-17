import pyspark.sql.types as t

from geh_wholesale.databases.table_column_names import (
    TableColumnNames as cname,
)

nullable = True

energy_per_es_schema = t.StructType(
    [
        t.StructField(cname.calculation_id, t.StringType(), not nullable),
        t.StructField(cname.result_id, t.StringType(), not nullable),
        t.StructField(cname.grid_area_code, t.StringType(), not nullable),
        t.StructField(cname.energy_supplier_id, t.StringType(), not nullable),
        t.StructField(
            cname.balance_responsible_party_id,
            t.StringType(),
            not nullable,
        ),
        t.StructField(cname.time_series_type, t.StringType(), not nullable),
        t.StructField(cname.resolution, t.StringType(), not nullable),
        t.StructField(cname.time, t.TimestampType(), not nullable),
        t.StructField(cname.quantity, t.DecimalType(18, 3), not nullable),
        t.StructField(
            cname.quantity_qualities,
            t.ArrayType(t.StringType()),
            not nullable,
        ),
    ]
)
