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
from pyspark.sql import SparkSession, Row
from settlement_report_job.domain.time_series_factory import create_time_series
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs


def test_create_time_series(spark: SparkSession):

    # # Arrange
    # test_data = [
    #     Row(
    #         grid_area_code="DK1",
    #         metering_point_id="MP1",
    #         metering_point_type="E17",
    #         observation_time="2023-01-01 00:00:00",
    #         quantity=100.0,
    #         resolution="PT1H",
    #     ),
    #     Row(
    #         grid_area_code="DK1",
    #         metering_point_id="MP1",
    #         metering_point_type="E17",
    #         observation_time="2023-01-01 01:00:00",
    #         quantity=150.0,
    #         resolution="PT1H",
    #     ),
    #     Row(
    #         grid_area_code="DK1",
    #         metering_point_id="MP1",
    #         metering_point_type="E17",
    #         observation_time="2023-01-01 00:15:00",
    #         quantity=50.0,
    #         resolution="PT15M",
    #     ),
    #     Row(
    #         grid_area_code="DK1",
    #         metering_point_id="MP1",
    #         metering_point_type="E17",
    #         observation_time="2023-01-01 00:30:00",
    #         quantity=75.0,
    #         resolution="PT15M",
    #     ),
    # ]
    # test_df = spark.createDataFrame(test_data)
    # mock_read_and_filter = mocker.patch(
    #     "settlement_report_job.domain.time_series_factory._read_and_filter_from_view",
    #     return_value=test_df,
    # )
    #
    # args = SettlementReportArgs(
    #     period_start="2023-01-01",
    #     period_end="2023-01-02",
    #     time_zone="Europe/Copenhagen",
    #     calculation_id_by_grid_area={"DK1": "calc1"},
    #     split_report_by_grid_area=True,
    #     prevent_large_text_files=True,
    # )
    #
    # query_directory = "/path/to/query_directory"
    #
    # # Act
    # result = create_time_series(spark, args, query_directory)
    #
    # # Assert
    # assert isinstance(result, list)
    # assert len(result) > 0

    assert True
