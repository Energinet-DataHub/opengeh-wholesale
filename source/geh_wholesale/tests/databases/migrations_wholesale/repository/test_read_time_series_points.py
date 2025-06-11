import pathlib
from datetime import datetime
from decimal import Decimal
from unittest import mock
from unittest.mock import patch

import pyspark.sql.functions as f
import pytest
from featuremanagement import FeatureManager
from geh_common.data_products.measurements_core.measurements_gold import current_v1 as measurements_gold_current_v1
from pyspark.sql import SparkSession

from geh_wholesale.constants import Colname
from geh_wholesale.databases.migrations_wholesale import MigrationsWholesaleRepository
from geh_wholesale.databases.migrations_wholesale.repository import measurements_current_v1_schmea
from geh_wholesale.databases.migrations_wholesale.schemas import time_series_points_schema
from geh_wholesale.infrastructure.paths import MigrationsWholesaleDatabase
from tests import SPARK_CATALOG_NAME
from tests.helpers.data_frame_utils import assert_dataframes_equal
from tests.helpers.delta_table_utils import write_dataframe_to_table


def _create_time_series_point_row(
    metering_point_id: str = "some-metering-point-id",
) -> dict:
    return {
        Colname.metering_point_id: metering_point_id,
        Colname.quantity: Decimal("1.123456"),
        Colname.quality: "foo",
        Colname.observation_time: datetime(2022, 6, 8, 22, 0, 0),
    }


class TestWhenContractMismatch:
    def test_raises_assertion_error(self, spark: SparkSession) -> None:
        # Arrange
        row = _create_time_series_point_row()
        repository = create_migration_wholesale_repository()
        df = spark.createDataFrame(data=[row], schema=time_series_points_schema)
        df = df.drop(Colname.metering_point_id)

        # Act
        with mock.patch.object(repository._spark.read.format("delta"), "table", return_value=df):
            with pytest.raises(AssertionError) as exc_info:
                repository.read_time_series_points()

            # Assert
            assert "Schema mismatch" in str(exc_info.value)


class TestWhenValidInput:
    def test_returns_expected_df(
        self,
        spark: SparkSession,
        mock_feature_manager_false: FeatureManager,
        tmp_path: pathlib.Path,
        calculation_input_folder: str,
    ) -> None:
        # Arrange
        calculation_input_path = f"{str(tmp_path)}/{calculation_input_folder}"
        time_series_points_table_location = (
            f"{calculation_input_path}/{MigrationsWholesaleDatabase.TIME_SERIES_POINTS_TABLE_NAME}"
        )
        row = _create_time_series_point_row()
        expected = spark.createDataFrame(data=[row], schema=time_series_points_schema)
        write_dataframe_to_table(
            spark,
            expected,
            "dummy_database_name",
            MigrationsWholesaleDatabase.TIME_SERIES_POINTS_TABLE_NAME,
            time_series_points_table_location,
            time_series_points_schema,
        )

        repository = create_migration_wholesale_repository(spark, mock_feature_manager_false)

        # Act
        actual = repository.read_time_series_points()

        # Assert
        assert_dataframes_equal(actual, expected)


class TestWhenValidInputAndExtraColumns:
    def test_returns_expected_df(self, spark: SparkSession, mock_feature_manager_false: FeatureManager) -> None:
        # Arrange
        row = _create_time_series_point_row()
        repository = create_migration_wholesale_repository(feature_manager_mock=mock_feature_manager_false)
        df = spark.createDataFrame(data=[row], schema=time_series_points_schema)
        df = df.withColumn("test", f.lit("test"))

        # Act & Assert
        with mock.patch.object(repository._spark.read.format("delta"), "table", return_value=df):
            repository.read_time_series_points()


class TestFeatureFlagWhenToggling:
    def test_false_feature_flag_uses_migrations_table(
        self,
        spark: SparkSession,
        mock_feature_manager_false: FeatureManager,
    ) -> None:
        # Arrange
        repository = create_migration_wholesale_repository(spark, mock_feature_manager_false)

        # Act
        with patch(
            "geh_wholesale.databases.migrations_wholesale.repository.read_table",
        ) as read_table_mock:
            repository.read_time_series_points()

            # Assert
            read_table_mock.assert_called_once_with(
                spark,
                SPARK_CATALOG_NAME,
                "dummy_database_name",
                MigrationsWholesaleDatabase.TIME_SERIES_POINTS_TABLE_NAME,
                time_series_points_schema,
            )

    def test_true_feature_flag_uses_measurements_gold_table(
        self,
        spark: SparkSession,
        mock_feature_manager_true: FeatureManager,
    ) -> None:
        # Arrange
        repository = create_migration_wholesale_repository(spark, mock_feature_manager_true)

        # Act
        with patch(
            "geh_wholesale.databases.migrations_wholesale.repository.read_table",
        ) as read_table_mock:
            repository.read_time_series_points()

            # Assert
            read_table_mock.assert_called_once_with(
                spark,
                SPARK_CATALOG_NAME,
                "dummy_database_name2",
                measurements_gold_current_v1.view_name,
                measurements_current_v1_schmea,
            )


def create_migration_wholesale_repository(
    spark: SparkSession = mock.Mock(),
    feature_manager_mock: FeatureManager = mock.Mock(),
    catalog_name: str = SPARK_CATALOG_NAME,
    calculation_input_database_name: str = "dummy_database_name",
    measurements_gold_database_name: str = "dummy_database_name2",
) -> MigrationsWholesaleRepository:
    return MigrationsWholesaleRepository(
        spark,
        feature_manager_mock,
        catalog_name,
        calculation_input_database_name,
        measurements_gold_database_name,
    )
