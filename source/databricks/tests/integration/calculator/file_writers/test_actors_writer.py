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
from package.codelists.market_role import MarketRole
from package.codelists.time_series_type import TimeSeriesType
from package.file_writers.actors_writer import ActorsWriter
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
DEFAULT_ENERGY_SUPPLIER_GLN = "987654321"
DEFAULT_BALANCE_RESPONSIBLE_GLN = "23232323"


def _create_result_row(
    grid_area: str,
    energy_supplier_gln: str,
    balance_responsible_gln: str,
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
        Colname.energy_supplier_id: energy_supplier_gln,
        Colname.balance_responsible_id: balance_responsible_gln,
    }

    return row


def _get_glns_from_actors_file(
    output_path: str, batch_id: str, grid_area: str, time_series_type: TimeSeriesType
) -> tuple[list[str], list[str]]:
    actors_path = infra.get_actors_file_relative_path(
        batch_id, grid_area, time_series_type
    )
    actors_json = find_file(output_path, f"{actors_path}/part-*.json")

    es_glns = []
    brp_glns = []
    with open(actors_json, "r") as json_file:
        for line in json_file:
            json_data = json.loads(line)
            es_glns.append(json_data["energy_supplier_gln"])
            brp_glns.append(json_data["balance_responsible_party_gln"])

    return es_glns, brp_glns


def test__write__actors_file_has_expected_es_glns(
    spark: SparkSession, tmpdir: Path
) -> None:
    # Arrange
    output_path = str(tmpdir)
    expected_es_glns_805 = ["123", "234"]
    expected_brp_glns_805 = ["321", "321"]
    expected_es_glns_806 = ["123", "345"]
    expected_brp_glns_806 = ["321", "543"]
    time_series_type = TimeSeriesType.NON_PROFILED_CONSUMPTION

    rows = []
    rows.append(
        _create_result_row(
            grid_area="805",
            energy_supplier_gln=expected_es_glns_805[0],
            balance_responsible_gln=expected_brp_glns_805[0],
        )
    )
    rows.append(
        _create_result_row(
            grid_area="805",
            energy_supplier_gln=expected_es_glns_805[1],
            balance_responsible_gln=expected_brp_glns_805[1],
        )
    )
    rows.append(
        _create_result_row(
            grid_area="806",
            energy_supplier_gln=expected_es_glns_806[0],
            balance_responsible_gln=expected_brp_glns_806[0],
        )
    )
    rows.append(
        _create_result_row(
            grid_area="806",
            energy_supplier_gln=expected_es_glns_806[1],
            balance_responsible_gln=expected_brp_glns_806[1],
        )
    )
    result_df = spark.createDataFrame(rows)

    sut = ActorsWriter(output_path, DEFAULT_BATCH_ID)

    # Act
    sut.write(result_df, time_series_type)

    # Assert
    actual_es_gln_805, actual_brp_gln_805 = _get_glns_from_actors_file(
        output_path, DEFAULT_BATCH_ID, "805", time_series_type
    )
    actual_es_gln_806, actual_brp_gln_806 = _get_glns_from_actors_file(
        output_path, DEFAULT_BATCH_ID, "806", time_series_type
    )

    assert len(actual_es_gln_805) == len(expected_es_glns_805) and len(
        actual_es_gln_806
    ) == len(expected_es_glns_806)
    assert set(actual_es_gln_805) == set(expected_es_glns_805) and set(
        actual_es_gln_806
    ) == set(expected_es_glns_806)

    # assert brp
    assert len(actual_brp_gln_805) == len(expected_brp_glns_805) and len(
        actual_brp_gln_806
    ) == len(expected_brp_glns_806)
    assert set(actual_brp_gln_805) == set(expected_brp_glns_805) and set(
        actual_brp_gln_806
    ) == set(expected_brp_glns_806)


def test__write_per_ga_per_actor__actors_file_path_matches_contract(
    spark: SparkSession,
    contracts_path: str,
    tmpdir,
) -> None:
    # Arrange
    row = [
        _create_result_row(
            grid_area=DEFAULT_GRID_AREA,
            energy_supplier_gln=DEFAULT_ENERGY_SUPPLIER_GLN,
            balance_responsible_gln=DEFAULT_BALANCE_RESPONSIBLE_GLN,
        )
    ]
    result_df = spark.createDataFrame(data=row)
    relative_output_path = infra.get_actors_file_relative_path(
        DEFAULT_BATCH_ID, DEFAULT_GRID_AREA, TimeSeriesType.NON_PROFILED_CONSUMPTION
    )
    sut = ActorsWriter(str(tmpdir), DEFAULT_BATCH_ID)

    # Act
    sut.write(result_df, TimeSeriesType.NON_PROFILED_CONSUMPTION)

    # Assert
    actual_result_file = find_file(
        f"{str(tmpdir)}/",
        f"{relative_output_path}/part-*.json",
    )
    assert_file_path_match_contract(
        contracts_path, actual_result_file, CalculationFileType.ActorsFile
    )
