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
from decimal import Decimal

import pandas as pd
import pytest
from pyspark.sql import SparkSession
from pyspark.sql.functions import window, col

from package.calculation.energy.energy_results import (
    EnergyResults,
)
from package.calculation.energy.exchange_aggregators import (
    aggregate_net_exchange_per_ga,
)
from package.calculation.preparation.quarterly_metering_point_time_series import (
    QuarterlyMeteringPointTimeSeries,
)
from package.codelists import (
    MeteringPointType,
    QuantityQuality,
    MeteringPointResolution,
)
from package.constants import Colname

date_time_formatting_string = "%Y-%m-%dT%H:%M:%S%z"
default_obs_time = datetime.strptime(
    "2020-01-01T00:00:00+0000", date_time_formatting_string
)
numberOfQuarters = 5  # Not too many as it has a massive impact on test performance


@pytest.fixture(scope="module")
def enriched_time_series_data_frame(
    spark: SparkSession,
) -> QuarterlyMeteringPointTimeSeries:
    """Sample Time Series DataFrame"""

    # Create empty pandas df
    pandas_df = pd.DataFrame(
        {
            Colname.metering_point_id: [],
            Colname.metering_point_type: [],
            Colname.grid_area: [],
            Colname.to_grid_area: [],
            Colname.from_grid_area: [],
            Colname.quantity: [],
            Colname.observation_time: [],
            Colname.quality: [],
            Colname.resolution: [],
        }
    )

    # add 24 hours of exchange with different examples of exchange between grid areas. See readme.md for more info

    for quarter_number in range(numberOfQuarters):
        pandas_df = add_row_of_data(
            pandas_df,
            MeteringPointType.EXCHANGE.value,
            "B",
            "A",
            Decimal(2) * quarter_number,
            default_obs_time + timedelta(minutes=quarter_number * 15),
        )

        pandas_df = add_row_of_data(
            pandas_df,
            MeteringPointType.EXCHANGE.value,
            "B",
            "A",
            Decimal("0.5") * quarter_number,
            default_obs_time + timedelta(minutes=quarter_number * 15),
        )
        pandas_df = add_row_of_data(
            pandas_df,
            MeteringPointType.EXCHANGE.value,
            "B",
            "A",
            Decimal("0.7") * quarter_number,
            default_obs_time + timedelta(minutes=quarter_number * 15),
        )

        pandas_df = add_row_of_data(
            pandas_df,
            MeteringPointType.EXCHANGE.value,
            "A",
            "B",
            Decimal(3) * quarter_number,
            default_obs_time + timedelta(minutes=quarter_number * 15),
        )
        pandas_df = add_row_of_data(
            pandas_df,
            MeteringPointType.EXCHANGE.value,
            "A",
            "B",
            Decimal("0.9") * quarter_number,
            default_obs_time + timedelta(minutes=quarter_number * 15),
        )
        pandas_df = add_row_of_data(
            pandas_df,
            MeteringPointType.EXCHANGE.value,
            "A",
            "B",
            Decimal("1.2") * quarter_number,
            default_obs_time + timedelta(minutes=quarter_number * 15),
        )

        pandas_df = add_row_of_data(
            pandas_df,
            MeteringPointType.EXCHANGE.value,
            "C",
            "A",
            Decimal("0.7") * quarter_number,
            default_obs_time + timedelta(minutes=quarter_number * 15),
        )
        pandas_df = add_row_of_data(
            pandas_df,
            MeteringPointType.EXCHANGE.value,
            "A",
            "C",
            Decimal("1.1") * quarter_number,
            default_obs_time + timedelta(minutes=quarter_number * 15),
        )
        pandas_df = add_row_of_data(
            pandas_df,
            MeteringPointType.EXCHANGE.value,
            "A",
            "C",
            Decimal("1.5") * quarter_number,
            default_obs_time + timedelta(minutes=quarter_number * 15),
        )
        # "D" only appears as a from-grid-area (case used to prove bug in implementation)
        pandas_df = add_row_of_data(
            pandas_df,
            MeteringPointType.EXCHANGE.value,
            "A",
            "D",
            Decimal("1.6") * quarter_number,
            default_obs_time + timedelta(minutes=quarter_number * 15),
        )
        # "E" only appears as a to-grid-area (case used to prove bug in implementation)
        pandas_df = add_row_of_data(
            pandas_df,
            MeteringPointType.EXCHANGE.value,
            "E",
            "F",
            Decimal("44.4") * quarter_number,
            default_obs_time + timedelta(minutes=quarter_number * 15),
        )
        # Test sign of net exchange. Net exchange should be TO - FROM
        pandas_df = add_row_of_data(
            pandas_df,
            MeteringPointType.EXCHANGE.value,
            "X",
            "Y",
            Decimal("42") * quarter_number,
            default_obs_time + timedelta(minutes=quarter_number * 15),
        )
        pandas_df = add_row_of_data(
            pandas_df,
            MeteringPointType.EXCHANGE.value,
            "Y",
            "X",
            Decimal("12") * quarter_number,
            default_obs_time + timedelta(minutes=quarter_number * 15),
        )

    df = (
        spark.createDataFrame(pandas_df)
        .withColumn(
            Colname.time_window, window(col(Colname.observation_time), "15 minutes")
        )
        .withColumn(Colname.quarter_time, col(Colname.observation_time))
    )
    return QuarterlyMeteringPointTimeSeries(df)


