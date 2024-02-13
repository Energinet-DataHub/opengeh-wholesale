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
from typing import Any, Callable

import pytest
from pyspark.sql import DataFrame, SparkSession

from package.calculation.CalculationResults import BasisDataContainer
from package.calculation.preparation.transformations import basis_data
from package.calculation_output.basis_data_writer import BasisDataWriter
from package.codelists import (
    BasisDataType,
    MeteringPointResolution,
    MeteringPointType,
    SettlementMethod,
    QuantityQuality,
)
from package.constants import Colname
from package.infrastructure import paths
from tests.helpers.assert_calculation_file_path import (
    CalculationFileType,
    assert_file_path_match_contract,
)
from tests.helpers.file_utils import find_file

DEFAULT_CALCULATION_ID = "0b15a420-9fc8-409a-a169-fbd49479d718"
DEFAULT_GRID_AREA = "105"
DEFAULT_ENERGY_SUPPLIER = "8100000000108"
PERIOD_START = datetime(2022, 2, 1, 22, 0, 0)
PERIOD_END = datetime(2022, 3, 1, 22, 0, 0)
TIME_ZONE = "Europe/Copenhagen"


@pytest.fixture(scope="module")
def metering_point_time_series_factory(spark: SparkSession) -> Callable[..., DataFrame]:
    def factory() -> DataFrame:
        df = []
        df.append(
            _create_metering_point_time_series_point(MeteringPointResolution.HOUR)
        )
        df.append(
            _create_metering_point_time_series_point(MeteringPointResolution.QUARTER)
        )
        return spark.createDataFrame(df)

    return factory


@pytest.fixture(scope="module")
def metering_point_period_df_factory(spark: SparkSession) -> Callable[..., DataFrame]:
    def factory() -> DataFrame:
        df = []
        df.append(_create_metering_point_period(MeteringPointResolution.HOUR))
        df.append(_create_metering_point_period(MeteringPointResolution.QUARTER))
        return spark.createDataFrame(df)

    return factory


def _create_metering_point_time_series_point(
    resolution: MeteringPointResolution,
) -> dict[str, Any]:
    data = {
        Colname.metering_point_id: "metering_point_id",
        Colname.metering_point_type: MeteringPointType.PRODUCTION.value,
        Colname.grid_area: DEFAULT_GRID_AREA,
        Colname.balance_responsible_id: "someId",
        Colname.energy_supplier_id: DEFAULT_ENERGY_SUPPLIER,
        Colname.quantity: Decimal("1"),
        Colname.observation_time: PERIOD_START,
        Colname.quality: QuantityQuality.ESTIMATED.value,
        Colname.resolution: resolution.value,
    }
    return data


def _create_metering_point_period(
    resolution: MeteringPointResolution,
) -> dict[str, Any]:
    data = {
        Colname.metering_point_id: "the-meteringpoint-id",
        Colname.grid_area: DEFAULT_GRID_AREA,
        Colname.from_date: PERIOD_START,
        Colname.to_date: PERIOD_END,
        Colname.metering_point_type: "the_metering_point_type",
        Colname.settlement_method: SettlementMethod.FLEX.value,
        Colname.from_grid_area: "",
        Colname.to_grid_area: "",
        Colname.resolution: resolution.value,
        Colname.energy_supplier_id: DEFAULT_ENERGY_SUPPLIER,
        Colname.balance_responsible_id: "someId",
    }
    return data


