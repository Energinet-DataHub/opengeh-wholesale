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

from package.codelists import (
    MeteringPointResolution,
    TimeSeriesQuality,
    MarketRole,
    TimeSeriesType,
    Grouping,
)
from package.constants import Colname
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


def test__write_per_ga_per_actor__result_file_path_matches_contract(
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
    sut.write_per_ga_per_actor(
        result_df,
        TimeSeriesType.NON_PROFILED_CONSUMPTION,
        MarketRole.ENERGY_SUPPLIER,
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


def test__write_per_ga__result_file_path_matches_contract(
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
    sut.write_per_ga(
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


def test__write_ga_brp_es__result_file_path_matches_contract(
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
    sut.write_per_ga_per_brp_per_es(
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
