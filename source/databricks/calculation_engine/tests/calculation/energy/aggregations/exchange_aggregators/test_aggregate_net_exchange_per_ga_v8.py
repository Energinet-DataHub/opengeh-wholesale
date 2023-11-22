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
from pyspark.sql.functions import col

from package.calculation.energy.energy_results import (
    EnergyResults,
)
from package.calculation.energy.aggregators.exchange_aggregators import (
    aggregate_net_exchange_per_ga,
)
from package.calculation.preparation.quarterly_metering_point_time_series import (
    QuarterlyMeteringPointTimeSeries,
    _quarterly_metering_point_time_series_schema,
)
from package.codelists import (
    MeteringPointType,
    QuantityQuality,
    SettlementMethod,
)
from package.constants import Colname

date_time_formatting_string = "%Y-%m-%dT%H:%M:%S%z"
default_obs_time = datetime.strptime(
    "2020-01-01T00:00:00+0000", date_time_formatting_string
)
numberOfQuarters = 5  # Not too many as it has a massive impact on test performance


@pytest.fixture(scope="module")
def quarterly_metering_point_time_series(
    spark: SparkSession,
) -> QuarterlyMeteringPointTimeSeries:
    """Sample Time Series DataFrame"""

    # Create empty pandas df
    pandas_df = pd.DataFrame(
        {
            Colname.grid_area: [],
            Colname.to_grid_area: [],
            Colname.from_grid_area: [],
            Colname.metering_point_id: [],
            Colname.metering_point_type: [],
            Colname.quantity: [],
            Colname.quality: [],
            Colname.energy_supplier_id: [],
            Colname.balance_responsible_id: [],
            Colname.settlement_method: [],
            Colname.time_window: [],
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

    df = spark.createDataFrame(pandas_df, _quarterly_metering_point_time_series_schema)

    return QuarterlyMeteringPointTimeSeries(df)


def add_row_of_data(
    pandas_df: pd.DataFrame,
    point_type,
    to_grid_area,
    from_grid_area,
    quantity: Decimal,
    timestamp: datetime,
):
    """
    Helper method to create a new row in the dataframe to improve readability and maintainability
    """
    new_row = {
        Colname.grid_area: "grid-area",
        Colname.to_grid_area: to_grid_area,
        Colname.from_grid_area: from_grid_area,
        Colname.metering_point_id: "metering-point-id",
        Colname.metering_point_type: point_type,
        Colname.quantity: quantity,
        Colname.quality: QuantityQuality.ESTIMATED.value,
        Colname.energy_supplier_id: "energy-supplier-id",
        Colname.balance_responsible_id: "balance-responsible-id",
        Colname.settlement_method: SettlementMethod.NON_PROFILED.value,
        Colname.time_window: [timestamp, timestamp + timedelta(minutes=15)],
    }
    return pandas_df.append(new_row, ignore_index=True)


@pytest.fixture(scope="module")
def aggregated_data_frame(quarterly_metering_point_time_series):
    """Perform aggregation"""
    return aggregate_net_exchange_per_ga(quarterly_metering_point_time_series, [])


def test_test_data_has_correct_row_count(quarterly_metering_point_time_series):
    """Check sample data row count"""
    assert quarterly_metering_point_time_series.df.count() == (13 * numberOfQuarters)


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
