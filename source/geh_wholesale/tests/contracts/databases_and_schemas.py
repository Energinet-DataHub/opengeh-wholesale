import importlib.util
import os
from importlib.util import spec_from_file_location
from pathlib import Path

from pyspark.sql import SparkSession


def get_views_from_database(database_name: str, spark: SparkSession) -> list:
    return _get_from_database(database_name, spark, "VIEW")


def get_tables_from_database(database_name: str, spark: SparkSession) -> list:
    return _get_from_database(database_name, spark, "TABLE")


def _get_from_database(database_name: str, spark: SparkSession, table_type: str) -> list:
    tables = spark.catalog.listTables(database_name)
    tables = [table for table in tables if table.tableType == table_type]
    return tables


def get_expected_schemas(folder: str) -> dict:
    schemas = {}
    current_directory = Path(__file__).parent
    schemas_folder = current_directory / ".." / ".." / "contracts" / folder

    for root, _, files in os.walk(schemas_folder):
        database_name = Path(root).name
        for file_name in files:
            if file_name.endswith(".py") and not file_name.startswith("__init__"):
                # Remove the file extension
                schema_name = file_name[:-3]

                module_path = os.path.join(root, file_name)
                spec = spec_from_file_location(schema_name, module_path)
                if spec is None:
                    raise ImportError(f"Failed to import module from path '{module_path}'.")

                module = importlib.util.module_from_spec(spec)
                spec.loader.exec_module(module)

                if hasattr(module, schema_name):
                    schemas[f"{database_name}.{schema_name}"] = getattr(module, schema_name)
                else:
                    raise AttributeError(
                        f"The data product '{module}' does not define the expected contract '{schema_name}'"
                    )

    return schemas
