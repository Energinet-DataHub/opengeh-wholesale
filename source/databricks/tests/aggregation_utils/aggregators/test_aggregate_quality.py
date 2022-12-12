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
from datetime import datetime, timedelta
from geh_stream.codelists import Colname
from pyspark.sql.types import StructType, StringType, TimestampType
from geh_stream.codelists import Quality, MarketEvaluationPointType
from geh_stream.aggregation_utils.aggregators import aggregate_quality
import pytest
import pandas as pd

date_time_formatting_string = "%Y-%m-%dT%H:%M:%S%z"
default_obs_time = datetime.strptime("2020-01-01T00:00:00+0000", date_time_formatting_string)

qualities = ["E01", "56", "D01", "QM"]
mp = ["E17", "E18"]


@pytest.fixture(scope="module")
def schema():
    return StructType() \
        .add(Colname.grid_area, StringType(), False) \
        .add(Colname.metering_point_type, StringType()) \
        .add(Colname.time, TimestampType()) \
        .add(Colname.quality, StringType())


@pytest.fixture(scope="module")
def expected_schema():
    return StructType() \
        .add(Colname.grid_area, StringType(), False) \
        .add(Colname.metering_point_type, StringType()) \
        .add(Colname.time, TimestampType()) \
        .add(Colname.quality, StringType()) \
        .add(Colname.aggregated_quality, StringType(), False)


# Create test data factory containing three consumption entries within the same grid area and time window
@pytest.fixture(scope="module")
def test_data_factory(spark, schema):
    def factory(quality_1,
                quality_2,
                quality_3):
        df_qualities = [quality_1, quality_2, quality_3]
        pandas_df = pd.DataFrame({
            Colname.grid_area: [],
            Colname.metering_point_type: [],
            Colname.time: [],
            Colname.quality: [],
        })
        for i in range(3):
            pandas_df = pandas_df.append({
                Colname.grid_area: str(1),
                Colname.metering_point_type: MarketEvaluationPointType.consumption.value,
                Colname.time: default_obs_time + timedelta(hours=1),
                Colname.quality: df_qualities[i]
            }, ignore_index=True)
        return spark.createDataFrame(pandas_df, schema=schema)
    return factory


def test_set_aggregated_quality_to_estimated_when_quality_within_hour_is_estimated_and_read(test_data_factory):
    df = test_data_factory(Quality.estimated.value, Quality.as_read.value, Quality.as_read.value)

    result_df = aggregate_quality(df)

    assert(result_df.collect()[0][Colname.aggregated_quality] == Quality.estimated.value)


def test_set_aggregated_quality_to_estimated_when_quality_within_hour_is_estimated_and_calculated(test_data_factory):
    df = test_data_factory(Quality.estimated.value, Quality.calculated.value, Quality.calculated.value)

    result_df = aggregate_quality(df)

    assert(result_df.collect()[0][Colname.aggregated_quality] == Quality.estimated.value)


def test_set_aggregated_quality_to_estimated_when_quality_within_hour_is_estimated_calculated_and_read(test_data_factory):
    df = test_data_factory(Quality.calculated.value, Quality.as_read.value, Quality.estimated.value)

    result_df = aggregate_quality(df)

    assert(result_df.collect()[0][Colname.aggregated_quality] == Quality.estimated.value)


def test_set_aggregated_quality_to_estimated_when_quality_within_hour_is_estimated_and_quantity_missing(test_data_factory):
    df = test_data_factory(Quality.estimated.value, Quality.quantity_missing.value, Quality.quantity_missing.value)

    result_df = aggregate_quality(df)

    assert(result_df.collect()[0][Colname.aggregated_quality] == Quality.estimated.value)


def test_set_aggregated_quality_to_estimated_when_quality_within_hour_is_read_and_quantity_missing(test_data_factory):
    df = test_data_factory(Quality.as_read.value, Quality.quantity_missing.value, Quality.quantity_missing.value)

    result_df = aggregate_quality(df)

    assert(result_df.collect()[0][Colname.aggregated_quality] == Quality.estimated.value)


def test_set_aggregated_quality_to_estimated_when_quality_within_hour_is_calculated_and_quantity_missing(test_data_factory):
    df = test_data_factory(Quality.calculated.value, Quality.quantity_missing.value, Quality.quantity_missing.value)

    result_df = aggregate_quality(df)

    assert(result_df.collect()[0][Colname.aggregated_quality] == Quality.estimated.value)


def test_set_aggregated_quality_to_read_when_quality_within_hour_is_either_read_or_calculated(test_data_factory):
    df = test_data_factory(Quality.as_read.value, Quality.calculated.value, Quality.calculated.value)

    result_df = aggregate_quality(df)

    assert(result_df.collect()[0][Colname.aggregated_quality] == Quality.as_read.value)


def test_returns_correct_schema(test_data_factory, expected_schema):
    """
    Aggregator should return the correct schema, including the proper fields for the aggregated quantity values
    and time window (from the single-hour resolution specified in the aggregator).
    """
    df = test_data_factory(Quality.estimated.value, Quality.estimated.value, Quality.estimated.value)
    aggregated_df = aggregate_quality(df)
    assert aggregated_df.schema == expected_schema


# Create test data factory containing one consumption and one production entry within the same grid area and time window
@pytest.fixture(scope="module")
def test_data_factory_with_diff_market_evalution_point_type(spark, schema):
    def factory():
        df_qualities = Quality.estimated.value
        pandas_df = pd.DataFrame({
            Colname.grid_area: [],
            Colname.metering_point_type: [],
            Colname.time: [],
            Colname.quality: [],
        })
        for i in range(2):
            pandas_df = pandas_df.append({
                Colname.grid_area: str(1),
                Colname.metering_point_type: mp[i],
                Colname.time: default_obs_time + timedelta(hours=1),
                Colname.quality: df_qualities[i]
            }, ignore_index=True)
        return spark.createDataFrame(pandas_df, schema=schema)
    return factory


def test_input_and_output_dataframe_should_return_same_row_count(test_data_factory_with_diff_market_evalution_point_type):
    # Input dataframe
    df = test_data_factory_with_diff_market_evalution_point_type()

    # Output dataframe
    result_df = aggregate_quality(df)

    assert df.count() == result_df.count()
