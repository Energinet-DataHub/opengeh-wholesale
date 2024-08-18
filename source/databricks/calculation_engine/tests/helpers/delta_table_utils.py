# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

from pyspark.sql import SparkSession, DataFrame
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
    print(f"{database_name}.{table_name} write")
    spark.sql(f"CREATE DATABASE IF NOT EXISTS {database_name}")

    sql_schema = _struct_type_to_sql_schema(schema)
    spark.sql(
        f"CREATE TABLE IF NOT EXISTS {database_name}.{table_name} ({sql_schema}) USING DELTA LOCATION '{table_location}'"
    )
    print(sql_schema + "sql schema")
    print(df.schema.simpleString() + "df schema")

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
