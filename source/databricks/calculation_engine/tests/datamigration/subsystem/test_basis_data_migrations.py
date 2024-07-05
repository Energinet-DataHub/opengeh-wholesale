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
import pytest
from pyspark.sql import SparkSession

from package.infrastructure.paths import WholesaleBasisDataDatabase


@pytest.mark.parametrize(
    "table_name",
    WholesaleBasisDataDatabase.TABLE_NAMES,
)
def test__basis_data_table__is_managed(
    spark: SparkSession, migrations_executed: None, table_name: str
) -> None:
    """
    It has been decided that all Delta Tables in the system should be managed, since it gives several benefits
    such enabling more Databricks features and ensuring that access rights are only managed by Unity Catalog
    """

    table_description = spark.sql(
        f"DESCRIBE EXTENDED {WholesaleBasisDataDatabase.DATABASE_NAME}.{table_name}"
    )

    is_managed = any(
        prop["col_name"] == "Type" and prop["data_type"] == "MANAGED"
        for prop in table_description.collect()
    )

    assert is_managed
