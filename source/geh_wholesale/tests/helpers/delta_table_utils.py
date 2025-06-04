from pyspark.sql import DataFrame, SparkSession
from pyspark.sql.types import StructType


def write_dataframe_to_table(
    spark: SparkSession,
    df: DataFrame,
    database_name: str,
    table_name: str,
    table_location: str,
    schema: StructType,
    mode: str = "overwrite",
) -> None:
    print(f"{database_name}.{table_name} write")  # noqa: T201
    spark.sql(f"CREATE DATABASE IF NOT EXISTS {database_name}")

    sql_schema = _struct_type_to_sql_schema(schema)
    spark.sql(
        f"CREATE TABLE IF NOT EXISTS {database_name}.{table_name} ({sql_schema}) USING DELTA LOCATION '{table_location}'"
    )

    df.write.format("delta").mode(mode).saveAsTable(f"{database_name}.{table_name}")


def _struct_type_to_sql_schema(schema: StructType) -> str:
    schema_string = ""
    for field in schema.fields:
        field_name = field.name
        field_type = field.dataType.simpleString()

        if not field.nullable:
            field_type += " NOT NULL"

        schema_string += f"{field_name} {field_type}, "

    # Remove the trailing comma and space
    schema_string = schema_string.rstrip(", ")
    return schema_string
