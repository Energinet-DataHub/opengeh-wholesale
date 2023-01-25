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
from pathlib import Path
from package.process_step_result_writer import ProcessStepResultWriter
from package.constants.time_series_type import TimeSeriesType
from package.constants.market_role import MarketRole
from package.constants import Colname
from package.codelists import TimeSeriesQuality, MeteringPointResolution
from pyspark.sql.types import StructType, StringType, DecimalType, TimestampType
from pyspark.sql import SparkSession
from decimal import Decimal
from datetime import datetime
from tests.helpers.file_utils import find_first_file


ACTORS_FOLDER = "actors"


def _get_result_schema() -> StructType:
    return (
        StructType()
        .add(Colname.grid_area, StringType(), False)
        .add(
            Colname.time_window,
            StructType()
            .add(Colname.start, TimestampType())
            .add(Colname.end, TimestampType()),
            False,
        )
        .add(Colname.sum_quantity, DecimalType(20, 1))
        .add(Colname.quality, StringType())
        .add(Colname.resolution, StringType())
        .add(Colname.energy_supplier_id, StringType())
    )


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


def _get_actors_path(
    output_path: str,
    grid_area: str,
    time_series_type: TimeSeriesType,
    market_role: MarketRole,
) -> str:
    return f"{output_path}/{ACTORS_FOLDER}/grid_area={grid_area}/time_series_type={time_series_type.value}/market_role={market_role.value}"


def _get_gln_from_actors_file(
    output_path: str,
    grid_area: str,
    time_series_type: TimeSeriesType,
    market_role: MarketRole,
) -> list[str]:

    actors_path = _get_actors_path(
        output_path, grid_area, time_series_type, market_role
    )
    actors_json = find_first_file(actors_path, "part-*.json")

    gln = []
    with open(actors_json, "r") as json_file:
        for line in json_file:
            json_data = json.loads(line)
            gln.append(json_data[Colname.gln])

    return gln


def test__write_per_ga__does_not_write_actor_data(spark: SparkSession, tmpdir) -> None:

    # Arrange
    actors_directory = Path.joinpath(Path(tmpdir), ACTORS_FOLDER)
    row = [_create_result_row(grid_area="805", energy_supplier_id="123")]
    result_df = spark.createDataFrame(data=row, schema=_get_result_schema())
    sut = ProcessStepResultWriter(str(tmpdir))

    # Act
    sut.write_per_ga(result_df, TimeSeriesType.NON_PROFILED_CONSUMPTION)

    # Assert
    assert not actors_directory.exists()


def test__write_per_ga_per_actor__actors_folder_is_created(
    spark: SparkSession, tmpdir
) -> None:

    # Arrange
    actors_directory = Path.joinpath(Path(tmpdir), ACTORS_FOLDER)
    row = [_create_result_row(grid_area="805", energy_supplier_id="123")]
    result_df = spark.createDataFrame(data=row, schema=_get_result_schema())
    sut = ProcessStepResultWriter(str(tmpdir))

    # Act
    sut.write_per_ga_per_actor(
        result_df, TimeSeriesType.NON_PROFILED_CONSUMPTION, MarketRole.ENERGY_SUPPLIER
    )

    # Assert
    assert actors_directory.exists()


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
    result_df = spark.createDataFrame(rows, schema=_get_result_schema())

    sut = ProcessStepResultWriter(output_path)

    # Act
    sut.write_per_ga_per_actor(result_df, time_series_type, market_role)

    # Assert
    actual_gln_805 = _get_gln_from_actors_file(
        output_path, "805", time_series_type, market_role
    )
    actual_gln_806 = _get_gln_from_actors_file(
        output_path, "806", time_series_type, market_role
    )

    assert set(actual_gln_805) == set(expected_gln_805) and set(actual_gln_806) == set(
        expected_gln_806
    )
