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

from delta.tables import DeltaTable
from package.infrastructure import initialize_spark
from pyspark.sql import SparkSession
from package.infrastructure.paths import (
    WholesaleResultsInternalDatabase,
    WholesaleBasisDataInternalDatabase,
    WholesaleInternalDatabase,
)
from package.common.logger import Logger
import package.infrastructure.environment_variables as env_vars


def optimise_tables() -> None:
    logger = Logger(__name__)

    spark = initialize_spark()
    catalog_name = env_vars.get_catalog_name()

    schema_table_dicts = {
        f"{catalog_name}.{WholesaleResultsInternalDatabase.DATABASE_NAME}": WholesaleResultsInternalDatabase.TABLE_NAMES,
        f"{catalog_name}.{WholesaleBasisDataInternalDatabase.DATABASE_NAME}": WholesaleBasisDataInternalDatabase.TABLE_NAMES,
        f"{catalog_name}.{WholesaleInternalDatabase.DATABASE_NAME}": WholesaleInternalDatabase.TABLE_NAMES,
    }

    total_tables = sum(len(table_names) for table_names in schema_table_dicts.values())
    logger.info(f"Total number of tables to optimise: {total_tables}")
    for schema_name, table_names in schema_table_dicts.items():
        logger.info(f"Running optimise for tables in schema: {schema_name}")
        for table_name in table_names:
            optimise_table(spark, schema_name, table_name, logger)


def optimise_table(
    spark: SparkSession, schema_name: str, table_name: str, logger: Logger
) -> None:
    full_table_name = f"{schema_name}.{table_name}"
    try:
        logger.info(f"Starting to optimise table: {full_table_name}")
        delta_table = DeltaTable.forName(spark, f"{full_table_name}")
        delta_table.optimize().executeCompaction()
        logger.info(f"Finished optimising table: {full_table_name}")
    except Exception as e:
        logger.error(f"Failed to optimise table: {full_table_name}. Error: {e}")
