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

from geh_common.testing.dataframes.assert_schemas import assert_contract
from pyspark.sql import DataFrame, SparkSession
from pyspark.sql.types import StructType


def read_table(
    spark: SparkSession,
    catalog_name: str,
    database_name: str,
    table_name: str,
    contract: StructType,
) -> DataFrame:
    name = f"{catalog_name}.{database_name}.{table_name}"
    df = spark.read.format("delta").table(name)

    # Assert that the schema of the data matches the defined contract
    assert_contract(df.schema, contract)

    # Select only the columns that are defined in the contract to avoid potential downstream issues
    df = df.select(contract.fieldNames())

    # When reading Parquet/Delta files, all columns are automatically converted to be nullable for compatibility reasons
    # See 'https://github.com/delta-io/delta/issues/873#issuecomment-1012426632' for more context
    # This is a workaround to ensure that the schema of the DataFrame matches the contract
    df.schema = contract

    return df
