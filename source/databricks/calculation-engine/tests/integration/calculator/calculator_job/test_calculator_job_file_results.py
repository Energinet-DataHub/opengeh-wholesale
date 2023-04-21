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
import pytest
from tests.contract_utils import assert_contract_matches_schema
from . import configuration as C
from package.codelists import (
    TimeSeriesType,
    AggregationLevel,
)
import package.infrastructure as infra


@pytest.mark.parametrize(
    "grid_area,energy_supplier_gln,balance_responsible_party_gln,time_series_type,aggregation_level",
    [
        ("805", None, None, TimeSeriesType.PRODUCTION, AggregationLevel.total_ga),
        ("806", None, None, TimeSeriesType.PRODUCTION, AggregationLevel.total_ga),
        (
            "805",
            C.energy_supplier_gln_a,
            None,
            TimeSeriesType.NON_PROFILED_CONSUMPTION,
            AggregationLevel.es_per_ga,
        ),
        (
            "806",
            C.energy_supplier_gln_a,
            None,
            TimeSeriesType.NON_PROFILED_CONSUMPTION,
            AggregationLevel.es_per_ga,
        ),
        (
            "805",
            C.energy_supplier_gln_b,
            None,
            TimeSeriesType.NON_PROFILED_CONSUMPTION,
            AggregationLevel.es_per_ga,
        ),
        (
            "806",
            C.energy_supplier_gln_b,
            None,
            TimeSeriesType.NON_PROFILED_CONSUMPTION,
            AggregationLevel.es_per_ga,
        ),
        (
            "805",
            C.energy_supplier_gln_a,
            C.balance_responsible_party_gln_a,
            TimeSeriesType.NON_PROFILED_CONSUMPTION,
            AggregationLevel.es_per_brp_per_ga,
        ),
        (
            "806",
            C.energy_supplier_gln_a,
            C.balance_responsible_party_gln_a,
            TimeSeriesType.NON_PROFILED_CONSUMPTION,
            AggregationLevel.es_per_brp_per_ga,
        ),
        (
            "805",
            None,
            None,
            TimeSeriesType.NON_PROFILED_CONSUMPTION,
            AggregationLevel.total_ga,
        ),
        (
            "806",
            None,
            None,
            TimeSeriesType.NON_PROFILED_CONSUMPTION,
            AggregationLevel.total_ga,
        ),
    ],
)
def test__result_is_generated_for_requested_grid_areas(
    spark: SparkSession,
    data_lake_path: str,
    executed_calculation_job: None,
    grid_area: str,
    energy_supplier_gln: str,
    balance_responsible_party_gln: str,
    time_series_type: TimeSeriesType,
    aggregation_level: AggregationLevel,
) -> None:
    # Act: Calculator job is executed just once per session. See the fixture `executed_calculation_job`

    # Assert
    result_path = infra.get_result_file_relative_path(
        C.executed_batch_id,
        grid_area,
        energy_supplier_gln,
        balance_responsible_party_gln,
        time_series_type,
        aggregation_level,
    )
    print(result_path)
    result = spark.read.json(f"{data_lake_path}/{result_path}")
    assert result.count() >= 1, "Calculator job failed to write files"


def test__production_total_ga__schema_must_match_contract_with_dotnet(
    spark: SparkSession,
    data_lake_path: str,
    contracts_path: str,
    executed_calculation_job: None,
) -> None:
    # Arrange
    result_relative_path = infra.get_result_file_relative_path(
        C.executed_batch_id,
        "805",
        None,
        None,
        TimeSeriesType.PRODUCTION,
        AggregationLevel.total_ga,
    )
    result_path = f"{data_lake_path}/{result_relative_path}"

    # Act: Calculator job is executed just once per session. See the fixture `executed_calculation_job`

    # Assert
    result_805 = spark.read.json(result_path)

    assert_contract_matches_schema(
        f"{contracts_path}/calculator-result.json",
        result_805.schema,
    )


def test__non_profiled_consumption_per_es__schema_must_match_contract_with_dotnet(
    spark: SparkSession,
    data_lake_path: str,
    contracts_path: str,
    executed_calculation_job: None,
) -> None:
    # Arrange
    result_relative_path = infra.get_result_file_relative_path(
        C.executed_batch_id,
        "805",
        C.energy_supplier_gln_a,
        None,
        TimeSeriesType.NON_PROFILED_CONSUMPTION,
        AggregationLevel.es_per_ga,
    )
    result_path = f"{data_lake_path}/{result_relative_path}"

    # Act: Calculator job is executed just once per session. See the fixture `executed_calculation_job`

    # Assert
    result_805 = spark.read.json(result_path)

    assert_contract_matches_schema(
        f"{contracts_path}/calculator-result.json",
        result_805.schema,
    )


def test__non_profiled_consumption_per_es__has_expected_number_of_rows(
    spark: SparkSession,
    data_lake_path: str,
    executed_calculation_job: None,
) -> None:
    # Arrange
    result_relative_path = infra.get_result_file_relative_path(
        C.executed_batch_id,
        "806",
        C.energy_supplier_gln_a,
        None,
        TimeSeriesType.NON_PROFILED_CONSUMPTION,
        AggregationLevel.es_per_ga,
    )

    # Act: Calculator job is executed just once per session. See the fixture `executed_calculation_job`

    # Assert
    consumption_806 = spark.read.json(f"{data_lake_path}/{result_relative_path}")
    assert consumption_806.count() == 192  # period is from 01-01 -> 01-03


def test__production_total_ga__has_expected_number_of_rows(
    spark: SparkSession,
    data_lake_path: str,
    executed_calculation_job: None,
) -> None:
    # Arrange
    result_relative_path = infra.get_result_file_relative_path(
        C.executed_batch_id,
        "806",
        None,
        None,
        TimeSeriesType.PRODUCTION,
        AggregationLevel.total_ga,
    )

    # Act: Calculator job is executed just once per session. See the fixture `executed_calculation_job`

    # Assert
    production_806 = spark.read.json(f"{data_lake_path}/{result_relative_path}")
    assert production_806.count() == 192  # period is from 01-01 -> 01-03
