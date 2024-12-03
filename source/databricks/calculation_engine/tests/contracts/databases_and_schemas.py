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
import importlib.util
import os
from importlib.util import spec_from_file_location
from pathlib import Path

from pyspark.sql import SparkSession


def get_views_from_database(database_name: str, spark: SparkSession) -> list:
    tables = spark.catalog.listTables(database_name)
    views = [table for table in tables if table.tableType == "VIEW"]
    assert views, f"No views found in database {database_name}."
    return views


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
                    raise ImportError(
                        f"Failed to import module from path '{module_path}'."
                    )

                module = importlib.util.module_from_spec(spec)
                spec.loader.exec_module(module)

                if hasattr(module, schema_name):
                    schemas[f"{database_name}.{schema_name}"] = getattr(
                        module, schema_name
                    )
                else:
                    raise AttributeError(
                        f"The data product '{module}' does not define the expected contract '{schema_name}'"
                    )

    return schemas
