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
import tests.test_factories.time_series_points_csv_factory as factory
from datetime import datetime


@pytest.mark.parametrize(
    "resolution",
    [
        DataProductMeteringPointResolution.HOUR,
        DataProductMeteringPointResolution.QUARTER,
    ],
)
def test_write_files(
    dbutils: DBUtilsFixture,
    spark: SparkSession,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    resolution: DataProductMeteringPointResolution,
):
    # Arrange
    expected_files = 2
    report_data_type = (
        ReportDataType.TimeSeriesHourly
        if resolution == DataProductMeteringPointResolution.HOUR
        else ReportDataType.TimeSeriesQuarterly
    )
    test_spec = factory.TimeSeriesPointCsvTestDataSpec(
        metering_point_type=MeteringPointType.CONSUMPTION,
        start_of_day=standard_wholesale_fixing_scenario_args.period_start,
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
    assert len(result_files) == expected_files