def _get_basis_data_paths(calculation_filetype: CalculationFileType) -> str:
    if calculation_filetype == CalculationFileType.MASTER_BASIS_DATA_FOR_TOTAL_GA:
        return paths.get_basis_data_path(
            BasisDataType.MASTER_BASIS_DATA, DEFAULT_CALCULATION_ID, DEFAULT_GRID_AREA
        )
    elif calculation_filetype == CalculationFileType.MASTER_BASIS_DATA_FOR_ES_PER_GA:
        return paths.get_basis_data_path(
            BasisDataType.MASTER_BASIS_DATA,
            DEFAULT_CALCULATION_ID,
            DEFAULT_GRID_AREA,
            DEFAULT_ENERGY_SUPPLIER,
        )
    elif (
        calculation_filetype
        == CalculationFileType.TIME_SERIES_QUARTER_BASIS_DATA_FOR_TOTAL_GA
    ):
        return paths.get_basis_data_path(
            BasisDataType.TIME_SERIES_QUARTER, DEFAULT_CALCULATION_ID, DEFAULT_GRID_AREA
        )
    elif (
        calculation_filetype
        == CalculationFileType.TIME_SERIES_QUARTER_BASIS_DATA_FOR_ES_PER_GA
    ):
        return paths.get_basis_data_path(
            BasisDataType.TIME_SERIES_QUARTER,
            DEFAULT_CALCULATION_ID,
            DEFAULT_GRID_AREA,
            DEFAULT_ENERGY_SUPPLIER,
        )
    elif calculation_filetype == CalculationFileType.TIME_SERIES_HOUR_BASIS_DATA:
        return paths.get_basis_data_path(
            BasisDataType.TIME_SERIES_HOUR, DEFAULT_CALCULATION_ID, DEFAULT_GRID_AREA
        )
    elif (
        calculation_filetype
        == CalculationFileType.TIME_SERIES_HOUR_BASIS_DATA_FOR_ES_PER_GA
    ):
        return paths.get_basis_data_path(
            BasisDataType.TIME_SERIES_HOUR,
            DEFAULT_CALCULATION_ID,
            DEFAULT_GRID_AREA,
            DEFAULT_ENERGY_SUPPLIER,
        )

    raise ValueError(f"Unexpected CalculationFileType, {calculation_filetype}")


def _get_all_basis_data_file_types() -> list[CalculationFileType]:
    return [
        CalculationFileType.MASTER_BASIS_DATA_FOR_ES_PER_GA,
        CalculationFileType.MASTER_BASIS_DATA_FOR_TOTAL_GA,
        CalculationFileType.TIME_SERIES_QUARTER_BASIS_DATA_FOR_TOTAL_GA,
        CalculationFileType.TIME_SERIES_QUARTER_BASIS_DATA_FOR_ES_PER_GA,
        CalculationFileType.TIME_SERIES_HOUR_BASIS_DATA,
        CalculationFileType.TIME_SERIES_HOUR_BASIS_DATA_FOR_ES_PER_GA,
    ]


def test__write__writes_to_paths_that_match_contract(
    contracts_path: str,
    tmpdir: Path,
    metering_point_period_df_factory: Callable[..., DataFrame],
    metering_point_time_series_factory
) -> None:
    """
    This test calls 'write' once and then asserts on all file contracts.
    This is done to avoid multiple write operations, and thereby reduce execution time
    """
    # Arrange
    metering_point_period_df = metering_point_period_df_factory()
    metering_point_time_series = metering_point_time_series_factory()
    (
        timeseries_quarter_df,
        timeseries_hour_df,
    ) = basis_data.get_metering_point_time_series_basis_data_dfs(
        metering_point_time_series,
        TIME_ZONE,
    )

    master_basis_data_df = basis_data.get_master_basis_data_df(metering_point_period_df)
    
    basis_data_container: BasisDataContainer = BasisDataContainer()
    basis_data_container.timeseries_quarter_df = timeseries_quarter_df
    basis_data_container.timeseries_hour_df = timeseries_hour_df
    basis_data_container.master_basis_data_df = master_basis_data_df
    sut = BasisDataWriter(str(tmpdir), DEFAULT_CALCULATION_ID)

    # Act
    sut.write(basis_data_container)

    # Assert
    for file_type in _get_all_basis_data_file_types():
        actual_file_path = find_file(
            f"{str(tmpdir)}/",
            f"{_get_basis_data_paths(file_type)}/part-*.csv",
        )
        assert_file_path_match_contract(
            contracts_path,
            actual_file_path,
            file_type,
        )
