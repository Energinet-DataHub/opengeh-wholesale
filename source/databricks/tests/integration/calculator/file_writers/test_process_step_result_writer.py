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
from delta.tables import DeltaTable
from package.codelists import (
    Grouping,
    MeteringPointResolution,
    TimeSeriesQuality,
    TimeSeriesType,
)
from package.constants import Colname
from package.file_writers.process_step_result_writer import ProcessStepResultWriter
from pyspark.sql import SparkSession
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


def test__write___when_grouping_is_es_per_ga__result_file_path_matches_contract(
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
        Grouping.es_per_ga,
    )
    sut = ProcessStepResultWriter(str(tmpdir), DEFAULT_BATCH_ID)

    # Act: Executed in fixture executed_calculation_job
    sut.write(
        result_df,
        TimeSeriesType.NON_PROFILED_CONSUMPTION,
        Grouping.es_per_ga,
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


def test__write___when_grouping_is_total_ga__result_file_path_matches_contract(
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
        Grouping.total_ga,
    )
    sut = ProcessStepResultWriter(str(tmpdir), DEFAULT_BATCH_ID)

    # Act: Executed in fixture executed_calculation_job
    sut.write(
        result_df,
        TimeSeriesType.PRODUCTION,
        Grouping.total_ga,
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


def test__write___when_grouping_is_ga_brp_es__result_file_path_matches_contract(
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
        Grouping.es_per_brp_per_ga,
    )
    sut = ProcessStepResultWriter(str(tmpdir), DEFAULT_BATCH_ID)

    # Act: Executed in fixture executed_calculation_job
    sut.write(
        result_df,
        TimeSeriesType.PRODUCTION,
        Grouping.es_per_brp_per_ga,
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
    "grouping",
    Grouping,
)
def test__write__writes_grouping_column(
    spark: SparkSession, tmpdir: Path, grouping: Grouping
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

    # Act: Executed in fixture executed_calculation_job
    sut.write(
        result_df,
        TimeSeriesType.PRODUCTION,
        grouping,
    )

    # Assert
    actual_df = spark.read.table(table_name)
    actual_df.show()
    assert actual_df.collect()[0]["grouping"] == grouping.value


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

    # Act: Executed in fixture executed_calculation_job
    sut.write(
        result_df,
        time_series_type,
        Grouping.total_ga,
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

    # Act: Executed in fixture executed_calculation_job
    sut.write(
        result_df,
        TimeSeriesType.NON_PROFILED_CONSUMPTION,
        Grouping.total_ga,
    )

    # Assert
    actual_df = spark.read.table(table_name)
    assert actual_df.collect()[0]["batch_id"] == DEFAULT_BATCH_ID
