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
from pyspark.sql import DataFrame, SparkSession
from pyspark.sql.types import StructType

from package.common.schemas import assert_contract, assert_schema
from package.infrastructure.paths import (
    WholesaleInternalDatabase,
    HiveBasisDataDatabase,
)
from .schemas import hive_calculations_schema, grid_loss_metering_points_schema


class TableReader:
    def __init__(
        self,
        spark: SparkSession,
        catalog_name: str,
        grid_loss_metering_points_table_name: str | None = None,
    ) -> None:
        self._spark = spark
        self._catalog_name = catalog_name
        self._grid_loss_metering_points_table_name = (
            grid_loss_metering_points_table_name
            or WholesaleInternalDatabase.GRID_LOSS_METERING_POINTS_TABLE_NAME
        )

    def read_grid_loss_metering_points(self) -> DataFrame:
        return _read_from_uc(
            self._spark,
            self._catalog_name,
            WholesaleInternalDatabase.DATABASE_NAME,
            self._grid_loss_metering_points_table_name,
            grid_loss_metering_points_schema,
        )

    def read_calculations(self) -> DataFrame:
        table_name = f"{HiveBasisDataDatabase.DATABASE_NAME}.{HiveBasisDataDatabase.CALCULATIONS_TABLE_NAME}"
        df = self._spark.read.format("delta").table(table_name)

        # Though it's our own table, we still want to make sure it has the expected schema as
        # it might have been changed due to e.g. (failed) migrations or backup/restore.
        assert_schema(df.schema, hive_calculations_schema)

        return df


def _read_from_uc(
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

    return df
