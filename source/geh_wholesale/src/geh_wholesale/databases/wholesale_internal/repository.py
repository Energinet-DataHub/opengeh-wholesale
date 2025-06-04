from pyspark.sql import DataFrame, SparkSession
from pyspark.sql.functions import col

from geh_wholesale.infrastructure.paths import (
    WholesaleInternalDatabase,
)

from ..repository_helper import read_table
from ..table_column_names import TableColumnNames
from .schemas import (
    calculations_schema,
    grid_loss_metering_point_ids_schema,
)


class WholesaleInternalRepository:
    def __init__(
        self,
        spark: SparkSession,
        catalog_name: str,
        grid_loss_metering_point_ids_table_name: str | None = None,
    ) -> None:
        self._spark = spark
        self._catalog_name = catalog_name
        self._grid_loss_metering_point_ids_table_name = (
            grid_loss_metering_point_ids_table_name or WholesaleInternalDatabase.GRID_LOSS_METERING_POINT_IDS_TABLE_NAME
        )

    def read_grid_loss_metering_point_ids(self) -> DataFrame:
        return read_table(
            self._spark,
            self._catalog_name,
            WholesaleInternalDatabase.DATABASE_NAME,
            self._grid_loss_metering_point_ids_table_name,
            grid_loss_metering_point_ids_schema,
        )

    def read_calculations(self) -> DataFrame:
        return read_table(
            self._spark,
            self._catalog_name,
            WholesaleInternalDatabase.DATABASE_NAME,
            WholesaleInternalDatabase.CALCULATIONS_TABLE_NAME,
            calculations_schema,
        )

    def get_by_calculation_id(self, calculation_id: str) -> DataFrame:
        return self.read_calculations().where(col(TableColumnNames.calculation_id) == calculation_id)
