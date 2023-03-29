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

from pyspark.sql import SparkSession
from . import configuration as C
from package.codelists import (
    BasisDataType,
)
import package.infrastructure as infra


def test__creates_hour_for_total_ga__with_expected_columns_names(
    spark: SparkSession,
    data_lake_path: str,
    worker_id: str,
    executed_calculation_job: None,
) -> None:
    # Arrange
    basis_data_relative_path = infra.get_basis_data_path(
        BasisDataType.TimeSeriesHour, C.executed_batch_id, "805"
    )

    # Act
    # we run the calculator once per session. See the fixture executed_calculation_job in top of this file

    # Assert
    actual = spark.read.option("header", "true").csv(
        f"{data_lake_path}/{worker_id}/{basis_data_relative_path}"
    )
    assert actual.columns == [
        "METERINGPOINTID",
        "TYPEOFMP",
        "STARTDATETIME",
        *[f"ENERGYQUANTITY{i+1}" for i in range(24)],
    ]


def test__creates_hour_for_es_per_ga__with_expected_columns_names(
    spark: SparkSession,
    data_lake_path: str,
    worker_id: str,
    executed_calculation_job: None,
) -> None:
    # Arrange
    basis_data_relative_path = infra.get_basis_data_path(
        BasisDataType.TimeSeriesHour,
        C.executed_batch_id,
        "805",
        C.energy_supplier_gln_a,
    )

    # Act
    # we run the calculator once per session. See the fixture executed_calculation_job in top of this file

    # Assert
    actual = spark.read.option("header", "true").csv(
        f"{data_lake_path}/{worker_id}/{basis_data_relative_path}"
    )
    assert actual.columns == [
        "METERINGPOINTID",
        "TYPEOFMP",
        "STARTDATETIME",
        *[f"ENERGYQUANTITY{i+1}" for i in range(24)],
    ]


def test__creates_quarter_for_total_ga__with_expected_columns_names(
    spark: SparkSession,
    data_lake_path: str,
    worker_id: str,
    executed_calculation_job: None,
) -> None:
    # Arrange
    relative_path = infra.get_basis_data_path(
        BasisDataType.TimeSeriesQuarter, C.executed_batch_id, "805"
    )

    # Act
    # we run the calculator once per session. See the fixture executed_calculation_job in top of this file

    # Assert
    actual = spark.read.option("header", "true").csv(
        f"{data_lake_path}/{worker_id}/{relative_path}"
    )

    assert actual.columns == [
        "METERINGPOINTID",
        "TYPEOFMP",
        "STARTDATETIME",
        *[f"ENERGYQUANTITY{i+1}" for i in range(96)],
    ]


def test__creates_quarter_for_es_per_ga__with_expected_columns_names(
    spark: SparkSession,
    data_lake_path: str,
    worker_id: str,
    executed_calculation_job: None,
) -> None:
    # Arrange
    relative_path = infra.get_basis_data_path(
        BasisDataType.TimeSeriesQuarter,
        C.executed_batch_id,
        "805",
        C.energy_supplier_gln_a,
    )

    # Act
    # we run the calculator once per session. See the fixture executed_calculation_job in top of this file

    # Assert
    actual = spark.read.option("header", "true").csv(
        f"{data_lake_path}/{worker_id}/{relative_path}"
    )

    assert actual.columns == [
        "METERINGPOINTID",
        "TYPEOFMP",
        "STARTDATETIME",
        *[f"ENERGYQUANTITY{i+1}" for i in range(96)],
    ]


def test__creates_quarter_for_total_ga__per_grid_area(
    spark: SparkSession,
    data_lake_path: str,
    worker_id: str,
    executed_calculation_job: None,
) -> None:
    # Arrange
    basis_data_relative_path_805 = infra.get_basis_data_path(
        BasisDataType.TimeSeriesQuarter, C.executed_batch_id, "805"
    )
    basis_data_relative_path_806 = infra.get_basis_data_path(
        BasisDataType.TimeSeriesQuarter, C.executed_batch_id, "806"
    )

    # Act
    # we run the calculator once per session. See the fixture executed_calculation_job in top of this file

    # Assert
    basis_data_805 = spark.read.option("header", "true").csv(
        f"{data_lake_path}/{worker_id}/{basis_data_relative_path_805}"
    )

    basis_data_806 = spark.read.option("header", "true").csv(
        f"{data_lake_path}/{worker_id}/{basis_data_relative_path_806}"
    )

    assert (
        basis_data_805.count() >= 1
    ), "Calculator job failed to write basis data files for grid area 805"

    assert (
        basis_data_806.count() >= 1
    ), "Calculator job failed to write basis data files for grid area 806"


def test__creates_quarter_for_es_per_ga__per_energy_supplier(
    spark: SparkSession,
    data_lake_path: str,
    worker_id: str,
    executed_calculation_job: None,
) -> None:
    # Arrange
    basis_data_relative_path_a = infra.get_basis_data_path(
        BasisDataType.TimeSeriesQuarter,
        C.executed_batch_id,
        "805",
        C.energy_supplier_gln_a,
    )
    basis_data_relative_path_b = infra.get_basis_data_path(
        BasisDataType.TimeSeriesQuarter,
        C.executed_batch_id,
        "805",
        C.energy_supplier_gln_b,
    )

    # Act
    # we run the calculator once per session. See the fixture executed_calculation_job in top of this file

    # Assert
    basis_data_a = spark.read.option("header", "true").csv(
        f"{data_lake_path}/{worker_id}/{basis_data_relative_path_a}"
    )

    basis_data_b = spark.read.option("header", "true").csv(
        f"{data_lake_path}/{worker_id}/{basis_data_relative_path_b}"
    )

    assert (
        basis_data_a.count() >= 1
    ), "Calculator job failed to write basis data files for energy supplier a correctly"

    assert (
        basis_data_b.count() >= 1
    ), "Calculator job failed to write basis data files for energy supplier b correctly"


