import pathlib
from unittest import mock

import pyspark.sql.functions as f
import pytest
from featuremanagement import FeatureManager
from pyspark.sql import SparkSession

import tests.databases.migrations_wholesale.repository.input_metering_point_periods_factory as factory
from geh_wholesale.constants import Colname
from geh_wholesale.databases.migrations_wholesale import MigrationsWholesaleRepository
from geh_wholesale.databases.migrations_wholesale.schemas import metering_point_periods_schema
from geh_wholesale.infrastructure.paths import MigrationsWholesaleDatabase
from tests import SPARK_CATALOG_NAME
from tests.helpers.data_frame_utils import assert_dataframes_equal
from tests.helpers.delta_table_utils import write_dataframe_to_table


class TestWhenValidInput:
    def test_returns_expected_df(
        self,
        spark: SparkSession,
        tmp_path: pathlib.Path,
        calculation_input_folder: str,
        mock_feature_manager_false: FeatureManager,
    ) -> None:
        # Arrange
        calculation_input_path = f"{str(tmp_path)}/{calculation_input_folder}"
        table_location = f"{calculation_input_path}/{MigrationsWholesaleDatabase.METERING_POINT_PERIODS_TABLE_NAME}"
        row = factory.create_row()
        df = factory.create(spark, row)
        write_dataframe_to_table(
            spark,
            df,
            "test_database",
            MigrationsWholesaleDatabase.METERING_POINT_PERIODS_TABLE_NAME,
            table_location,
            metering_point_periods_schema,
        )
        reader = MigrationsWholesaleRepository(
            spark, mock_feature_manager_false, SPARK_CATALOG_NAME, "test_database", "test_database"
        )

        # Act
        actual = reader.read_metering_point_periods()

        # Assert
        assert_dataframes_equal(actual, df)


class TestWhenValidInputAndMoreColumns:
    def test_raises_assertion_error(self, spark: SparkSession) -> None:
        # Arrange
        reader = MigrationsWholesaleRepository(
            mock.Mock(),
            mock.Mock(),
            "dummy_catalog_name",
            "dummy_database_name",
            "dummy_database_name2",
        )
        row = factory.create_row()
        df = factory.create(spark, row)
        df = df.withColumn("test", f.lit("test"))

        # Act & Assert
        with mock.patch.object(reader._spark.read.format("delta"), "table", return_value=df):
            reader.read_metering_point_periods()


class TestWhenContractMismatch:
    def test_raises_assertion_error(self, spark: SparkSession) -> None:
        # Arrange
        reader = MigrationsWholesaleRepository(
            mock.Mock(),
            mock.Mock(),
            "dummy_catalog_name",
            "dummy_database_name",
            "dummy_database_name2",
        )
        row = factory.create_row()
        df = factory.create(spark, row)
        df = df.drop(Colname.metering_point_id)

        # Act & Assert
        with mock.patch.object(reader._spark.read.format("delta"), "table", return_value=df):
            with pytest.raises(AssertionError) as exc_info:
                reader.read_metering_point_periods()

            assert "Schema mismatch" in str(exc_info.value)
