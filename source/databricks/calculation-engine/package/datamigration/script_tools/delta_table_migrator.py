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

from pyspark.sql import SparkSession


class DeltaTableMigrator:
    "Tool for applying various kinds of migrations to a Delta table"

    def __init__(
        self,
        spark: SparkSession,
        table_name: str,
    ) -> None:
        self.__spark__ = spark
        self.__table_name__ = table_name

    def apply_sql(self, sql_statements: list[str]) -> None:
        """Applies the given SQL statements to the table.

        Supports the following placeholders:
        - `{table_name}`: The name of the delta table
        """
        for sql in sql_statements:
            sql = sql.replace("{table_name}", self.__table_name__)
            self.__spark__.sql(sql)