def test__creates_hour_for_total_ga__per_grid_area(
    spark: SparkSession,
    data_lake_path: str,
    worker_id: str,
    executed_calculation_job: None,
) -> None:
    # Arrange
    basis_data_relative_path_805 = infra.get_basis_data_path(
        BasisDataType.TimeSeriesHour, C.executed_batch_id, "805"
    )
    basis_data_relative_path_806 = infra.get_basis_data_path(
        BasisDataType.TimeSeriesHour, C.executed_batch_id, "806"
    )

    # Act
    # we run the calculator once per session. See the fixture executed_calculation_job in top of this file

    # Assert
    basis_data_805 = spark.read.option("header", "true").csv(
        f"{data_lake_path}/{worker_id}/{basis_data_relative_path_805}"
    )

    basis_data_806 = spark.read.option("header", "true").csv(
        f"{data_lake_path}/{worker_id}/{basis_data_relative_path_806}"
    )

    assert (
        basis_data_805.count() >= 1
    ), "Calculator job failed to write basis data files for grid area 805"

    assert (
        basis_data_806.count() >= 1
    ), "Calculator job failed to write basis data files for grid area 806"


def test__creates_hour_for_es_per_ga__per_energy_supplier(
    spark: SparkSession,
    data_lake_path: str,
    worker_id: str,
    executed_calculation_job: None,
) -> None:
    # Arrange
    basis_data_relative_path_a = infra.get_basis_data_path(
        BasisDataType.TimeSeriesHour,
        C.executed_batch_id,
        "805",
        C.energy_supplier_gln_a,
    )
    basis_data_relative_path_b = infra.get_basis_data_path(
        BasisDataType.TimeSeriesHour,
        C.executed_batch_id,
        "805",
        C.energy_supplier_gln_b,
    )

    # Act
    # we run the calculator once per session. See the fixture executed_calculation_job in top of this file

    # Assert
    basis_data_a = spark.read.option("header", "true").csv(
        f"{data_lake_path}/{worker_id}/{basis_data_relative_path_a}"
    )

    basis_data_b = spark.read.option("header", "true").csv(
        f"{data_lake_path}/{worker_id}/{basis_data_relative_path_b}"
    )

    assert (
        basis_data_a.count() >= 1
    ), "Calculator job failed to write basis data files for grid area 805"

    assert (
        basis_data_b.count() >= 1
    ), "Calculator job failed to write basis data files for grid area 806"


def test__master_basis_data_for_total_ga_has_expected_columns_names(
    spark: SparkSession,
    data_lake_path: str,
    worker_id: str,
    executed_calculation_job: None,
) -> None:
    # Arrange
    basis_data_path = infra.get_basis_data_path(
        BasisDataType.MasterBasisData, C.executed_batch_id, "805"
    )

    # Act
    # we run the calculator once per session. See the fixture executed_calculation_job in top of this file

    # Assert
    actual = spark.read.option("header", "true").csv(
        f"{data_lake_path}/{worker_id}/{basis_data_path}"
    )

    assert actual.columns == [
        "METERINGPOINTID",
        "VALIDFROM",
        "VALIDTO",
        "GRIDAREA",
        "TOGRIDAREA",
        "FROMGRIDAREA",
        "TYPEOFMP",
        "SETTLEMENTMETHOD",
        "ENERGYSUPPLIERID",
    ]


def test__master_basis_data_for_es_per_ga_has_expected_columns_names(
    spark: SparkSession,
    data_lake_path: str,
    worker_id: str,
    executed_calculation_job: None,
) -> None:
    # Arrange
    basis_data_path = infra.get_basis_data_path(
        BasisDataType.MasterBasisData,
        C.executed_batch_id,
        "805",
        C.energy_supplier_gln_a,
    )

    # Act
    # we run the calculator once per session. See the fixture executed_calculation_job in top of this file

    # Assert
    actual = spark.read.option("header", "true").csv(
        f"{data_lake_path}/{worker_id}/{basis_data_path}"
    )

    assert actual.columns == [
        "METERINGPOINTID",
        "VALIDFROM",
        "VALIDTO",
        "GRIDAREA",
        "TOGRIDAREA",
        "FROMGRIDAREA",
        "TYPEOFMP",
        "SETTLEMENTMETHOD",
    ]


def test__creates_master_basis_data_per_grid_area(
    spark: SparkSession,
    data_lake_path: str,
    worker_id: str,
    executed_calculation_job: None,
) -> None:
    # Arrange
    basis_data_path_805 = infra.get_basis_data_path(
        BasisDataType.MasterBasisData, C.executed_batch_id, "805"
    )
    basis_data_path_806 = infra.get_basis_data_path(
        BasisDataType.MasterBasisData, C.executed_batch_id, "806"
    )

    # Act: Executed in fixture executed_calculation_job

    # Assert
    master_basis_data_805 = spark.read.option("header", "true").csv(
        f"{data_lake_path}/{worker_id}/{basis_data_path_805}"
    )

    master_basis_data_806 = spark.read.option("header", "true").csv(
        f"{data_lake_path}/{worker_id}/{basis_data_path_806}"
    )

    assert (
        master_basis_data_805.count() >= 1
    ), "Calculator job failed to write master basis data files for grid area 805"

    assert (
        master_basis_data_806.count() >= 1
    ), "Calculator job failed to write master basis data files for grid area 806"
