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
            WholesaleResultsInternalDatabase.DATABASE_NAME,
            WholesaleResultsInternalDatabase.ENERGY_TABLE_NAME,
            energy_schema,
        )

    def read_energy_per_es(self) -> DataFrame:
        return read_table(
            self._spark,
            self._catalog_name,
            WholesaleResultsInternalDatabase.DATABASE_NAME,
            WholesaleResultsInternalDatabase.ENERGY_PER_ES_TABLE_NAME,
            energy_per_es_schema,
        )

    def read_energy_per_brp(self) -> DataFrame:
        return read_table(
            self._spark,
            self._catalog_name,
            WholesaleResultsInternalDatabase.DATABASE_NAME,
            WholesaleResultsInternalDatabase.ENERGY_PER_BRP_TABLE_NAME,
            energy_per_brp_schema,
        )

    def read_amount_per_charge(self) -> DataFrame:
        return read_table(
            self._spark,
            self._catalog_name,
            WholesaleResultsInternalDatabase.DATABASE_NAME,
            WholesaleResultsInternalDatabase.AMOUNTS_PER_CHARGE_TABLE_NAME,
            amounts_per_charge_schema,
        )
