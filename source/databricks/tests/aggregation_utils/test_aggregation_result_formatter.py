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
from decimal import Decimal
from datetime import datetime
from geh_stream.codelists import Colname, ResolutionDuration, MarketEvaluationPointType
from geh_stream.aggregation_utils.aggregation_result_formatter import create_dataframe_from_aggregation_result_schema
from tests.helpers.dataframe_creators import aggregation_result_factory
from geh_stream.codelists import Quality
from geh_stream.shared.data_classes import Metadata
from geh_stream.schemas.output import aggregation_result_schema
import pytest
import pandas as pd


@pytest.fixture(scope="module")
def agg_result_factory(spark):
    def factory(
        grid_area="A",
        start=datetime(2020, 1, 1, 0, 0),
        end=datetime(2020, 1, 1, 1, 0),
        resolution=ResolutionDuration.hour,
        sum_quantity=Decimal("1.234"),
        quality=Quality.estimated.value,
        metering_point_type=MarketEvaluationPointType.consumption.value
    ):
        return spark.createDataFrame(pd.DataFrame().append([{
            Colname.grid_area: grid_area,
            Colname.time_window: {
                Colname.start: start,
                Colname.end: end},
            Colname.resolution: resolution,
            Colname.sum_quantity: sum_quantity,
            Colname.quality: quality,
            Colname.metering_point_type: metering_point_type}],
            ignore_index=True))
    return factory


def test__create_dataframe_from_aggregation_result_schema__can_create_a_dataframe_that_match_aggregation_result_schema(agg_result_factory):
    # Arrange
    metadata = Metadata("1", "1", "1", "1", "1")
    result = agg_result_factory()
    # Act
    actual = create_dataframe_from_aggregation_result_schema(metadata, result)
    # Assert
    assert actual.schema == aggregation_result_schema


def test__create_dataframe_from_aggregation_result_schema__match_expected_dataframe(agg_result_factory, aggregation_result_factory):
    # Arrange
    metadata = Metadata("1", "1", "1", "1", "1")
    result = agg_result_factory()
    expected = aggregation_result_factory(
        grid_area="A",
        time_window_start=datetime(2020, 1, 1, 0, 0),
        time_window_end=datetime(2020, 1, 1, 1, 0),
        resolution=ResolutionDuration.hour,
        sum_quantity=Decimal("1.234"),
        quality=Quality.estimated.value,
        metering_point_type=MarketEvaluationPointType.consumption.value
    )
    # Act
    actual = create_dataframe_from_aggregation_result_schema(metadata, result)
    # Assert
    assert actual.collect() == expected.collect()
