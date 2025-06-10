import pathlib
from datetime import datetime
from unittest import mock

import pyspark.sql.functions as f
import pytest
from featuremanagement import FeatureManager
from pyspark.sql import SparkSession

from geh_wholesale.constants import Colname
from geh_wholesale.databases.migrations_wholesale import MigrationsWholesaleRepository
from geh_wholesale.databases.migrations_wholesale.schemas import charge_link_periods_schema
from geh_wholesale.infrastructure.paths import MigrationsWholesaleDatabase
from tests import SPARK_CATALOG_NAME
from tests.helpers.data_frame_utils import assert_dataframes_equal
from tests.helpers.delta_table_utils import write_dataframe_to_table

DEFAULT_FROM_DATE = datetime(2022, 6, 8, 22, 0, 0)
DEFAULT_TO_DATE = datetime(2022, 6, 8, 22, 0, 0)


def _create_charge_link_period_row() -> dict:
    return {
        Colname.charge_code: "foo",
        Colname.charge_type: "foo",
        Colname.charge_owner: "foo",
        Colname.metering_point_id: "foo",
        Colname.quantity: 1,
        Colname.from_date: DEFAULT_FROM_DATE,
        Colname.to_date: DEFAULT_TO_DATE,
    }


class TestWhenContractMismatch:
    def test__raises_assertion_error(
        self,
        spark: SparkSession,
    ) -> None:
        # Arrange
        row = _create_charge_link_period_row()
        reader = MigrationsWholesaleRepository(
            mock.Mock(),
            mock.Mock(),
            "dummy_catalog_name",
            "dummy_database_name",
            "dummy_database_name2",
        )
        df = spark.createDataFrame(data=[row], schema=charge_link_periods_schema)
        df = df.drop(Colname.charge_type)

        # Act & Assert
        with mock.patch.object(reader._spark.read.format("delta"), "table", return_value=df):
            with pytest.raises(AssertionError) as exc_info:
                reader.read_charge_link_periods()

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
        table_location = f"{calculation_input_path}/{MigrationsWholesaleDatabase.CHARGE_LINK_PERIODS_TABLE_NAME}"
        row = _create_charge_link_period_row()
        df = spark.createDataFrame(data=[row], schema=charge_link_periods_schema)
        write_dataframe_to_table(
            spark,
            df,
            "test_database",
            MigrationsWholesaleDatabase.CHARGE_LINK_PERIODS_TABLE_NAME,
            table_location,
            charge_link_periods_schema,
        )
        expected = df
        reader = MigrationsWholesaleRepository(
            spark, mock_feature_manager_false, SPARK_CATALOG_NAME, "test_database", "test_database"
        )

        # Act
        actual = reader.read_charge_link_periods()

        # Assert
        assert_dataframes_equal(actual, expected)


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
        row = _create_charge_link_period_row()
        df = spark.createDataFrame(data=[row], schema=charge_link_periods_schema)
        df = df.withColumn("test", f.lit("test"))

        # Act & Assert
        with mock.patch.object(reader._spark.read.format("delta"), "table", return_value=df):
            reader.read_charge_link_periods()
