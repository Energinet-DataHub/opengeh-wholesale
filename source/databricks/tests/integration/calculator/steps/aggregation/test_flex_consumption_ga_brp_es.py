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
from package.constants import Colname
from package.steps.aggregation.aggregators import (
    aggregate_flex_consumption_ga_brp_es,
    _aggregate_per_ga_and_brp_and_es,
)
from package.codelists import (
    MeteringPointType,
    SettlementMethod,
    TimeSeriesQuality,
)
from package.shared.data_classes import Metadata
from package.schemas.output import aggregation_result_schema
from pyspark.sql import DataFrame, SparkSession
from pyspark.sql.types import (
    StructType,
    StringType,
    DecimalType,
    TimestampType,
)
from typing import Callable
import pytest
import pandas as pd
from pandas.core.frame import DataFrame as PandasDataFrame


e_20 = MeteringPointType.exchange.value
e_17 = MeteringPointType.consumption.value
e_18 = MeteringPointType.production.value
e_02 = SettlementMethod.non_profiled.value
d_01 = SettlementMethod.flex.value

# Default time series data point values
default_point_type = e_17
default_settlement_method = d_01
default_domain = "D1"
default_responsible = "R1"
default_supplier = "S1"
default_quantity = Decimal(1)
default_resolution = "PT15M"

date_time_formatting_string = "%Y-%m-%dT%H:%M:%S%z"
default_obs_time = datetime.strptime(
    "2020-01-01T00:00:00+0000", date_time_formatting_string
)

metadata = Metadata("1", "1", "1", "1")


@pytest.fixture(scope="module")
def time_series_schema() -> StructType:
    """
    Input time series data point schema
    """
    return (
        StructType()
        .add(Colname.metering_point_type, StringType(), False)
        .add(Colname.settlement_method, StringType())
        .add(Colname.grid_area, StringType(), False)
        .add(Colname.balance_responsible_id, StringType())
        .add(Colname.energy_supplier_id, StringType())
        .add(Colname.quantity, DecimalType())
        .add(Colname.observation_time, TimestampType())
        .add(Colname.quality, StringType())
        .add(Colname.resolution, StringType())
    )


@pytest.fixture(scope="module")
def time_series_row_factory(
    spark: SparkSession, time_series_schema: StringType
) -> Callable[..., DataFrame]:
    """
    Factory to generate a single row of time series data, with default parameters as specified above.
    """

    def factory(
        point_type: str = default_point_type,
        settlement_method: str = default_settlement_method,
        domain: str = default_domain,
        responsible: str = default_responsible,
        supplier: str = default_supplier,
        quantity: Decimal = default_quantity,
        obs_time: datetime = default_obs_time,
        resolution: str = default_resolution,
    ) -> DataFrame:
        pandas_df = pd.DataFrame(
            {
                Colname.metering_point_type: [point_type],
                Colname.settlement_method: [settlement_method],
                Colname.grid_area: [domain],
                Colname.balance_responsible_id: [responsible],
                Colname.energy_supplier_id: [supplier],
                Colname.quantity: [quantity],
                Colname.observation_time: [obs_time],
                Colname.quality: [TimeSeriesQuality.estimated.value],
                Colname.resolution: [resolution],
            }
        )
        return spark.createDataFrame(pandas_df, schema=time_series_schema)

    return factory


def check_aggregation_row(
    df: DataFrame,
    row: int,
    grid: str,
    responsible: str,
    supplier: str,
    sum: Decimal,
    start: datetime,
    end: datetime,
) -> None:
    """
    Helper function that checks column values for the given row.
    Note that start and end datetimes are timezone-naive - we set the Spark session timezone to UTC in the
    conftest.py, since:
        From https://stackoverflow.com/questions/48746376/timestamptype-in-pyspark-with-datetime-tzaware-objects:
        "TimestampType in pyspark is not tz aware like in Pandas rather it passes long ints
        and displays them according to your machine's local time zone (by default)"
    """

    pandas_df: PandasDataFrame = df.toPandas()
    assert pandas_df[Colname.grid_area][row] == grid
    assert pandas_df[Colname.balance_responsible_id][row] == responsible
    assert pandas_df[Colname.energy_supplier_id][row] == supplier
    assert pandas_df[Colname.sum_quantity][row] == sum
    assert pandas_df[Colname.time_window][row].start == start
    assert pandas_df[Colname.time_window][row].end == end


