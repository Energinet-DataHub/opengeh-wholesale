from datetime import datetime, timedelta
from decimal import Decimal

import pytest
from pyspark.sql import SparkSession
from pyspark.sql.functions import col
from pyspark.sql.types import Row

from geh_wholesale.calculation.energy.aggregators.exchange_aggregators import (
    aggregate_exchange,
    aggregate_exchange_per_neighbor,
)
from geh_wholesale.calculation.energy.data_structures.energy_results import (
    EnergyResults,
)
from geh_wholesale.calculation.preparation.data_structures.metering_point_time_series import (
    MeteringPointTimeSeries,
    metering_point_time_series_schema,
)
from geh_wholesale.codelists import MeteringPointType
from geh_wholesale.constants import Colname
from tests.calculation.energy import (
    metering_point_time_series_factories as factories,
)

date_time_formatting_string = "%Y-%m-%dT%H:%M:%S%z"
default_obs_time = datetime.strptime("2020-01-01T00:00:00+0000", date_time_formatting_string)
numberOfQuarters = 5  # Not too many as it has a massive impact on test performance

ALL_GRID_AREAS = ["A", "B", "C", "D", "E", "F", "X", "Y"]


@pytest.fixture(scope="module")
def metering_point_time_series(
    spark: SparkSession,
) -> MeteringPointTimeSeries:
    rows = []

    # add 24 hours of exchange with different examples of exchange between grid areas. See readme.md for more info
    for quarter_number in range(numberOfQuarters):
        obs_time = default_obs_time + timedelta(minutes=quarter_number * 15)

        rows.append(_create_row("B", "A", Decimal(2) * quarter_number, obs_time))
        rows.append(_create_row("B", "A", Decimal("0.5") * quarter_number, obs_time))
        rows.append(_create_row("B", "A", Decimal("0.7") * quarter_number, obs_time))
        rows.append(_create_row("A", "B", Decimal(3) * quarter_number, obs_time))
        rows.append(_create_row("A", "B", Decimal("0.9") * quarter_number, obs_time))
        rows.append(_create_row("A", "B", Decimal("1.2") * quarter_number, obs_time))
        rows.append(_create_row("C", "A", Decimal("0.7") * quarter_number, obs_time))
        rows.append(_create_row("A", "C", Decimal("1.1") * quarter_number, obs_time))
        rows.append(_create_row("A", "C", Decimal("1.5") * quarter_number, obs_time))
        # "D" only appears as a from-grid-area (case used to prove bug in implementation)
        rows.append(_create_row("A", "D", Decimal("1.6") * quarter_number, obs_time))
        # "E" only appears as a to-grid-area (case used to prove bug in implementation)
        rows.append(_create_row("E", "F", Decimal("44.4") * quarter_number, obs_time))
        # Test sign of net exchange. Net exchange should be TO - FROM
        rows.append(_create_row("X", "Y", Decimal("42") * quarter_number, obs_time))
        rows.append(_create_row("Y", "X", Decimal("12") * quarter_number, obs_time))

    df = spark.createDataFrame(data=rows, schema=metering_point_time_series_schema)
    return MeteringPointTimeSeries(df)


def _create_row(
    to_grid_area: str,
    from_grid_area: str,
    quantity: Decimal,
    timestamp: datetime,
) -> Row:
    return factories.create_row(
        to_grid_area=to_grid_area,
        from_grid_area=from_grid_area,
        metering_point_type=MeteringPointType.EXCHANGE,
        quantity=quantity,
        observation_time=timestamp,
    )


@pytest.fixture(scope="module")
def aggregated_data_frame(metering_point_time_series):
    """Perform aggregation"""
    exchange_per_neighbor = aggregate_exchange_per_neighbor(metering_point_time_series, ALL_GRID_AREAS)
    return aggregate_exchange(exchange_per_neighbor)


def test_test_data_has_correct_row_count(metering_point_time_series):
    """Check sample data row count"""
    assert metering_point_time_series.df.count() == (13 * numberOfQuarters)


def test_exchange_has_correct_sign(aggregated_data_frame) -> None:
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
) -> None:
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


def check_aggregation_row(df: EnergyResults, grid_area: str, quantity: Decimal, time: datetime) -> None:
    """Helper function that checks column values for the given row"""
    gridfiltered = df.df.where(df.df[Colname.grid_area_code] == grid_area).select(
        col(Colname.grid_area_code),
        col(Colname.quantity),
        col(Colname.observation_time),
    )
    res = gridfiltered.filter(gridfiltered[Colname.observation_time] == time).collect()
    assert res[0][Colname.quantity] == quantity
