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
from os import truncate
import pytest
from decimal import Decimal
import pandas as pd
from datetime import datetime, timedelta
from geh_stream.codelists import Colname, ResultKeyName
from geh_stream.aggregation_utils.aggregators import aggregate_net_exchange_per_neighbour_ga
from geh_stream.codelists import MarketEvaluationPointType, ConnectionState, Quality
from geh_stream.shared.data_classes import Metadata
from geh_stream.schemas.output import aggregation_result_schema
from pyspark.sql.types import StructType, StringType, DecimalType, TimestampType


e_20 = MarketEvaluationPointType.exchange.value
date_time_formatting_string = "%Y-%m-%dT%H:%M:%S%z"
default_obs_time = datetime.strptime(
    "2020-01-01T00:00:00+0000",
    date_time_formatting_string)
numberOfTestHours = 24
estimated_quality = Quality.estimated.value
metadata = Metadata("1", "1", "1", "1", "1")

df_template = {
    Colname.grid_area: [],
    Colname.metering_point_type: [],
    Colname.in_grid_area: [],
    Colname.out_grid_area: [],
    Colname.quantity: [],
    Colname.time: [],
    Colname.connection_state: [],
    Colname.aggregated_quality: []
}


@pytest.fixture(scope="module")
def time_series_schema():
    return StructType() \
        .add(Colname.grid_area, StringType()) \
        .add(Colname.metering_point_type, StringType()) \
        .add(Colname.in_grid_area, StringType()) \
        .add(Colname.out_grid_area, StringType()) \
        .add(Colname.quantity, DecimalType(38)) \
        .add(Colname.time, TimestampType()) \
        .add(Colname.connection_state, StringType()) \
        .add(Colname.aggregated_quality, StringType())


@pytest.fixture(scope="module")
def single_hour_test_data(spark, time_series_schema):
    pandas_df = pd.DataFrame(df_template)
    pandas_df = add_row_of_data(pandas_df, "A", "A", "B", default_obs_time, Decimal("10"))
    pandas_df = add_row_of_data(pandas_df, "A", "A", "B", default_obs_time, Decimal("15"))
    pandas_df = add_row_of_data(pandas_df, "A", "B", "A", default_obs_time, Decimal("5"))
    pandas_df = add_row_of_data(pandas_df, "B", "B", "A", default_obs_time, Decimal("10"))
    pandas_df = add_row_of_data(pandas_df, "A", "A", "C", default_obs_time, Decimal("20"))
    pandas_df = add_row_of_data(pandas_df, "C", "C", "A", default_obs_time, Decimal("10"))
    pandas_df = add_row_of_data(pandas_df, "C", "C", "A", default_obs_time, Decimal("5"))
    return spark.createDataFrame(pandas_df, schema=time_series_schema)


@pytest.fixture(scope="module")
def multi_hour_test_data(spark, time_series_schema):
    pandas_df = pd.DataFrame(df_template)
    for i in range(numberOfTestHours):
        pandas_df = add_row_of_data(pandas_df, "A", "A", "B", default_obs_time + timedelta(hours=i), Decimal("10"))
        pandas_df = add_row_of_data(pandas_df, "A", "A", "B", default_obs_time + timedelta(hours=i), Decimal("15"))
        pandas_df = add_row_of_data(pandas_df, "A", "B", "A", default_obs_time + timedelta(hours=i), Decimal("5"))
        pandas_df = add_row_of_data(pandas_df, "B", "B", "A", default_obs_time + timedelta(hours=i), Decimal("10"))
        pandas_df = add_row_of_data(pandas_df, "A", "A", "C", default_obs_time + timedelta(hours=i), Decimal("20"))
        pandas_df = add_row_of_data(pandas_df, "C", "C", "A", default_obs_time + timedelta(hours=i), Decimal("10"))
        pandas_df = add_row_of_data(pandas_df, "C", "C", "A", default_obs_time + timedelta(hours=i), Decimal("5"))
    return spark.createDataFrame(pandas_df, schema=time_series_schema)


def add_row_of_data(pandas_df, domain, in_domain, out_domain, timestamp, quantity):
    new_row = {Colname.grid_area: domain,
               Colname.metering_point_type: e_20,
               Colname.in_grid_area: in_domain,
               Colname.out_grid_area: out_domain,
               Colname.quantity: quantity,
               Colname.time: timestamp,
               Colname.connection_state: ConnectionState.connected.value,
               Colname.aggregated_quality: estimated_quality}
    return pandas_df.append(new_row, ignore_index=True)


def test_aggregate_net_exchange_per_neighbour_ga_single_hour(single_hour_test_data):
    results = {}
    results[ResultKeyName.aggregation_base_dataframe] = single_hour_test_data
    df = aggregate_net_exchange_per_neighbour_ga(results, metadata).orderBy(
        Colname.in_grid_area,
        Colname.out_grid_area,
        Colname.time_window)
    values = df.collect()
    assert df.count() == 4
    assert values[0][Colname.in_grid_area] == "A"
    assert values[1][Colname.out_grid_area] == "C"
    assert values[2][Colname.in_grid_area] == "B"
    assert values[0][Colname.sum_quantity] == Decimal("10")
    assert values[1][Colname.sum_quantity] == Decimal("5")
    assert values[2][Colname.sum_quantity] == Decimal("-10")
    assert values[3][Colname.sum_quantity] == Decimal("-5")


def test_aggregate_net_exchange_per_neighbour_ga_multi_hour(multi_hour_test_data):
    results = {}
    results[ResultKeyName.aggregation_base_dataframe] = multi_hour_test_data
    df = aggregate_net_exchange_per_neighbour_ga(results, metadata).orderBy(
        Colname.in_grid_area,
        Colname.out_grid_area,
        Colname.time_window)
    values = df.collect()
    assert df.count() == 96
    assert values[0][Colname.in_grid_area] == "A"
    assert values[0][Colname.out_grid_area] == "B"
    assert values[0][Colname.time_window][Colname.start].strftime(date_time_formatting_string) == "2020-01-01T00:00:00"
    assert values[0][Colname.time_window][Colname.end].strftime(date_time_formatting_string) == "2020-01-01T01:00:00"
    assert values[0][Colname.sum_quantity] == Decimal("10")
    assert values[19][Colname.in_grid_area] == "A"
    assert values[19][Colname.out_grid_area] == "B"
    assert values[19][Colname.time_window][Colname.start].strftime(date_time_formatting_string) == "2020-01-01T19:00:00"
    assert values[19][Colname.time_window][Colname.end].strftime(date_time_formatting_string) == "2020-01-01T20:00:00"
    assert values[19][Colname.sum_quantity] == Decimal("10")


def test_expected_schema(single_hour_test_data):
    results = {}
    results[ResultKeyName.aggregation_base_dataframe] = single_hour_test_data
    df = aggregate_net_exchange_per_neighbour_ga(results, metadata).orderBy(
        Colname.in_grid_area,
        Colname.out_grid_area,
        Colname.time_window)
    assert df.schema == aggregation_result_schema
