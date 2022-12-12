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
from geh_stream.codelists import Colname, ResultKeyName, ResolutionDuration, MarketEvaluationPointType
from geh_stream.aggregation_utils.aggregators import calculate_added_system_correction
from geh_stream.codelists import Quality
from geh_stream.shared.data_classes import Metadata
from geh_stream.schemas.output import aggregation_result_schema
from geh_stream.aggregation_utils.aggregation_result_formatter import create_dataframe_from_aggregation_result_schema
from pyspark.sql.dataframe import DataFrame
from pyspark.sql.types import StructType, StringType, DecimalType, TimestampType
from pyspark.sql.functions import col
import pytest
import pandas as pd


@pytest.fixture(scope="module")
def grid_loss_schema():
    return StructType() \
        .add(Colname.grid_area, StringType(), False) \
        .add(Colname.time_window,
             StructType()
             .add(Colname.start, TimestampType())
             .add(Colname.end, TimestampType()),
             False) \
        .add(Colname.sum_quantity, DecimalType(18, 3)) \
        .add(Colname.quality, StringType()) \
        .add(Colname.resolution, StringType()) \
        .add(Colname.metering_point_type, StringType())


@pytest.fixture(scope="module")
def agg_result_factory(spark, grid_loss_schema):
    """
    Factory to generate a single row of time series data, with default parameters as specified above.
    """
    def factory():
        pandas_df = pd.DataFrame({
            Colname.grid_area: [],
            Colname.time_window: [],
            Colname.sum_quantity: [],
            Colname.resolution: [],
            Colname.metering_point_type: []
        })
        pandas_df = pandas_df.append([{
            Colname.grid_area: str(1), Colname.time_window: {Colname.start: datetime(2020, 1, 1, 0, 0), Colname.end: datetime(2020, 1, 1, 1, 0)},
            Colname.sum_quantity: Decimal(-12.567), Colname.quality: Quality.estimated.value, Colname.resolution: ResolutionDuration.hour,
            Colname.metering_point_type: MarketEvaluationPointType.exchange.value}, {
            Colname.grid_area: str(2), Colname.time_window: {Colname.start: datetime(2020, 1, 1, 0, 0), Colname.end: datetime(2020, 1, 1, 1, 0)},
            Colname.sum_quantity: Decimal(34.32), Colname.quality: Quality.estimated.value, Colname.resolution: ResolutionDuration.hour,
            Colname.metering_point_type: MarketEvaluationPointType.exchange.value}, {
            Colname.grid_area: str(3), Colname.time_window: {Colname.start: datetime(2020, 1, 1, 0, 0), Colname.end: datetime(2020, 1, 1, 1, 0)},
            Colname.sum_quantity: Decimal(0.0), Colname.quality: Quality.estimated.value, Colname.resolution: ResolutionDuration.hour,
            Colname.metering_point_type: MarketEvaluationPointType.exchange.value}],
            ignore_index=True)

        return spark.createDataFrame(pandas_df, schema=grid_loss_schema)
    return factory


def call_calculate_added_system_correction(agg_result_factory) -> DataFrame:
    metadata = Metadata("1", "1", "1", "1", "1")
    results = {}
    results[ResultKeyName.grid_loss] = create_dataframe_from_aggregation_result_schema(metadata, agg_result_factory())
    return calculate_added_system_correction(results, metadata)


def test_added_system_correction_has_no_values_below_zero(agg_result_factory):
    result = call_calculate_added_system_correction(agg_result_factory)

    assert result.filter(col(Colname.added_system_correction) < 0).count() == 0


def test_added_system_correction_change_negative_value_to_positive(agg_result_factory):
    result = call_calculate_added_system_correction(agg_result_factory)

    assert result.collect()[0][Colname.added_system_correction] == Decimal("12.56700")


def test_added_system_correction_change_positive_value_to_zero(agg_result_factory):
    result = call_calculate_added_system_correction(agg_result_factory)

    assert result.collect()[1][Colname.added_system_correction] == Decimal("0.00000")


def test_added_system_correction_values_that_are_zero_stay_zero(agg_result_factory):
    result = call_calculate_added_system_correction(agg_result_factory)

    assert result.collect()[2][Colname.added_system_correction] == Decimal("0.00000")


def test_returns_correct_schema(agg_result_factory):

    result = call_calculate_added_system_correction(agg_result_factory)
    assert result.schema == aggregation_result_schema
