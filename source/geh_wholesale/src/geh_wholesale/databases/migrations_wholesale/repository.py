from featuremanagement import FeatureManager
from geh_common.data_products.measurements_core.measurements_gold.current_v1 import (
    schema as measurements_current_v1_schmea,
)
from pyspark.sql import DataFrame, SparkSession

from geh_wholesale.constants import Colname
from geh_wholesale.databases.feature_flag_manager import FeatureFlags
from geh_wholesale.infrastructure.paths import (
    MeasurementsGoldDatabase,
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
        feature_manager: FeatureManager,
        catalog_name: str,
        calculation_input_database_name: str,
        measurements_gold_database_name: str,
        time_series_points_table_name: str | None = None,
        metering_point_periods_table_name: str | None = None,
        grid_loss_metering_point_ids_table_name: str | None = None,
        measurements_current_v1_view_name: str | None = None,
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

        self._measurements_gold_database_name = measurements_gold_database_name
        self._measurements_current_v1_table_name = (
            measurements_current_v1_view_name or MeasurementsGoldDatabase.CURRENT_V1
        )
        self._feature_manager = feature_manager

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
        # This a temporary release toggle (feature flag).
        # If the flag is enabled time series points are fetched from measurements gold table,
        # otherwise from migrations table.
        if self._feature_manager.is_enabled(FeatureFlags.measuredata_measurements):  # type: ignore
            df = read_table(
                self._spark,
                self._catalog_name,
                self._measurements_gold_database_name,
                self._measurements_current_v1_table_name,
                measurements_current_v1_schmea,
            )

            # Later in the flow assert_schema will fails if the column "metering_point_type" is present.
            # Therefore, it is dropped here.
            if "metering_point_type" in df.columns:
                df = df.drop("metering_point_type")

            # Reorder the columns to match time series point schema.
            df = df.select(
                Colname.metering_point_id,
                Colname.quantity,
                Colname.quality,
                Colname.observation_time,
            )

            return df

        else:
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
