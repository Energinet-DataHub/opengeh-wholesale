from pyspark.sql import DataFrame, SparkSession

from geh_wholesale.infrastructure.paths import (
    MigrationsWholesaleDatabase,
    WholesaleInternalDatabase,
)

from ..repository_helper import read_table
from .schemas import (
    charge_link_periods_schema,
    charge_price_information_periods_schema,
    charge_price_points_schema,
    metering_point_periods_schema,
    time_series_points_schema,
)


class MigrationsWholesaleRepository:
    def __init__(
        self,
        spark: SparkSession,
        catalog_name: str,
        calculation_input_database_name: str,
        time_series_points_table_name: str | None = None,
        metering_point_periods_table_name: str | None = None,
        grid_loss_metering_point_ids_table_name: str | None = None,
    ) -> None:
        self._spark = spark
        self._catalog_name = catalog_name
        self._calculation_input_database_name = calculation_input_database_name
        self._time_series_points_table_name = (
            time_series_points_table_name or MigrationsWholesaleDatabase.TIME_SERIES_POINTS_TABLE_NAME
        )
        self._metering_point_periods_table_name = (
            metering_point_periods_table_name or MigrationsWholesaleDatabase.METERING_POINT_PERIODS_TABLE_NAME
        )
        self._grid_loss_metering_point_ids_table_name = (
            grid_loss_metering_point_ids_table_name or WholesaleInternalDatabase.GRID_LOSS_METERING_POINT_IDS_TABLE_NAME
        )

    def read_metering_point_periods(
        self,
    ) -> DataFrame:
        return read_table(
            self._spark,
            self._catalog_name,
            self._calculation_input_database_name,
            self._metering_point_periods_table_name,
            metering_point_periods_schema,
        )

    def read_time_series_points(self) -> DataFrame:
        return read_table(
            self._spark,
            self._catalog_name,
            self._calculation_input_database_name,
            self._time_series_points_table_name,
            time_series_points_schema,
        )

    def read_charge_link_periods(self) -> DataFrame:
        return read_table(
            self._spark,
            self._catalog_name,
            self._calculation_input_database_name,
            MigrationsWholesaleDatabase.CHARGE_LINK_PERIODS_TABLE_NAME,
            charge_link_periods_schema,
        )

    def read_charge_price_information_periods(self) -> DataFrame:
        return read_table(
            self._spark,
            self._catalog_name,
            self._calculation_input_database_name,
            MigrationsWholesaleDatabase.CHARGE_PRICE_INFORMATION_PERIODS_TABLE_NAME,
            charge_price_information_periods_schema,
        )

    def read_charge_price_points(
        self,
    ) -> DataFrame:
        return read_table(
            self._spark,
            self._catalog_name,
            self._calculation_input_database_name,
            MigrationsWholesaleDatabase.CHARGE_PRICE_POINTS_TABLE_NAME,
            charge_price_points_schema,
        )
