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

from pathlib import Path
from typing import Callable

import package.infrastructure as infra
import pytest
from package.constants import Colname
from package.file_writers import BasisDataWriter
from pyspark.sql import DataFrame
from tests.helpers.assert_calculation_file_path import (
    CalculationFileType,
    assert_file_path_match_contract,
)
from tests.helpers.file_utils import find_file
from package.codelists import (
    MeteringPointResolution,
    MeteringPointType,
    TimeSeriesQuality,
)
from decimal import Decimal
from datetime import datetime

DEFAULT_BATCH_ID = "0b15a420-9fc8-409a-a169-fbd49479d718"
DEFAULT_GRID_AREA = "105"
PERIOD_START = datetime(2022, 2, 1, 22, 0, 0)
PERIOD_END = datetime(2022, 3, 1, 22, 0, 0)
TIME_ZONE = "Europe/Copenhagen"


@pytest.fixture(scope="module")
def enriched_time_series_factory(spark, timestamp_factory) -> Callable[..., DataFrame]:
    def factory(
        resolution=MeteringPointResolution.quarter.value,
        quantity=Decimal("1"),
        grid_area=DEFAULT_GRID_AREA,
        metering_point_id="the_metering_point_id",
        metering_point_type=MeteringPointType.production.value,
        time="2022-06-08T22:00:00.000Z",
    ) -> DataFrame:
        df = [
            {
                Colname.metering_point_id: metering_point_id,
                Colname.metering_point_type: metering_point_type,
                Colname.grid_area: grid_area,
                Colname.balance_responsible_id: "someId",
                Colname.energy_supplier_id: "someId",
                Colname.quantity: quantity,
                Colname.observation_time: time,
                Colname.quality: TimeSeriesQuality.estimated.value,
                Colname.resolution: resolution,
            }
        ]
        return spark.createDataFrame(df)

    return factory


@pytest.fixture(scope="module")
def metering_point_period_df_factory(
    spark, timestamp_factory
) -> Callable[..., DataFrame]:
    def factory(
        resolution=MeteringPointResolution.quarter.value,
    ) -> DataFrame:
        df = [
            {
                Colname.metering_point_id: "the-meteringpoint-id",
                Colname.grid_area: DEFAULT_GRID_AREA,
                Colname.from_date: timestamp_factory("2022-01-01T22:00:00.000Z"),
                Colname.to_date: timestamp_factory("2022-12-22T22:00:00.000Z"),
                Colname.metering_point_type: "the_metering_point_type",
                Colname.settlement_method: "D01",
                Colname.out_grid_area: "",
                Colname.in_grid_area: "",
                Colname.resolution: resolution,
                Colname.energy_supplier_id: "someId",
                Colname.balance_responsible_id: "someId",
            }
        ]
        return spark.createDataFrame(df)

    return factory


def test__master_basis_data_for_total_ga_filepath_matches_contract(
    contracts_path: str,
    tmpdir: Path,
    metering_point_period_df_factory: Callable[..., DataFrame],
    enriched_time_series_factory: Callable[..., DataFrame],
) -> None:
    # Arrange
    basis_data_path = infra.get_master_basis_data_for_total_ga_relative_path(
        DEFAULT_BATCH_ID, DEFAULT_GRID_AREA
    )
    metering_point_period_df = metering_point_period_df_factory()
    enriched_time_series = enriched_time_series_factory()
    sut = BasisDataWriter(str(tmpdir), DEFAULT_BATCH_ID)

    # Act
    sut.write(
        metering_point_period_df,
        enriched_time_series,
        PERIOD_START,
        PERIOD_END,
        TIME_ZONE,
    )

    # Assert
    actual_file_path = find_file(
        f"{str(tmpdir)}/",
        f"{basis_data_path}/part-*.csv",
    )
    assert_file_path_match_contract(
        contracts_path, actual_file_path, CalculationFileType.MasterBasisDataForTotalGa
    )


def test__hourly_basis_data_for_total_ga_filepath_matches_contract(
    contracts_path: str,
    tmpdir: Path,
    metering_point_period_df_factory: Callable[..., DataFrame],
    enriched_time_series_factory: Callable[..., DataFrame],
) -> None:
    # Arrange
    basis_data_path = infra.get_time_series_hour_for_total_ga_relative_path(
        DEFAULT_BATCH_ID, DEFAULT_GRID_AREA
    )
    metering_point_period_df = metering_point_period_df_factory(
        resolution=MeteringPointResolution.hour.value
    )
    enriched_time_series = enriched_time_series_factory(
        resolution=MeteringPointResolution.hour.value
    )
    sut = BasisDataWriter(str(tmpdir), DEFAULT_BATCH_ID)

    # Act
    sut.write(
        metering_point_period_df,
        enriched_time_series,
        PERIOD_START,
        PERIOD_END,
        TIME_ZONE,
    )

    # Assert
    actual_file_path = find_file(
        f"{str(tmpdir)}/",
        f"{basis_data_path}/part-*.csv",
    )
    assert_file_path_match_contract(
        contracts_path, actual_file_path, CalculationFileType.TimeSeriesHourBasisData
    )


