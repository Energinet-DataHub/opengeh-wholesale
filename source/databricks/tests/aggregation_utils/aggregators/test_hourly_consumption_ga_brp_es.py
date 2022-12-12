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
from geh_stream.codelists import Colname, ResultKeyName
from geh_stream.aggregation_utils.aggregators import aggregate_hourly_consumption, aggregate_per_ga_and_brp_and_es
from geh_stream.codelists import MarketEvaluationPointType, SettlementMethod, ConnectionState, Quality
from geh_stream.shared.data_classes import Metadata
from geh_stream.schemas.output import aggregation_result_schema
from pyspark.sql import DataFrame
from pyspark.sql.types import StructType, StringType, DecimalType, TimestampType
import pytest
import pandas as pd

e_17 = MarketEvaluationPointType.consumption.value
e_18 = MarketEvaluationPointType.production.value
e_01 = SettlementMethod.profiled.value
e_02 = SettlementMethod.non_profiled.value
connected = ConnectionState.connected.value

# Default time series data point values
default_point_type = e_17
default_settlement_method = e_02
default_domain = "D1"
default_responsible = "R1"
default_supplier = "S1"
default_quantity = Decimal(1)
default_connection_state = connected

date_time_formatting_string = "%Y-%m-%dT%H:%M:%S%z"
default_obs_time = datetime.strptime("2020-01-01T00:00:00+0000", date_time_formatting_string)

metadata = Metadata("1", "1", "1", "1", "1")


@pytest.fixture(scope="module")
def time_series_schema():
    """
    Input time series data point schema
    """
    return StructType() \
        .add(Colname.metering_point_type, StringType(), False) \
        .add(Colname.settlement_method, StringType()) \
        .add(Colname.grid_area, StringType(), False) \
        .add(Colname.balance_responsible_id, StringType()) \
        .add(Colname.energy_supplier_id, StringType()) \
        .add(Colname.quantity, DecimalType()) \
        .add(Colname.time, TimestampType()) \
        .add(Colname.connection_state, StringType()) \
        .add(Colname.aggregated_quality, StringType())


@pytest.fixture(scope="module")
def time_series_row_factory(spark, time_series_schema):
    """
    Factory to generate a single row of time series data, with default parameters as specified above.
    """
    def factory(point_type=default_point_type,
                settlement_method=default_settlement_method,
                domain=default_domain,
                responsible=default_responsible,
                supplier=default_supplier,
                quantity=default_quantity,
                obs_time=default_obs_time,
                connection_state=default_connection_state):
        pandas_df = pd.DataFrame({
            Colname.metering_point_type: [point_type],
            Colname.settlement_method: [settlement_method],
            Colname.grid_area: [domain],
            Colname.balance_responsible_id: [responsible],
            Colname.energy_supplier_id: [supplier],
            Colname.quantity: [quantity],
            Colname.time: [obs_time],
            Colname.connection_state: [connection_state],
            Colname.aggregated_quality: [Quality.estimated.value]},
                                )
        return spark.createDataFrame(pandas_df, schema=time_series_schema)
    return factory


def check_aggregation_row(df: DataFrame, row: int, grid: str, responsible: str, supplier: str, sum: Decimal, start: datetime, end: datetime):
    """
    Helper function that checks column values for the given row.
    Note that start and end datetimes are timezone-naive - we set the Spark session timezone to UTC in the
    conftest.py, since:
        From https://stackoverflow.com/questions/48746376/timestamptype-in-pyspark-with-datetime-tzaware-objects:
        "TimestampType in pyspark is not tz aware like in Pandas rather it passes long ints
        and displays them according to your machine's local time zone (by default)"
    """
    pandas_df = df.toPandas()
    assert pandas_df[Colname.grid_area][row] == grid
    assert pandas_df[Colname.balance_responsible_id][row] == responsible
    assert pandas_df[Colname.energy_supplier_id][row] == supplier
    assert pandas_df[Colname.sum_quantity][row] == sum
    assert pandas_df[Colname.time_window][row].start == start
    assert pandas_df[Colname.time_window][row].end == end


def test_hourly_consumption_supplier_aggregator_filters_out_incorrect_point_type(time_series_row_factory):
    """
    Aggregator should filter out all non "E17" MarketEvaluationPointType rows
    """
    results = {}
    results[ResultKeyName.aggregation_base_dataframe] = time_series_row_factory(point_type=e_18)
    aggregated_df = aggregate_hourly_consumption(results, metadata)
    assert aggregated_df.count() == 0


