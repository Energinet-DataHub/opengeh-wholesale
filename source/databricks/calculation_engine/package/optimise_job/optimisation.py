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
)


def optimise_tables() -> None:
    spark = initialize_spark()

    database_dict = {
        WholesaleResultsInternalDatabase.DATABASE_NAME: WholesaleResultsInternalDatabase.TABLE_NAMES,
        WholesaleBasisDataInternalDatabase.DATABASE_NAME: WholesaleBasisDataInternalDatabase.TABLE_NAMES,
    }

    for database_name, table_names in database_dict.items():
        for table_name in table_names:
            optimise_table(spark, database_name, table_name)


def optimise_table(spark: SparkSession, database_name: str, table_name: str) -> None:
    print(f"{database_name}.{table_name} optimise")
    delta_table = DeltaTable.forName(spark, f"{database_name}.{table_name}")
    delta_table.optimize().executeCompaction()
