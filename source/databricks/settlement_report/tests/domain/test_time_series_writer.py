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
from settlement_report_job.domain import time_series_writer

from pyspark.sql import SparkSession

from tests.fixtures import DBUtilsFixture
import pytest
from settlement_report_job.domain.metering_point_type import MeteringPointType
from settlement_report_job.domain.metering_point_resolution import (
    DataProductMeteringPointResolution,
)
from settlement_report_job.domain.report_data_type import ReportDataType

from settlement_report_job.domain.settlement_report_args import SettlementReportArgs
import tests.test_factories.time_series_csv_factory as factory


@pytest.mark.parametrize(
    "resolution,grid_area_codes,expected_file_count",
    [
        (DataProductMeteringPointResolution.HOUR, ["804", "805"], 2),
        (DataProductMeteringPointResolution.QUARTER, ["804", "805"], 2),
        (DataProductMeteringPointResolution.HOUR, ["804"], 1),
        (DataProductMeteringPointResolution.QUARTER, ["804", "805", "806"], 3),
    ],
)
def test_write__returns_files_corresponding_to_grid_area_codes(
    dbutils: DBUtilsFixture,
    spark: SparkSession,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    resolution: DataProductMeteringPointResolution,
    grid_area_codes: list[str],
    expected_file_count: int,
):
    # Arrange
    report_data_type = (
        ReportDataType.TimeSeriesHourly
        if resolution == DataProductMeteringPointResolution.HOUR
        else ReportDataType.TimeSeriesQuarterly
    )
    test_spec = factory.TimeSeriesCsvTestDataSpec(
        metering_point_type=MeteringPointType.CONSUMPTION,
        start_of_day=standard_wholesale_fixing_scenario_args.period_start,
        grid_area_codes=grid_area_codes,
        energy_quantity=235.0,
        resolution=resolution,
    )
    mock_prepared_time_series = factory.create(spark, test_spec)

    # Act
    result_files = time_series_writer.write(
        dbutils=dbutils,
        args=standard_wholesale_fixing_scenario_args,
        prepared_time_series=mock_prepared_time_series,
        report_data_type=report_data_type,
    )

    # Assert
    assert len(result_files) > 0
    assert len(result_files) == expected_file_count


def test_write__when_higher_default_parallelism__number_of_files_is_unchanged(
    dbutils: DBUtilsFixture,
    spark: SparkSession,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
):
    # Arrange
    spark.conf.set("spark.sql.shuffle.partitions", "10")
    spark.conf.set("spark.default.parallelism", "10")
    report_data_type = ReportDataType.TimeSeriesHourly
    resolution = DataProductMeteringPointResolution.HOUR
    expected_file_count = 2
    test_spec = factory.TimeSeriesCsvTestDataSpec(
        metering_point_type=MeteringPointType.CONSUMPTION,
        start_of_day=standard_wholesale_fixing_scenario_args.period_start,
        grid_area_codes=["804", "805"],
        energy_quantity=235.0,
        resolution=resolution,
    )
    mock_prepared_time_series = factory.create(spark, test_spec)

    # Act
    result_files = time_series_writer.write(
        dbutils=dbutils,
        args=standard_wholesale_fixing_scenario_args,
        prepared_time_series=mock_prepared_time_series,
        report_data_type=report_data_type,
    )

    # Assert
    assert len(result_files) > 0
    assert len(result_files) == expected_file_count


@pytest.mark.parametrize(
    "number_of_rows,rows_per_file,expected_file_count",
    [
        (201, 100, 3),
        (101, 100, 2),
        (100, 100, 1),
        (99, 100, 1),
    ],
)
def test_write__when_prevent_large_files_is_enabled__writes_expected_number_of_files(
    dbutils: DBUtilsFixture,
    spark: SparkSession,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    number_of_rows: int,
    rows_per_file: int,
    expected_file_count: int,
):
    # Arrange
    report_data_type = ReportDataType.TimeSeriesHourly
    resolution = DataProductMeteringPointResolution.HOUR
    standard_wholesale_fixing_scenario_args.prevent_large_text_files = True
    test_spec = factory.TimeSeriesCsvTestDataSpec(
        metering_point_type=MeteringPointType.CONSUMPTION,
        start_of_day=standard_wholesale_fixing_scenario_args.period_start,
        grid_area_codes=["804"],
        energy_quantity=235.0,
        resolution=resolution,
        num_metering_points=number_of_rows,
    )
    mock_prepared_time_series = factory.create(spark, test_spec)

    # Act
    result_files = time_series_writer.write(
        dbutils=dbutils,
        args=standard_wholesale_fixing_scenario_args,
        prepared_time_series=mock_prepared_time_series,
        report_data_type=report_data_type,
        rows_per_file=rows_per_file,
    )

    # Assert
    assert mock_prepared_time_series.count() == number_of_rows
    assert len(result_files) > 0
    assert len(result_files) == expected_file_count