def test_hourly_consumption_supplier_aggregator_filters_out_incorrect_settlement_method(time_series_row_factory):
    """
    Aggregator should filter out all non "E02" SettlementMethod rows
    """
    results = {}
    results[ResultKeyName.aggregation_base_dataframe] = time_series_row_factory(settlement_method=e_01)
    aggregated_df = aggregate_hourly_consumption(results, metadata)
    assert aggregated_df.count() == 0


def test_hourly_consumption_supplier_aggregator_aggregates_observations_in_same_hour(time_series_row_factory):
    """
    Aggregator should can calculate the correct sum of a "domain"-"responsible"-"supplier" grouping within the
    same 1hr time window
    """
    results = {}
    row1_df = time_series_row_factory(quantity=Decimal(1))
    row2_df = time_series_row_factory(quantity=Decimal(2))
    results[ResultKeyName.aggregation_base_dataframe] = row1_df.union(row2_df)
    aggregated_df = aggregate_hourly_consumption(results, metadata)

    # Create the start/end datetimes representing the start and end of the 1 hr time period
    # These should be datetime naive in order to compare to the Spark Dataframe
    start_time = datetime(2020, 1, 1, 0, 0, 0)
    end_time = datetime(2020, 1, 1, 1, 0, 0)

    assert aggregated_df.count() == 1
    check_aggregation_row(aggregated_df, 0, default_domain, default_responsible, default_supplier, Decimal(3), start_time, end_time)


def test_hourly_consumption_supplier_aggregator_returns_distinct_rows_for_observations_in_different_hours(time_series_row_factory):
    """
    Aggregator should calculate the correct sum of a "domain"-"responsible"-"supplier" grouping within the
    2 different 1hr time windows
    """
    diff_obs_time = datetime.strptime("2020-01-01T01:00:00+0000", date_time_formatting_string)
    results = {}
    row1_df = time_series_row_factory()
    row2_df = time_series_row_factory(obs_time=diff_obs_time)
    results[ResultKeyName.aggregation_base_dataframe] = row1_df.union(row2_df)
    aggregated_df = aggregate_hourly_consumption(results, metadata).sort(Colname.time_window)

    assert aggregated_df.count() == 2

    # Create the start/end datetimes representing the start and end of the 1 hr time period for each row's ObservationTime
    # These should be datetime naive in order to compare to the Spark Dataframe
    start_time_row1 = datetime(2020, 1, 1, 0, 0, 0)
    end_time_row1 = datetime(2020, 1, 1, 1, 0, 0)
    check_aggregation_row(aggregated_df, 0, default_domain, default_responsible, default_supplier, default_quantity, start_time_row1, end_time_row1)

    start_time_row2 = datetime(2020, 1, 1, 1, 0, 0)
    end_time_row2 = datetime(2020, 1, 1, 2, 0, 0)
    check_aggregation_row(aggregated_df, 1, default_domain, default_responsible, default_supplier, default_quantity, start_time_row2, end_time_row2)


def test_hourly_consumption_supplier_aggregator_returns_correct_schema(time_series_row_factory):
    """
    Aggregator should return the correct schema, including the proper fields for the aggregated quantity values
    and time window (from the single-hour resolution specified in the aggregator).
    """
    results = {}
    results[ResultKeyName.aggregation_base_dataframe] = time_series_row_factory()
    aggregated_df = aggregate_hourly_consumption(results, metadata)
    assert aggregated_df.schema == aggregation_result_schema


def test_hourly_consumption_test_invalid_connection_state(time_series_row_factory):
    results = {}
    results[ResultKeyName.aggregation_base_dataframe] = time_series_row_factory(connection_state=ConnectionState.new.value)
    aggregated_df = aggregate_hourly_consumption(results, metadata)
    assert aggregated_df.count() == 0


def test_hourly_consumption_test_filter_by_domain_is_pressent(time_series_row_factory):
    df = time_series_row_factory()
    aggregated_df = aggregate_per_ga_and_brp_and_es(df, MarketEvaluationPointType.consumption, SettlementMethod.non_profiled, metadata)
    assert aggregated_df.count() == 1


def test_hourly_consumption_test_filter_by_domain_is_not_pressent(time_series_row_factory):
    df = time_series_row_factory()
    aggregated_df = aggregate_per_ga_and_brp_and_es(df, MarketEvaluationPointType.consumption, SettlementMethod.flex_settled, metadata)
    assert aggregated_df.count() == 0


def test_expected_schema(time_series_row_factory):
    results = {}
    results[ResultKeyName.aggregation_base_dataframe] = time_series_row_factory()
    aggregated_df = aggregate_hourly_consumption(results, metadata)
    assert aggregated_df.schema == aggregation_result_schema
