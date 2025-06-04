import pyspark.sql.types as t

from geh_wholesale.databases.table_column_names import TableColumnNames

amounts_per_charge_schema = t.StructType(
    [
        t.StructField(TableColumnNames.calculation_id, t.StringType(), False),
        t.StructField(TableColumnNames.result_id, t.StringType(), False),
        t.StructField(TableColumnNames.grid_area_code, t.StringType(), False),
        t.StructField(TableColumnNames.energy_supplier_id, t.StringType(), False),
        t.StructField(TableColumnNames.quantity, t.DecimalType(18, 3), False),
        t.StructField(TableColumnNames.quantity_unit, t.StringType(), False),
        t.StructField(
            TableColumnNames.quantity_qualities,
            t.ArrayType(t.StringType()),
            True,
        ),
        t.StructField(TableColumnNames.time, t.TimestampType(), False),
        t.StructField(TableColumnNames.resolution, t.StringType(), False),
        t.StructField(TableColumnNames.metering_point_type, t.StringType(), False),
        t.StructField(TableColumnNames.settlement_method, t.StringType(), True),
        t.StructField(TableColumnNames.price, t.DecimalType(18, 6), True),
        t.StructField(TableColumnNames.amount, t.DecimalType(18, 6), True),
        t.StructField(TableColumnNames.is_tax, t.BooleanType(), False),
        t.StructField(TableColumnNames.charge_code, t.StringType(), False),
        t.StructField(TableColumnNames.charge_type, t.StringType(), False),
        t.StructField(TableColumnNames.charge_owner_id, t.StringType(), False),
    ]
)
