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

import json
from datetime import datetime
from decimal import Decimal
from pathlib import Path
from unittest.mock import patch

from package.codelists import MeteringPointResolution, TimeSeriesQuality
from package.constants import Colname
from package.constants.market_role import MarketRole
from package.constants.time_series_type import TimeSeriesType
from package.file_writers.process_step_result_writer import ProcessStepResultWriter
import package.infrastructure as infra
from pyspark.sql import SparkSession
from tests.helpers.assert_calculation_file_path import (
    CalculationFileType,
    assert_file_path_match_contract,
)
from tests.helpers.file_utils import find_file

ACTORS_FOLDER = "actors"
DEFAULT_BATCH_ID = "0b15a420-9fc8-409a-a169-fbd49479d718"
DEFAULT_GRID_AREA = "105"
DEFAULT_ENERGY_SUPPLIER_ID = "987654321"


def _create_result_row(
    grid_area: str,
    energy_supplier_id: str,
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
    }

    return row


def _get_gln_from_actors_file(
    output_path: str,
    batch_id: str,
    grid_area: str,
    time_series_type: TimeSeriesType,
    market_role: MarketRole,
) -> list[str]:

    actors_path = infra.get_actors_file_relative_path(
        batch_id, grid_area, time_series_type, market_role
    )
    actors_json = find_file(output_path, f"{actors_path}/part-*.json")

    gln = []
    with open(actors_json, "r") as json_file:
        for line in json_file:
            json_data = json.loads(line)
            gln.append(json_data[Colname.gln])

    return gln


@patch("package.file_writers.process_step_result_writer.actors_writer")
def test__write_per_ga__does_not_call_actors_writer(
    mock_actors_writer, spark: SparkSession, tmpdir
) -> None:

    # Arrange
    row = [
        _create_result_row(
            grid_area=DEFAULT_GRID_AREA, energy_supplier_id=DEFAULT_ENERGY_SUPPLIER_ID
        )
    ]
    result_df = spark.createDataFrame(data=row)
    sut = ProcessStepResultWriter(str(tmpdir), DEFAULT_BATCH_ID)

    # Act
    sut.write_per_ga(result_df, TimeSeriesType.NON_PROFILED_CONSUMPTION)

    # Assert
    mock_actors_writer.write.assert_not_called()


def test__write_per_ga_per_actor__actors_file_has_expected_gln(
    spark: SparkSession, tmpdir: Path
) -> None:

    # Arrange
    output_path = str(tmpdir)
    expected_gln_805 = ["123", "234"]
    expected_gln_806 = ["123", "345"]
    time_series_type = TimeSeriesType.NON_PROFILED_CONSUMPTION
    market_role = MarketRole.ENERGY_SUPPLIER

    rows = []
    rows.append(
        _create_result_row(grid_area="805", energy_supplier_id=expected_gln_805[0])
    )
    rows.append(
        _create_result_row(grid_area="805", energy_supplier_id=expected_gln_805[1])
    )
    rows.append(
        _create_result_row(grid_area="806", energy_supplier_id=expected_gln_806[0])
    )
    rows.append(
        _create_result_row(grid_area="806", energy_supplier_id=expected_gln_806[1])
    )
    result_df = spark.createDataFrame(rows)

    sut = ProcessStepResultWriter(output_path, DEFAULT_BATCH_ID)

    # Act
    sut.write_per_ga_per_actor(result_df, time_series_type, market_role)

    # Assert
    actual_gln_805 = _get_gln_from_actors_file(
        output_path, DEFAULT_BATCH_ID, "805", time_series_type, market_role
    )
    actual_gln_806 = _get_gln_from_actors_file(
        output_path, DEFAULT_BATCH_ID, "806", time_series_type, market_role
    )

    assert len(actual_gln_805) == len(expected_gln_805) and len(actual_gln_806) == len(
        expected_gln_806
    )
    assert set(actual_gln_805) == set(expected_gln_805) and set(actual_gln_806) == set(
        expected_gln_806
    )


def test__write_per_ga_per_actor__actors_file_path_matches_contract(
    spark: SparkSession,
    contracts_path: str,
    tmpdir,
) -> None:
    # Arrange
    row = [
        _create_result_row(
            grid_area=DEFAULT_GRID_AREA, energy_supplier_id=DEFAULT_ENERGY_SUPPLIER_ID
        )
    ]
    result_df = spark.createDataFrame(data=row)
    relative_output_path = infra.get_actors_file_relative_path(
        DEFAULT_BATCH_ID,
        DEFAULT_GRID_AREA,
        TimeSeriesType.NON_PROFILED_CONSUMPTION,
        MarketRole.ENERGY_SUPPLIER,
    )
    sut = ProcessStepResultWriter(str(tmpdir), DEFAULT_BATCH_ID)

    # Act
    sut.write_per_ga_per_actor(
        result_df, TimeSeriesType.NON_PROFILED_CONSUMPTION, MarketRole.ENERGY_SUPPLIER
    )

    # Assert
    actual_result_file = find_file(
        f"{str(tmpdir)}/",
        f"{relative_output_path}/part-*.json",
    )
    assert_file_path_match_contract(
        contracts_path, actual_result_file, CalculationFileType.ActorsFile
    )


def test__write_per_ga_per_actor__result_file_path_matches_contract(
    spark: SparkSession,
    contracts_path: str,
    tmpdir,
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
        TimeSeriesType.NON_PROFILED_CONSUMPTION,
    )
    sut = ProcessStepResultWriter(str(tmpdir), DEFAULT_BATCH_ID)

    # Act: Executed in fixture executed_calculation_job
    sut.write_per_ga_per_actor(
        result_df, TimeSeriesType.NON_PROFILED_CONSUMPTION, MarketRole.ENERGY_SUPPLIER
    )

    # Assert
    actual_result_file = find_file(
        f"{str(tmpdir)}/",
        f"{relative_output_path}/part-*.json",
    )
    assert_file_path_match_contract(
        contracts_path, actual_result_file, CalculationFileType.ResultFile
    )