@pytest.mark.parametrize(
    "point_type",
    [
        pytest.param(e_18, id="should filter out E18"),
        pytest.param(e_20, id="should filter out E20"),
    ],
)
def test_filters_out_incorrect_point_type(
    point_type: str, time_series_row_factory: Callable[..., DataFrame]
) -> None:
    """
    Aggregator should filter out all non "E17" MarketEvaluationPointType rows
    """
    df = time_series_row_factory(point_type=point_type)
    aggregated_df = aggregate_flex_consumption_ga_brp_es(df, metadata)
    assert aggregated_df.count() == 0


@pytest.mark.parametrize(
    "settlement_method",
    [
        pytest.param(e_02, id="should filter out E02"),
    ],
)
def test_filters_out_incorrect_settlement_method(
    settlement_method: str, time_series_row_factory: Callable[..., DataFrame]
) -> None:
    """
    Aggregator should filter out all non "D01" SettlementMethod rows
    """
    df = time_series_row_factory(settlement_method=settlement_method)
    aggregated_df = aggregate_flex_consumption_ga_brp_es(df, metadata)
    assert aggregated_df.count() == 0


def test_aggregates_observations_in_same_hour(
    time_series_row_factory: Callable[..., DataFrame],
) -> None:
    """
    Aggregator should can calculate the correct sum of a "domain"-"responsible"-"supplier" grouping within the
    same quarter hour time window
    """
    row1_df = time_series_row_factory(quantity=Decimal(1))
    row2_df = time_series_row_factory(quantity=Decimal(2))
    df = row1_df.union(row2_df)
    aggregated_df = aggregate_flex_consumption_ga_brp_es(df, metadata)

    # Create the start/end datetimes representing the start and end of the 1 hr time period
    # These should be datetime naive in order to compare to the Spark Dataframe
    start_time = datetime(2020, 1, 1, 0, 0, 0)
    end_time = datetime(2020, 1, 1, 0, 15, 0)

    assert aggregated_df.count() == 1
    check_aggregation_row(
        aggregated_df,
        0,
        default_domain,
        default_responsible,
        default_supplier,
        Decimal(3),
        start_time,
        end_time,
    )


def test_returns_distinct_rows_for_observations_in_different_hours(
    time_series_row_factory: Callable[..., DataFrame],
) -> None:
    """
    Aggregator should can calculate the correct sum of a "domain"-"responsible"-"supplier" grouping within the
    2 different quarter hour time windows
    """
    diff_obs_time = datetime.strptime(
        "2020-01-01T01:00:00+0000", date_time_formatting_string
    )
    row1_df = time_series_row_factory()
    row2_df = time_series_row_factory(obs_time=diff_obs_time)
    df = row1_df.union(row2_df)
    aggregated_df = aggregate_flex_consumption_ga_brp_es(df, metadata).sort(
        Colname.time_window
    )

    assert aggregated_df.count() == 2

    # Create the start/end datetimes representing the start and end of the quarter hour time period for each row's ObservationTime
    # These should be datetime naive in order to compare to the Spark Dataframe
    start_time_row1 = datetime(2020, 1, 1, 0, 0, 0)
    end_time_row1 = datetime(2020, 1, 1, 0, 15, 0)
    check_aggregation_row(
        aggregated_df,
        0,
        default_domain,
        default_responsible,
        default_supplier,
        default_quantity,
        start_time_row1,
        end_time_row1,
    )

    start_time_row2 = datetime(2020, 1, 1, 1, 0, 0)
    end_time_row2 = datetime(2020, 1, 1, 1, 15, 0)
    check_aggregation_row(
        aggregated_df,
        1,
        default_domain,
        default_responsible,
        default_supplier,
        default_quantity,
        start_time_row2,
        end_time_row2,
    )


def test_returns_correct_schema(
    time_series_row_factory: Callable[..., DataFrame]
) -> None:
    """
    Aggregator should return the correct schema, including the proper fields for the aggregated quantity values
    and time window (from the quarter-hour resolution specified in the aggregator).
    """
    df = time_series_row_factory()
    aggregated_df = aggregate_flex_consumption_ga_brp_es(df, metadata)
    assert aggregated_df.schema == aggregation_result_schema


def test_flex_consumption_test_filter_by_domain_is_present(
    time_series_row_factory: Callable[..., DataFrame],
) -> None:
    df = time_series_row_factory()
    aggregated_df = _aggregate_per_ga_and_brp_and_es(
        df,
        MeteringPointType.consumption,
        SettlementMethod.flex,
        metadata,
    )
    assert aggregated_df.count() == 1


def test_flex_consumption_test_filter_by_domain_is_not_present(
    time_series_row_factory: Callable[..., DataFrame]
) -> None:
    df = time_series_row_factory()
    aggregated_df = _aggregate_per_ga_and_brp_and_es(
        df,
        MeteringPointType.consumption,
        SettlementMethod.non_profiled,
        metadata,
    )
    assert aggregated_df.count() == 0
