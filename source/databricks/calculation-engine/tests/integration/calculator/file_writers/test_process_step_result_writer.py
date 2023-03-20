# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

from datetime import datetime
from decimal import Decimal
from pathlib import Path

import package.infrastructure as infra
import pytest
from package.codelists import (
    AggregationLevel,
    MeteringPointResolution,
    TimeSeriesQuality,
    TimeSeriesType,
)
from package.constants import Colname
from package.file_writers.process_step_result_writer import ProcessStepResultWriter
from pyspark.sql import SparkSession
from pyspark.sql.functions import lit
from tests.helpers.assert_calculation_file_path import (
    CalculationFileType,
    assert_file_path_match_contract,
)
from tests.helpers.file_utils import find_file

ACTORS_FOLDER = "actors"
DEFAULT_BATCH_ID = "0b15a420-9fc8-409a-a169-fbd49479d718"
DEFAULT_GRID_AREA = "105"
DEFAULT_ENERGY_SUPPLIER_ID = "9876543210123"
DEFAULT_BALANCE_RESPONSIBLE_ID = "1234567890123"


def _create_result_row(
    grid_area: str,
    energy_supplier_id: str = DEFAULT_ENERGY_SUPPLIER_ID,
    balance_responsible_id: str = DEFAULT_BALANCE_RESPONSIBLE_ID,
    quantity: str = "1.1",
    quality: TimeSeriesQuality = TimeSeriesQuality.measured,
) -> dict:
    row = {
        Colname.grid_area: grid_area,
        Colname.sum_quantity: Decimal(quantity),
        Colname.quality: quality.value,
        Colname.resolution: MeteringPointResolution.quarter.value,
        Colname.time_window: {
            Colname.start: datetime(2020, 1, 1, 0, 0),
            Colname.end: datetime(2020, 1, 1, 1, 0),
        },
        Colname.energy_supplier_id: energy_supplier_id,
        Colname.balance_responsible_id: balance_responsible_id,
    }

    return row


def test__write___when_aggregation_level_is_es_per_ga__result_file_path_matches_contract(
    spark: SparkSession,
    contracts_path: str,
    tmpdir: Path,
) -> None:
    # Arrange
    row = [
        _create_result_row(
            grid_area=DEFAULT_GRID_AREA, energy_supplier_id=DEFAULT_ENERGY_SUPPLIER_ID
        )
    ]
    result_df = spark.createDataFrame(data=row)
    relative_output_path = infra.get_result_file_relative_path(
        DEFAULT_BATCH_ID,
        DEFAULT_GRID_AREA,
        DEFAULT_ENERGY_SUPPLIER_ID,
        None,
        TimeSeriesType.NON_PROFILED_CONSUMPTION,
        AggregationLevel.es_per_ga,
    )
    sut = ProcessStepResultWriter(str(tmpdir), DEFAULT_BATCH_ID)

    # Act
    sut.write(
        result_df,
        TimeSeriesType.NON_PROFILED_CONSUMPTION,
        AggregationLevel.es_per_ga,
    )

    # Assert
    actual_result_file = find_file(
        f"{str(tmpdir)}/",
        f"{relative_output_path}/part-*.json",
    )
    assert_file_path_match_contract(
        contracts_path,
        actual_result_file,
        CalculationFileType.ResultFile,
    )


def test__write___when_aggregation_level_is_total_ga__result_file_path_matches_contract(
    spark: SparkSession,
    contracts_path: str,
    tmpdir: Path,
) -> None:
    # Arrange
    row = [_create_result_row(grid_area=DEFAULT_GRID_AREA, energy_supplier_id="None")]
    result_df = spark.createDataFrame(data=row)
    relative_output_path = infra.get_result_file_relative_path(
        DEFAULT_BATCH_ID,
        DEFAULT_GRID_AREA,
        None,
        None,
        TimeSeriesType.PRODUCTION,
        AggregationLevel.total_ga,
    )
    sut = ProcessStepResultWriter(str(tmpdir), DEFAULT_BATCH_ID)

    # Act
    sut.write(
        result_df,
        TimeSeriesType.PRODUCTION,
        AggregationLevel.total_ga,
    )

    # Assert
    actual_result_file = find_file(
        f"{str(tmpdir)}/",
        f"{relative_output_path}/part-*.json",
    )
    assert_file_path_match_contract(
        contracts_path,
        actual_result_file,
        CalculationFileType.ResultFileForTotalGridArea,
    )


