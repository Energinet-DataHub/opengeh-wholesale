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
from decimal import Decimal
import pandas as pd
from datetime import datetime, timedelta
from geh_stream.codelists import Colname, ResultKeyName
from geh_stream.aggregation_utils.aggregators import aggregate_net_exchange_per_ga
from geh_stream.codelists import MarketEvaluationPointType, ConnectionState, Quality
from geh_stream.shared.data_classes import Metadata
from geh_stream.schemas.output import aggregation_result_schema
from pyspark.sql import DataFrame
import pyspark.sql.functions as F
from pyspark.sql.types import StructType, StringType, DecimalType, TimestampType


e_20 = MarketEvaluationPointType.exchange.value
date_time_formatting_string = "%Y-%m-%dT%H:%M:%S%z"
default_obs_time = datetime.strptime(
    "2020-01-01T00:00:00+0000", date_time_formatting_string)
numberOfTestHours = 24

metadata = Metadata("1", "1", "1", "1", "1")

# Time series schema


@pytest.fixture(scope="module")
def time_series_schema():
    return StructType() \
        .add(Colname.metering_point_type, StringType(), False) \
        .add(Colname.in_grid_area, StringType()) \
        .add(Colname.out_grid_area, StringType(), False) \
        .add(Colname.quantity, DecimalType(38, 10)) \
        .add(Colname.time, TimestampType()) \
        .add(Colname.connection_state, StringType()) \
        .add(Colname.aggregated_quality, StringType())


@pytest.fixture(scope="module")
def expected_schema():
    """
    Expected exchange aggregation output schema
    """
    return StructType() \
        .add(Colname.grid_area, StringType()) \
        .add(Colname.time_window,
             StructType()
             .add(Colname.start, TimestampType())
             .add(Colname.end, TimestampType())
             ) \
        .add(Colname.sum_quantity, DecimalType(38, 9)) \
        .add(Colname.aggregated_quality, StringType())


@pytest.fixture(scope="module")
def time_series_data_frame(spark, time_series_schema):
    """
    Sample Time Series DataFrame
    """
    # Create empty pandas df
    pandas_df = pd.DataFrame({
        Colname.metering_point_type: [],
        Colname.in_grid_area: [],
        Colname.out_grid_area: [],
        Colname.quantity: [],
        Colname.time: [],
        Colname.connection_state: [],
        Colname.aggregated_quality: []
    })

    # add 24 hours of exchange with different examples of exchange between grid areas. See readme.md for more info

    for x in range(numberOfTestHours):
        pandas_df = add_row_of_data(pandas_df, e_20, "B", "A", Decimal(2) * x, default_obs_time + timedelta(hours=x), ConnectionState.connected.value)

        pandas_df = add_row_of_data(pandas_df, e_20, "B", "A", Decimal("0.5") * x, default_obs_time + timedelta(hours=x), ConnectionState.connected.value)
        pandas_df = add_row_of_data(pandas_df, e_20, "B", "A", Decimal("0.7") * x, default_obs_time + timedelta(hours=x), ConnectionState.connected.value)

        pandas_df = add_row_of_data(pandas_df, e_20, "A", "B", Decimal(3) * x, default_obs_time + timedelta(hours=x), ConnectionState.connected.value)
        pandas_df = add_row_of_data(pandas_df, e_20, "A", "B", Decimal("0.9") * x, default_obs_time + timedelta(hours=x), ConnectionState.connected.value)
        pandas_df = add_row_of_data(pandas_df, e_20, "A", "B", Decimal("1.2") * x, default_obs_time + timedelta(hours=x), ConnectionState.connected.value)

        pandas_df = add_row_of_data(pandas_df, e_20, "C", "A", Decimal("0.7") * x, default_obs_time + timedelta(hours=x), ConnectionState.connected.value)
        pandas_df = add_row_of_data(pandas_df, e_20, "A", "C", Decimal("1.1") * x, default_obs_time + timedelta(hours=x), ConnectionState.connected.value)
        pandas_df = add_row_of_data(pandas_df, e_20, "A", "C", Decimal("1.5") * x, default_obs_time + timedelta(hours=x), ConnectionState.connected.value)

    return spark.createDataFrame(pandas_df, schema=time_series_schema)


def add_row_of_data(pandas_df: pd.DataFrame, point_type, in_domain, out_domain, quantity: Decimal, timestamp, connectionState):
    """
    Helper method to create a new row in the dataframe to improve readability and maintainability
    """
    new_row = {
        Colname.metering_point_type: point_type,
        Colname.in_grid_area: in_domain,
        Colname.out_grid_area: out_domain,
        Colname.quantity: quantity,
        Colname.time: timestamp,
        Colname.connection_state: connectionState,
        Colname.aggregated_quality: Quality.estimated.value
    }
    return pandas_df.append(new_row, ignore_index=True)


@pytest.fixture(scope="module")
def aggregated_data_frame(time_series_data_frame):
    """Perform aggregation"""
    results = {}
    results[ResultKeyName.aggregation_base_dataframe] = time_series_data_frame
    result = aggregate_net_exchange_per_ga(results, metadata)
    return result


def test_test_data_has_correct_row_count(time_series_data_frame):
    """ Check sample data row count"""
    assert time_series_data_frame.count() == (9 * numberOfTestHours)


def test_exchange_aggregator_returns_correct_schema(aggregated_data_frame):
    """Check aggregation schema"""
    assert aggregated_data_frame.schema == aggregation_result_schema


def test_exchange_aggregator_returns_correct_aggregations(aggregated_data_frame):
    """Check accuracy of aggregations"""

    for x in range(numberOfTestHours):
        check_aggregation_row(aggregated_data_frame, "A", Decimal("3.8") * x, default_obs_time + timedelta(hours=x))
        check_aggregation_row(aggregated_data_frame, "B", Decimal("-1.9") * x, default_obs_time + timedelta(hours=x))
        check_aggregation_row(aggregated_data_frame, "C", Decimal("-1.9") * x, default_obs_time + timedelta(hours=x))


def check_aggregation_row(df: DataFrame, MeteringGridArea_Domain_mRID: str, sum: Decimal, time: datetime):
    """Helper function that checks column values for the given row"""
    gridfiltered = df.filter(df[Colname.grid_area] == MeteringGridArea_Domain_mRID).select(F.col(Colname.grid_area), F.col(
        Colname.sum_quantity), F.col(f"{Colname.time_window_start}").alias("start"), F.col(f"{Colname.time_window_end}").alias("end"))
    res = gridfiltered.filter(gridfiltered["start"] == time).toPandas()
    assert res[Colname.sum_quantity][0] == sum
