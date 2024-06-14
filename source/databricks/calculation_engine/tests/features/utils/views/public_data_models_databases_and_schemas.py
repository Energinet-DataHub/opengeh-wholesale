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
from typing import List

from pyspark.sql import SparkSession
from pyspark.sql.catalog import Database


def get_public_data_model_databases(spark: SparkSession) -> List[Database]:
    """
    Get all view databases.
    """
    negative_databases = {
        "default",
        "schema_migration",
        "wholesale_output",
        "wholesale_input",
        "basis_data",
    }
    databases = [
        db for db in spark.catalog.listDatabases() if db.name not in negative_databases
    ]

    if not databases:
        raise ValueError("No databases found.")

    return databases


def get_expected_public_data_model_schemas() -> dict:
    schemas = {}
    current_directory = os.getcwd()
    schemas_folder = current_directory + "/features/utils/views/expected_schemas"

    for root, _, files in os.walk(schemas_folder):
        for file_name in files:
            if file_name.endswith(".py") and not file_name.startswith("__"):
                schema_name = file_name[
                    :-3
                ]  # Remove the '.py' extension to get the module name
                module_path = os.path.join(root, file_name)

                # Import the module dynamically
                spec = importlib.util.spec_from_file_location(schema_name, module_path)
                module = importlib.util.module_from_spec(spec)
                spec.loader.exec_module(module)

                if hasattr(module, schema_name):
                    view_name = schema_name.replace("_schema", "")
                    schemas[view_name] = getattr(module, schema_name)
                else:
                    raise AttributeError(
                        f"Module {module} does not have the expected schema variable {schema_name}"
                    )

    return schemas
