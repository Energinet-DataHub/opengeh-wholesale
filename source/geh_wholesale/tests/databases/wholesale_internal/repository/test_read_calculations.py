import pathlib
import uuid
from datetime import datetime
from unittest import mock

import pyspark.sql.functions as f
import pytest
from pyspark.sql import SparkSession

from geh_wholesale.codelists import CalculationType
from geh_wholesale.databases import wholesale_internal
from geh_wholesale.databases.table_column_names import TableColumnNames
from geh_wholesale.databases.wholesale_internal.schemas import (
    calculations_schema,
)
from geh_wholesale.infrastructure.paths import (
    WholesaleInternalDatabase,
)
from tests import SPARK_CATALOG_NAME
from tests.helpers.data_frame_utils import assert_dataframes_equal
from tests.helpers.delta_table_utils import write_dataframe_to_table


def _create_calculation_row() -> dict:
    return {
        TableColumnNames.calculation_id: str(uuid.uuid4()),
        TableColumnNames.calculation_type: CalculationType.BALANCE_FIXING.value,
        TableColumnNames.calculation_period_start: datetime(2022, 6, 8, 22, 0, 0),
        TableColumnNames.calculation_period_end: datetime(2022, 6, 9, 22, 0, 0),
        TableColumnNames.calculation_version: 1,
        TableColumnNames.calculation_execution_time_start: datetime(2022, 6, 8, 22, 0, 0),
        TableColumnNames.is_internal_calculation: False,
    }


class TestWhenContractMismatch:
    def test_raises_assertion_error(self, spark: SparkSession) -> None:
        # Arrange
        row = _create_calculation_row()
        repository = wholesale_internal.WholesaleInternalRepository(
            mock.Mock(), "dummy_calculation_input_path", "dummy_catalog_name"
        )

        df = spark.createDataFrame(data=[row], schema=calculations_schema)
        df = df.drop(TableColumnNames.calculation_id)
        df = df.withColumn("test", f.lit("test"))

        # Act & Assert
        with mock.patch.object(repository._spark.read.format("delta"), "table", return_value=df):
            with pytest.raises(AssertionError) as exc_info:
                repository.read_calculations()

            assert "Schema mismatch" in str(exc_info.value)


class TestWhenValidInput:
    def test_returns_expected_df(
        self,
        spark: SparkSession,
        tmp_path: pathlib.Path,
        calculation_input_folder: str,
    ) -> None:
        # Arrange
        calculation_input_path = f"{str(tmp_path)}/{calculation_input_folder}"
        calculations_table_location = f"{calculation_input_path}/{WholesaleInternalDatabase.CALCULATIONS_TABLE_NAME}"
        row = _create_calculation_row()
        df = spark.createDataFrame(data=[row], schema=calculations_schema)
        write_dataframe_to_table(
            spark,
            df,
            WholesaleInternalDatabase.DATABASE_NAME,
            WholesaleInternalDatabase.CALCULATIONS_TABLE_NAME,
            calculations_table_location,
            calculations_schema,
        )
        expected = df

        repository = wholesale_internal.WholesaleInternalRepository(spark, SPARK_CATALOG_NAME)

        # Act
        actual = repository.read_calculations()

        # Assert
        assert_dataframes_equal(actual, expected)