def add_row_of_data(
    pandas_df: pd.DataFrame,
    point_type,
    to_grid_area,
    from_grid_area,
    quantity: Decimal,
    timestamp,
):
    """
    Helper method to create a new row in the dataframe to improve readability and maintainability
    """
    new_row = {
        Colname.metering_point_id: "metering-point-id",
        Colname.metering_point_type: point_type,
        Colname.grid_area: "grid-area",
        Colname.to_grid_area: to_grid_area,
        Colname.from_grid_area: from_grid_area,
        Colname.quantity: quantity,
        Colname.observation_time: timestamp,
        Colname.quality: QuantityQuality.ESTIMATED.value,
        Colname.resolution: MeteringPointResolution.QUARTER.value,
    }
    return pandas_df.append(new_row, ignore_index=True)


@pytest.fixture(scope="module")
def aggregated_data_frame(enriched_time_series_data_frame):
    """Perform aggregation"""
    return aggregate_net_exchange_per_ga(enriched_time_series_data_frame)


def test_test_data_has_correct_row_count(enriched_time_series_data_frame):
    """Check sample data row count"""
    assert enriched_time_series_data_frame.df.count() == (13 * numberOfQuarters)


def test_exchange_has_correct_sign(aggregated_data_frame):
    """Check that the sign of the net exchange is positive for the to-grid-area and negative for the from-grid-area"""
    check_aggregation_row(
        aggregated_data_frame,
        "X",
        Decimal("30"),
        default_obs_time + timedelta(minutes=15),
    )
    check_aggregation_row(
        aggregated_data_frame,
        "Y",
        Decimal("-30"),
        default_obs_time + timedelta(minutes=15),
    )


def test_exchange_aggregator__when_only_outgoing_quantity__returns_correct_aggregations(
    aggregated_data_frame,
):
    check_aggregation_row(
        aggregated_data_frame,
        "D",
        Decimal("-1.6"),
        default_obs_time + timedelta(minutes=15),
    )


def test_exchange_aggregator__when_only_incoming_quantity__returns_correct_aggregations(
    aggregated_data_frame,
):
    check_aggregation_row(
        aggregated_data_frame,
        "E",
        Decimal("44.4"),
        default_obs_time + timedelta(minutes=15),
    )


def test_exchange_aggregator_returns_correct_aggregations(
    aggregated_data_frame,
):
    """Check accuracy of aggregations"""

    for quarter_number in range(numberOfQuarters):
        check_aggregation_row(
            aggregated_data_frame,
            "A",
            Decimal("5.4") * quarter_number,
            default_obs_time + timedelta(minutes=quarter_number * 15),
        )
        check_aggregation_row(
            aggregated_data_frame,
            "B",
            Decimal("-1.9") * quarter_number,
            default_obs_time + timedelta(minutes=quarter_number * 15),
        )
        check_aggregation_row(
            aggregated_data_frame,
            "C",
            Decimal("-1.9") * quarter_number,
            default_obs_time + timedelta(minutes=quarter_number * 15),
        )


def check_aggregation_row(
    df: EnergyResults, grid_area: str, sum_quantity: Decimal, time: datetime
) -> None:
    """Helper function that checks column values for the given row"""
    gridfiltered = df.df.where(df.df[Colname.grid_area] == grid_area).select(
        col(Colname.grid_area),
        col(Colname.sum_quantity),
        col(f"{Colname.time_window_start}").alias("start"),
        col(f"{Colname.time_window_end}").alias("end"),
    )
    res = gridfiltered.filter(gridfiltered["start"] == time).toPandas()
    assert res[Colname.sum_quantity][0] == sum_quantity