def test__write___when_aggregation_level_is_ga_brp_es__result_file_path_matches_contract(
    spark: SparkSession,
    contracts_path: str,
    tmpdir: Path,
) -> None:
    # Arrange
    row = [
        _create_result_row(
            grid_area=DEFAULT_GRID_AREA,
            energy_supplier_id=DEFAULT_ENERGY_SUPPLIER_ID,
            balance_responsible_id=DEFAULT_BALANCE_RESPONSIBLE_ID,
        )
    ]
    result_df = spark.createDataFrame(data=row)
    relative_output_path = infra.get_result_file_relative_path(
        DEFAULT_BATCH_ID,
        DEFAULT_GRID_AREA,
        DEFAULT_ENERGY_SUPPLIER_ID,
        DEFAULT_BALANCE_RESPONSIBLE_ID,
        TimeSeriesType.PRODUCTION,
        AggregationLevel.es_per_brp_per_ga,
    )
    sut = ProcessStepResultWriter(str(tmpdir), DEFAULT_BATCH_ID)

    # Act
    sut.write(
        result_df,
        TimeSeriesType.PRODUCTION,
        AggregationLevel.es_per_brp_per_ga,
    )

    # Assert
    actual_result_file = find_file(
        f"{str(tmpdir)}/",
        f"{relative_output_path}/part-*.json",
    )
    assert_file_path_match_contract(
        contracts_path,
        actual_result_file,
        CalculationFileType.ResultFileForGaBrpEs,
    )


@pytest.mark.parametrize(
    "aggregation_level",
    AggregationLevel,
)
def test__write__writes_aggregation_level_column(
    spark: SparkSession, tmpdir: Path, aggregation_level: AggregationLevel
) -> None:
    # Arrange
    table_name = "result_table"
    spark.sql(
        f"DROP TABLE IF EXISTS {table_name}"
    )  # needed to avoid conflict between parametrized tests
    row = [
        _create_result_row(
            grid_area=DEFAULT_GRID_AREA,
            energy_supplier_id=DEFAULT_ENERGY_SUPPLIER_ID,
            balance_responsible_id=DEFAULT_BALANCE_RESPONSIBLE_ID,
        )
    ]
    result_df = spark.createDataFrame(data=row)
    sut = ProcessStepResultWriter(str(tmpdir), DEFAULT_BATCH_ID)

    # Act
    sut.write(
        result_df,
        TimeSeriesType.PRODUCTION,
        aggregation_level,
    )

    # Assert
    actual_df = spark.read.table(table_name)
    assert actual_df.collect()[0]["AggregationLevel"] == aggregation_level.value


@pytest.mark.parametrize(
    "time_series_type",
    TimeSeriesType,
)
def test__write__writes_time_series_type_column(
    spark: SparkSession, tmpdir: Path, time_series_type: TimeSeriesType
) -> None:
    # Arrange
    table_name = "result_table"
    spark.sql(
        f"DROP TABLE IF EXISTS {table_name}"
    )  # needed to avoid conflict between parametrized tests

    row = [
        _create_result_row(
            grid_area=DEFAULT_GRID_AREA,
            energy_supplier_id=DEFAULT_ENERGY_SUPPLIER_ID,
            balance_responsible_id=DEFAULT_BALANCE_RESPONSIBLE_ID,
        )
    ]
    result_df = spark.createDataFrame(data=row)
    sut = ProcessStepResultWriter(str(tmpdir), DEFAULT_BATCH_ID)

    # Act
    sut.write(
        result_df,
        time_series_type,
        AggregationLevel.total_ga,
    )

    # Assert
    actual_df = spark.read.table(table_name)
    assert actual_df.collect()[0]["time_series_type"] == time_series_type.value


def test__write__writes_batch_id(spark: SparkSession, tmpdir: Path) -> None:
    # Arrange
    table_name = "result_table"
    spark.sql(
        f"DROP TABLE IF EXISTS {table_name}"
    )  # needed to avoid conflict between parametrized tests

    row = [
        _create_result_row(
            grid_area=DEFAULT_GRID_AREA,
            energy_supplier_id=DEFAULT_ENERGY_SUPPLIER_ID,
            balance_responsible_id=DEFAULT_BALANCE_RESPONSIBLE_ID,
        )
    ]
    result_df = spark.createDataFrame(data=row)
    sut = ProcessStepResultWriter(str(tmpdir), DEFAULT_BATCH_ID)

    # Act
    sut.write(
        result_df,
        TimeSeriesType.NON_PROFILED_CONSUMPTION,
        AggregationLevel.total_ga,
    )

    # Assert
    actual_df = spark.read.table(table_name)
    assert actual_df.collect()[0]["batch_id"] == DEFAULT_BATCH_ID


def test__write_result_to_table__when_schema_differs_from_table__raise_exception(
    spark: SparkSession, tmpdir: Path
) -> None:
    # Arrange
    table_name = "result_table"
    spark.sql(
        f"DROP TABLE IF EXISTS {table_name}"
    )  # needed to avoid conflict between parametrized tests

    row = [
        _create_result_row(
            grid_area=DEFAULT_GRID_AREA,
            energy_supplier_id=DEFAULT_ENERGY_SUPPLIER_ID,
            balance_responsible_id=DEFAULT_BALANCE_RESPONSIBLE_ID,
        )
    ]
    result_df_1 = spark.createDataFrame(data=row)
    result_df_2 = result_df_1.withColumn("extra_column", lit("some_value"))
    sut = ProcessStepResultWriter(str(tmpdir), DEFAULT_BATCH_ID)
    sut._write_result_to_table(result_df_1, AggregationLevel.total_ga)

    # Act and Assert
    with pytest.raises(Exception):
        sut._write_result_to_table(result_df_2, AggregationLevel.total_ga)
