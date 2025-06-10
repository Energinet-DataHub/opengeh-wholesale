import pathlib
from datetime import datetime
from decimal import Decimal
from unittest import mock

import pyspark.sql.functions as f
import pytest
from featuremanagement import FeatureManager
from pyspark.sql import SparkSession

from geh_wholesale.constants import Colname
from geh_wholesale.databases.migrations_wholesale import MigrationsWholesaleRepository
from geh_wholesale.databases.migrations_wholesale.schemas import charge_price_points_schema
from geh_wholesale.infrastructure.paths import MigrationsWholesaleDatabase
from tests import SPARK_CATALOG_NAME
from tests.helpers.data_frame_utils import assert_dataframes_equal
from tests.helpers.delta_table_utils import write_dataframe_to_table

DEFAULT_OBSERVATION_TIME = datetime(2022, 6, 8, 22, 0, 0)
DEFAULT_FROM_DATE = datetime(2022, 6, 8, 22, 0, 0)
DEFAULT_TO_DATE = datetime(2022, 6, 8, 22, 0, 0)


def _create_change_price_point_row() -> dict:
    return {
        Colname.charge_code: "foo",
        Colname.charge_type: "foo",
        Colname.charge_owner: "foo",
        Colname.charge_price: Decimal("1.123456"),
        Colname.charge_time: DEFAULT_OBSERVATION_TIME,
    }


class TestWhenContractMismatch:
    def test__raises_assertion_error(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange
        row = _create_change_price_point_row()
        reader = MigrationsWholesaleRepository(
            mock.Mock(),
            mock.Mock(),
            "dummy_catalog_name",
            "dummy_database_name",
            "dummy_database_name2",
        )
        df = spark.createDataFrame(data=[row], schema=charge_price_points_schema)
        df = df.drop(Colname.charge_code)

        # Act & Assert
        with mock.patch.object(reader._spark.read.format("delta"), "table", return_value=df):
            with pytest.raises(AssertionError) as exc_info:
                reader.read_charge_price_points()

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
        table_location = f"{calculation_input_path}/{MigrationsWholesaleDatabase.CHARGE_PRICE_POINTS_TABLE_NAME}"
        row = _create_change_price_point_row()
        df = spark.createDataFrame(data=[row], schema=charge_price_points_schema)
        write_dataframe_to_table(
            spark,
            df,
            "test_database",
            MigrationsWholesaleDatabase.CHARGE_PRICE_POINTS_TABLE_NAME,
            table_location,
            charge_price_points_schema,
        )
        expected = df
        reader = MigrationsWholesaleRepository(
            spark, mock_feature_manager_false, SPARK_CATALOG_NAME, "test_database", "test_database2"
        )

        # Act
        actual = reader.read_charge_price_points()

        # Assert
        assert_dataframes_equal(actual, expected)


class TestWhenValidInputAndExtraColumns:
    def test__returns_expected_df(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange
        row = _create_change_price_point_row()
        reader = MigrationsWholesaleRepository(
            mock.Mock(),
            mock.Mock(),
            "dummy_catalog_name",
            "dummy_database_name",
            "dummy_database_name2",
        )
        df = spark.createDataFrame(data=[row], schema=charge_price_points_schema)
        df = df.withColumn("test", f.lit("test"))

        # Act & Assert
        with mock.patch.object(reader._spark.read.format("delta"), "table", return_value=df):
            reader.read_charge_price_points()