def test__quarterly_basis_data_for_total_ga_filepath_matches_contract(
    contracts_path: str,
    tmpdir: Path,
    metering_point_period_df_factory: Callable[..., DataFrame],
    enriched_time_series_factory: Callable[..., DataFrame],
) -> None:
    # Arrange
    basis_data_path = infra.get_time_series_quarter_for_total_ga_relative_path(
        DEFAULT_BATCH_ID, DEFAULT_GRID_AREA
    )
    metering_point_period_df = metering_point_period_df_factory(
        resolution=MeteringPointResolution.quarter.value
    )
    enriched_time_series = enriched_time_series_factory(
        resolution=MeteringPointResolution.quarter.value
    )
    sut = BasisDataWriter(str(tmpdir), DEFAULT_BATCH_ID)

    # Act
    sut.write(
        metering_point_period_df,
        enriched_time_series,
        PERIOD_START,
        PERIOD_END,
        TIME_ZONE,
    )

    # Assert
    actual_file_path = find_file(
        f"{str(tmpdir)}/",
        f"{basis_data_path}/part-*.csv",
    )
    assert_file_path_match_contract(
        contracts_path,
        actual_file_path,
        CalculationFileType.TimeSeriesQuarterBasisDataForTotalGa,
    )


# def test__quarterly_basis_data_for_total_ga_filepath_matches_contract(
#     data_lake_path: str,
#     worker_id: str,
#     contracts_path: str,
#     executed_calculation_job: None,
# ) -> None:
#     # Arrange
#     relative_output_path = infra.get_time_series_quarter_for_total_ga_relative_path(
#         executed_batch_id, "805"
#     )cd

#     # Act: Executed in fixture executed_calculation_job

#     # Assert
#     actual_file_path = find_file(
#         f"{data_lake_path}/{worker_id}", f"{relative_output_path}/part-*.csv"
#     )
#     assert_file_path_match_contract(
#         contracts_path,
#         actual_file_path,
#         CalculationFileType.TimeSeriesQuarterBasisDataForTotalGa,
#     )


# def test__master_basis_data_for_es_per_ga_filepath_matches_contract(
#     data_lake_path: str,
#     worker_id: str,
#     contracts_path: str,
#     executed_calculation_job: None,
# ) -> None:
#     # Arrange
#     master_basis_data_path = infra.get_master_basis_data_for_es_per_ga_relative_path(
#         executed_batch_id, "805", energy_supplier_gln_a
#     )

#     # Act: Executed in fixture executed_calculation_job

#     # Assert
#     actual_file_path = find_file(
#         f"{data_lake_path}/{worker_id}/",
#         f"{master_basis_data_path}/part-*.csv",
#     )
#     assert_file_path_match_contract(
#         contracts_path, actual_file_path, CalculationFileType.MasterBasisDataForEsPerGa
#     )


# def test__hourly_basis_data_for_es_per_ga_filepath_matches_contract(
#     data_lake_path: str,
#     worker_id: str,
#     contracts_path: str,
#     executed_calculation_job: None,
# ) -> None:
#     # Arrange
#     relative_output_path = infra.get_time_series_hour_for_es_per_ga_relative_path(
#         executed_batch_id, "805", energy_supplier_gln_a
#     )

#     # Act: Executed in fixture executed_calculation_job

#     # Assert
#     actual_file_path = find_file(
#         f"{data_lake_path}/{worker_id}", f"{relative_output_path}/part-*.csv"
#     )
#     assert_file_path_match_contract(
#         contracts_path,
#         actual_file_path,
#         CalculationFileType.TimeSeriesHourBasisDataForEsPerGa,
#     )


# def test__quarterly_basis_data_for_es_per_ga_filepath_matches_contract(
#     data_lake_path: str,
#     worker_id: str,
#     contracts_path: str,
#     executed_calculation_job: None,
# ) -> None:
#     # Arrange
#     relative_output_path = infra.get_time_series_quarter_for_es_per_ga_relative_path(
#         executed_batch_id, "805", energy_supplier_gln_a
#     )

#     # Act: Executed in fixture executed_calculation_job

#     # Assert
#     actual_file_path = find_file(
#         f"{data_lake_path}/{worker_id}", f"{relative_output_path}/part-*.csv"
#     )
#     assert_file_path_match_contract(
#         contracts_path,
#         actual_file_path,
#         CalculationFileType.TimeSeriesQuarterBasisDataForEsPerGa,
#     )
