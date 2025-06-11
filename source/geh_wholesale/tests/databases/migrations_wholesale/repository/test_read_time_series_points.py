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

from geh_wholesale.codelists import MeteringPointType
from geh_wholesale.constants import Colname
from geh_wholesale.databases.migrations_wholesale import MigrationsWholesaleRepository
from geh_wholesale.databases.migrations_wholesale.schemas import time_series_points_schema
from geh_wholesale.infrastructure.paths import MigrationsWholesaleDatabase
from tests import SPARK_CATALOG_NAME
from tests.helpers.data_frame_utils import assert_dataframes_equal, assert_schema
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


def _create_current_v1_row(
    metering_point_id: str = "some-metering-point-id",
) -> dict:
    return {
        Colname.metering_point_id: metering_point_id,
        Colname.quantity: Decimal("1.123456"),
        Colname.quality: "foo",
        Colname.observation_time: datetime(2022, 6, 8, 22, 0, 0),
        "metering_point_type": MeteringPointType.CONSUMPTION.value,
    }


class TestWhenContractMismatch:
    def test_raises_assertion_error(self, spark: SparkSession) -> None:
        # Arrange
        row = _create_time_series_point_row()
        reader = MigrationsWholesaleRepository(
            mock.Mock(),
            mock.Mock(),
            "dummy_catalog_name",
            "dummy_database_name",
            "dummy_database_name2",
        )
        df = spark.createDataFrame(data=[row], schema=time_series_points_schema)
        df = df.drop(Colname.metering_point_id)

        # Act & Assert
        with mock.patch.object(reader._spark.read.format("delta"), "table", return_value=df):
            with pytest.raises(AssertionError) as exc_info:
                reader.read_time_series_points()

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
        df = spark.createDataFrame(data=[row], schema=time_series_points_schema)
        write_dataframe_to_table(
            spark,
            df,
            "test_database",
            MigrationsWholesaleDatabase.TIME_SERIES_POINTS_TABLE_NAME,
            time_series_points_table_location,
            time_series_points_schema,
        )
        expected = df

        reader = MigrationsWholesaleRepository(
            spark, mock_feature_manager_false, SPARK_CATALOG_NAME, "test_database", "test_database2"
        )

        # Act
        actual = reader.read_time_series_points()

        # Assert
        assert_dataframes_equal(actual, expected)


class TestWhenValidInputAndExtraColumns:
    def test_returns_expected_df(self, spark: SparkSession, mock_feature_manager_false: FeatureManager) -> None:
        # Arrange
        row = _create_time_series_point_row()
        reader = MigrationsWholesaleRepository(
            mock.Mock(),
            mock_feature_manager_false,
            "dummy_catalog_name",
            "dummy_database_name",
            "dummy_database_name2",
        )
        df = spark.createDataFrame(data=[row], schema=time_series_points_schema)
        df = df.withColumn("test", f.lit("test"))

        # Act & Assert
        with mock.patch.object(reader._spark.read.format("delta"), "table", return_value=df):
            reader.read_time_series_points()


class TestFeatureFlagWhenToggling:
    def test_false_feature_flag_uses_migrations_table(
        self,
        spark: SparkSession,
        mock_feature_manager_false: FeatureManager,
    ) -> None:
        # Arrange
        reader = MigrationsWholesaleRepository(
            spark, mock_feature_manager_false, SPARK_CATALOG_NAME, "test_database", "test_database2"
        )

        # Act
        with patch(
            "geh_wholesale.databases.migrations_wholesale.repository.read_table",
        ) as read_table_mock:
            reader.read_time_series_points()

            # Assert
            read_table_mock.assert_called_once_with(
                mock.ANY,
                SPARK_CATALOG_NAME,
                "test_database",
                MigrationsWholesaleDatabase.TIME_SERIES_POINTS_TABLE_NAME,
                time_series_points_schema,
            )

    def test_true_feature_flag_uses_measurements_gold_table(
        self,
        spark: SparkSession,
        mock_feature_manager_true: FeatureManager,
    ) -> None:
        # Arrange
        repository = MigrationsWholesaleRepository(
            spark, mock_feature_manager_true, SPARK_CATALOG_NAME, "test_database", "test_database2"
        )

        # Act
        with patch(
            "geh_wholesale.databases.migrations_wholesale.repository.read_table",
        ) as read_table_mock:
            repository.read_time_series_points()

            # Assert
            read_table_mock.assert_called_once_with(
                mock.ANY,
                SPARK_CATALOG_NAME,
                "test_database2",
                measurements_gold_current_v1.view_name,
                measurements_current_v1_schmea,
            )

    def test_dropping_column_when_reading_from_measurements_gold_table(
        self,
        spark: SparkSession,
        mock_feature_manager_true: FeatureManager,
    ) -> None:
        # Arrange
        row = _create_current_v1_row()
        df = spark.createDataFrame(data=[row], schema=measurements_gold_current_v1.schema)

        with mock.patch("geh_wholesale.databases.migrations_wholesale.repository.read_table", return_value=df):
            repository = MigrationsWholesaleRepository(
                spark, mock_feature_manager_true, SPARK_CATALOG_NAME, "test_database", "test_database2"
            )

            # Act
            actual = repository.read_time_series_points()

            # Assert
            assert "metering_point_type" not in actual.columns
            assert_schema(actual.schema, time_series_points_schema)
