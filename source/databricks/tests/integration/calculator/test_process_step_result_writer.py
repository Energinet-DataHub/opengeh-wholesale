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

import pytest
from os import path
from package.process_step_result_writer import ProcessStepResultWriter
from package.constants.time_series_type import TimeSeriesType
from package.constants.market_role import MarketRole
from package.constants import Colname
from package.codelists import TimeSeriesQuality, MeteringPointResolution
from pyspark.sql.types import StructType, StringType, DecimalType, TimestampType
from pyspark.sql import DataFrame, SparkSession
from decimal import Decimal
from datetime import datetime

# executed_batch_id = "0b15a420-9fc8-409a-a169-fbd49479d718"
# grid_area_gln = "grid_area"
# energy_supplier_gln_a = "8100000000108"
# energy_supplier_gln_b = "8100000000109"


@pytest.fixture(scope="module")
def result_schema() -> StructType:
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
        # .add(Colname.metering_point_type, StringType())
    )


def create_result_row(
    grid_area: str,
    energy_supplier_id: str,
    quantity: str = "1.1",
    quality: TimeSeriesQuality = TimeSeriesQuality.measured,
) -> list:
    row = [
        {
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
    ]

    return row


def get_actors_path(
    output_path: str, grid_area: str, time_series_type: str, market_role: MarketRole
) -> str:
    return f"{output_path}/actors/grid_area={grid_area}/time_series_type={time_series_type.value}/market_role={market_role.value}"


def test__write_per_ga_per_actor__has_expected_gln(
    spark: SparkSession, tmpdir, result_schema
) -> None:

    # Arrange
    grid_area_805 = "805"
    es_id_1 = "123"
    es_id_2 = "234"
    time_series_type = TimeSeriesType.NON_PROFILED_CONSUMPTION
    market_role = MarketRole.ENERGY_SUPPLIER
    rows = []
    rows.append(create_result_row(grid_area=grid_area_805, energy_supplier_id=es_id_1))
    rows.append(create_result_row(grid_area=grid_area_805, energy_supplier_id=es_id_2))
    row1 = create_result_row(grid_area=grid_area_805, energy_supplier_id=es_id_1)
    result_df = spark.createDataFrame(row1, schema=result_schema)

    sut = ProcessStepResultWriter(tmpdir)

    # file_name = tmpdir.join("test_file.txt")

    # Act
    sut.write_per_ga_per_actor(result_df, time_series_type, market_role)

    # Assert
    folder = get_actors_path(tmpdir, grid_area_805, time_series_type, market_role)
    print(folder)

    assert path.exists(folder)


# def get_result_path(
#     data_lake_path: str, grid_area: str, gln: str, time_series_type: str
# ) -> str:
#     return f"{data_lake_path}/calculation-output/batch_id={executed_batch_id}/result/grid_area={grid_area}/gln={gln}/time_series_type={time_series_type}"


# def test__result_is_generated_for_requested_grid_areas(
#     spark: SparkSession,
#     data_lake_path,
#     worker_id,
#     executed_calculation_job,
# ):
#     # Arrange
#     data_lake_path = f"{data_lake_path}/{worker_id}"

#     expected_ga_gln_type = [
#         ["805", grid_area_gln, TimeSeriesType.PRODUCTION.value],
#         ["806", grid_area_gln, TimeSeriesType.PRODUCTION.value],
#         ["805", energy_supplier_gln_a, TimeSeriesType.NON_PROFILED_CONSUMPTION.value],
#         ["806", energy_supplier_gln_a, TimeSeriesType.NON_PROFILED_CONSUMPTION.value],
#         ["805", energy_supplier_gln_b, TimeSeriesType.NON_PROFILED_CONSUMPTION.value],
#         ["806", energy_supplier_gln_b, TimeSeriesType.NON_PROFILED_CONSUMPTION.value],
#     ]

#     # Act
#     # we run the calculator once per session. See the fixture executed_calculation_job in top of this file

#     # Assert
#     for grid_area, gln, time_series_type in expected_ga_gln_type:
#         result = spark.read.json(
#             get_result_path(
#                 data_lake_path,
#                 grid_area,
#                 gln,
#                 time_series_type,
#             )
#         )
#         assert result.count() >= 1, "Calculator job failed to write files"


# def test__when_result_is_per_energy_supplier__actor_list_is_generated(
#     spark: SparkSession,
#     data_lake_path: str,
#     worker_id: str,
#     executed_calculation_job: None,
# ) -> None:

#     # Arrange
#     time_series_type = TimeSeriesType.NON_PROFILED_CONSUMPTION
#     output_path = (
#         f"{data_lake_path}/{worker_id}/calculation-output/batch_id={executed_batch_id}"
#     )
#     actors_path = (
#         f"{output_path}/actors/grid_area=805/time_series_type={time_series_type.value}"
#     )

#     # Act
#     # we run the calculator once per session. See the fixture executed_calculation_job in top of this file

#     # Assert
#     actors = spark.read.json(actors_path)
#     assert path.exists(actors_path)
#     assert actors.count() >= 1


# def test__when_result_is_only_per_grid_area__no_actor_list_is_generated(
#     spark: SparkSession,
#     data_lake_path: str,
#     worker_id: str,
#     executed_calculation_job: None,
# ) -> None:
#     print(type(worker_id))
#     print(type(executed_calculation_job))
#     # Arrange
#     time_series_type = TimeSeriesType.PRODUCTION
#     output_path = (
#         f"{data_lake_path}/{worker_id}/calculation-output/batch_id={executed_batch_id}"
#     )
#     actors_path = (
#         f"{output_path}/actors/grid_area=805/time_series_type={time_series_type.value}"
#     )

#     # Act
#     # we run the calculator once per session. See the fixture executed_calculation_job in top of this file

#     # Assert
#     assert not path.exists(actors_path)


# def test__calculator_result_schema_must_match_contract_with_dotnet(
#     spark,
# ):
#     # Arrange
#     data_lake_path = f"{data_lake_path}/{worker_id}"
#     result_path = get_result_path(
#         data_lake_path, "805", grid_area_gln, TimeSeriesType.PRODUCTION.value
#     )

#     # Act
#     # we run the calculator once per session. See the fixture executed_calculation_job in top of this file

#     # Assert
#     result_805 = spark.read.json(result_path)

#     assert_contract_matches_schema(
#         f"{source_path}/contracts/internal/calculator-result.json",
#         result_805.schema,
#     )


# @pytest.fixture(scope="session")
# def calculation_file_paths_contract(source_path):
#     with open(f"{source_path}/contracts/calculation-file-paths.yml", "r") as stream:
#         return DictObj(yaml.safe_load(stream))


# def create_file_path_expression(directory_expression, extension):
#     """Create file path regular expression from a directory expression
#     and a file extension.
#     The remaining base file name can be one or more characters except for forward slash ("/").
#     """
#     return f"{directory_expression}[^/]+{extension}"


# def test__actors_file_path_matches_contract(
#     data_lake_path,
#     find_first_file,
#     worker_id,
#     executed_calculation_job,
#     calculation_file_paths_contract,
# ):
#     # Arrange
#     contract = calculation_file_paths_contract.actors_file
#     expected_path_expression = create_file_path_expression(
#         contract.directory_expression,
#         contract.extension,
#     )
#     # Act: Executed in fixture executed_calculation_job

#     # Assert
#     actual_result_file = find_first_file(
#         f"{data_lake_path}/{worker_id}",
#         f"calculation-output/batch_id={executed_batch_id}/actors/grid_area=805/time_series_type=non_profiled_consumption/actor_type=energy_supplier/part-*.json",
#     )
#     assert re.match(expected_path_expression, actual_result_file)


# def test__result_file_path_matches_contract(
#     data_lake_path,
#     find_first_file,
#     worker_id,
#     executed_calculation_job,
#     calculation_file_paths_contract,
# ):
#     # Arrange
#     contract = calculation_file_paths_contract.result_file
#     expected_path_expression = create_file_path_expression(
#         contract.directory_expression,
#         contract.extension,
#     )
#     # Act: Executed in fixture executed_calculation_job

#     # Assert
#     actual_result_file = find_first_file(
#         f"{data_lake_path}/{worker_id}",
#         f"calculation-output/batch_id={executed_batch_id}/result/grid_area=805/gln={grid_area_gln}/time_series_type=production/part-*.json",
#     )
#     assert re.match(expected_path_expression, actual_result_file)
