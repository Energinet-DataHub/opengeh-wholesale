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

from geh_wholesale.infrastructure.paths import (
    WholesaleResultsInternalDatabase,
)

from ..repository_helper import read_table
from ..wholesale_results_internal.schemas import (
    amounts_per_charge_schema,
    energy_per_brp_schema,
    energy_per_es_schema,
    energy_schema,
)


class WholesaleResultsInternalRepository:
    def __init__(
        self,
        spark: SparkSession,
        catalog_name: str,
    ) -> None:
        self._spark = spark
        self._catalog_name = catalog_name

    def read_energy(self) -> DataFrame:
        return read_table(
            self._spark,
            self._catalog_name,
            WholesaleResultsInternalDatabase().DATABASE_WHOLESALE_RESULTS_INTERNAL,
            WholesaleResultsInternalDatabase().ENERGY_TABLE_NAME,
            energy_schema,
        )

    def read_energy_per_es(self) -> DataFrame:
        return read_table(
            self._spark,
            self._catalog_name,
            WholesaleResultsInternalDatabase().DATABASE_WHOLESALE_RESULTS_INTERNAL,
            WholesaleResultsInternalDatabase().ENERGY_PER_ES_TABLE_NAME,
            energy_per_es_schema,
        )

    def read_energy_per_brp(self) -> DataFrame:
        return read_table(
            self._spark,
            self._catalog_name,
            WholesaleResultsInternalDatabase().DATABASE_WHOLESALE_RESULTS_INTERNAL,
            WholesaleResultsInternalDatabase().ENERGY_PER_BRP_TABLE_NAME,
            energy_per_brp_schema,
        )

    def read_amount_per_charge(self) -> DataFrame:
        return read_table(
            self._spark,
            self._catalog_name,
            WholesaleResultsInternalDatabase().DATABASE_WHOLESALE_RESULTS_INTERNAL,
            WholesaleResultsInternalDatabase().AMOUNTS_PER_CHARGE_TABLE_NAME,
            amounts_per_charge_schema,
        )
