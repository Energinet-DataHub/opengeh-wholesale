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
from pyspark.sql import SparkSession
from pyspark.sql.functions import col, window
from typing import Callable
import pytest
import pandas as pd
from pandas.core.frame import DataFrame as PandasDataFrame

from package.calculation.energy.energy_results import (
    EnergyResults,
)
from package.calculation.preparation.quarterly_metering_point_time_series import (
    QuarterlyMeteringPointTimeSeries,
)
from package.constants import Colname
from package.calculation.energy.aggregators import (
    aggregate_non_profiled_consumption_ga_brp_es,
    _aggregate_per_ga_and_brp_and_es,
)
from package.codelists import (
    MeteringPointType,
    SettlementMethod,
    QuantityQuality,
    MeteringPointResolution,
)

# Default time series data point values
default_point_type = MeteringPointType.CONSUMPTION.value
default_settlement_method = SettlementMethod.NON_PROFILED.value
default_domain = "D1"
default_responsible = "R1"
default_supplier = "S1"
default_quantity = Decimal(1)

date_time_formatting_string = "%Y-%m-%dT%H:%M:%S%z"
default_obs_time = datetime.strptime(
    "2020-01-01T00:00:00+0000", date_time_formatting_string
)


@pytest.fixture(scope="module")
def time_series_row_factory(
    spark: SparkSession,
) -> Callable[..., QuarterlyMeteringPointTimeSeries]:
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
    ) -> QuarterlyMeteringPointTimeSeries:
        pandas_df = pd.DataFrame(
            {
                Colname.metering_point_id: ["metering-point-id"],
                Colname.metering_point_type: [point_type],
                Colname.settlement_method: [settlement_method],
                Colname.grid_area: [domain],
                Colname.balance_responsible_id: [responsible],
                Colname.energy_supplier_id: [supplier],
                Colname.quantity: [quantity],
                Colname.quarter_quantity: [quantity],
                Colname.quality: QuantityQuality.MEASURED.value,
                Colname.time_window: [obs_time],
                Colname.observation_time: [obs_time],
                Colname.resolution: [MeteringPointResolution.QUARTER.value],
            },
        )
        df = (
            spark.createDataFrame(pandas_df)
            .withColumn(
                Colname.time_window, window(col(Colname.time_window), "15 minutes")
            )
            .withColumn(Colname.quarter_time, col(Colname.observation_time))
        )

        return QuarterlyMeteringPointTimeSeries(df)

    return factory


def check_aggregation_row(
    df: EnergyResults,
    row: int,
    grid: str,
    responsible: str,
    supplier: str,
    sum_quantity: Decimal,
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
    pandas_df: PandasDataFrame = df.df.toPandas()
    assert pandas_df[Colname.grid_area][row] == grid
    assert pandas_df[Colname.balance_responsible_id][row] == responsible
    assert pandas_df[Colname.energy_supplier_id][row] == supplier
    assert pandas_df[Colname.sum_quantity][row] == sum_quantity
    assert pandas_df[Colname.time_window][row].start == start
    assert pandas_df[Colname.time_window][row].end == end


def test_consumption_supplier_aggregator_filters_out_incorrect_point_type(
    time_series_row_factory: Callable[..., QuarterlyMeteringPointTimeSeries],
) -> None:
    """
    Aggregator should filter out all non-consumption MarketEvaluationPointType rows
    """
    # Arrange
    time_series = time_series_row_factory(point_type=MeteringPointType.PRODUCTION.value)

    # Act
    aggregated_df = aggregate_non_profiled_consumption_ga_brp_es(time_series)

    # Assert
    assert aggregated_df.df.count() == 0


def test_consumption_supplier_aggregator_aggregates_observations_in_same_hour(
    time_series_row_factory: Callable[..., QuarterlyMeteringPointTimeSeries],
) -> None:
    """
    Aggregator should can calculate the correct sum of a "domain"-"responsible"-"supplier" grouping within the
    same quarter hour time window
    """
    row1_df = time_series_row_factory(quantity=Decimal(1))
    row2_df = time_series_row_factory(quantity=Decimal(2))
    time_series = row1_df.df.union(row2_df.df)
    time_series = QuarterlyMeteringPointTimeSeries(time_series)
    aggregated_df = aggregate_non_profiled_consumption_ga_brp_es(time_series)

    # Create the start/end datetimes representing the start and end of the 1 hr time period
    # These should be datetime naive in order to compare to the Spark Dataframe
    start_time = datetime(2020, 1, 1, 0, 0, 0)
    end_time = datetime(2020, 1, 1, 0, 15, 0)

    assert aggregated_df.df.count() == 1
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


def test_consumption_supplier_aggregator_returns_distinct_rows_for_observations_in_different_hours(
    time_series_row_factory: Callable[..., QuarterlyMeteringPointTimeSeries],
) -> None:
    """
    Aggregator should calculate the correct sum of a "domain"-"responsible"-"supplier" grouping within the
    2 different quarter hour time windows
    """
    # Arrange
    diff_obs_time = datetime.strptime(
        "2020-01-01T01:00:00+0000", date_time_formatting_string
    )
    row1_df = time_series_row_factory()
    row2_df = time_series_row_factory(obs_time=diff_obs_time)
    time_series = row1_df.df.union(row2_df.df)
    time_series = QuarterlyMeteringPointTimeSeries(time_series)

    # Act
    # TODO BJM: Need to sort?
    actual = aggregate_non_profiled_consumption_ga_brp_es(time_series)

    # Assert
    assert actual.df.count() == 2

    # Create the start/end datetimes representing the start and end of the quarter hour time period for each row's ObservationTime
    # These should be datetime naive in order to compare to the Spark Dataframe
    start_time_row1 = datetime(2020, 1, 1, 0, 0, 0)
    end_time_row1 = datetime(2020, 1, 1, 0, 15, 0)
    check_aggregation_row(
        actual,
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
        actual,
        1,
        default_domain,
        default_responsible,
        default_supplier,
        default_quantity,
        start_time_row2,
        end_time_row2,
    )


def test_consumption_test_filter_by_domain_is_pressent(
    time_series_row_factory: Callable[..., QuarterlyMeteringPointTimeSeries],
) -> None:
    df = time_series_row_factory()
    aggregated_df = _aggregate_per_ga_and_brp_and_es(
        df,
        MeteringPointType.CONSUMPTION,
        SettlementMethod.NON_PROFILED,
    )
    assert aggregated_df.df.count() == 1


def test_consumption_test_filter_by_domain_is_not_present(
    time_series_row_factory: Callable[..., QuarterlyMeteringPointTimeSeries],
) -> None:
    df = time_series_row_factory()
    aggregated_df = _aggregate_per_ga_and_brp_and_es(
        df,
        MeteringPointType.CONSUMPTION,
        SettlementMethod.FLEX,
    )
    assert aggregated_df.df.count() == 0
