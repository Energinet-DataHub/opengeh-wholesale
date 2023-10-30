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

from datetime import datetime
from decimal import Decimal
from typing import Callable, Optional

import pyspark.sql.functions as F
import pytest
from pandas.core.frame import DataFrame as PandasDataFrame
from pyspark.sql import SparkSession

from package.calculation.energy.aggregators import (
    aggregate_production_ga_brp_es,
    _aggregate_per_ga_and_brp_and_es,
)
from package.calculation.energy.energy_results import (
    EnergyResults,
)
from package.calculation.preparation.quarterly_metering_point_time_series import (
    QuarterlyMeteringPointTimeSeries,
)
from package.codelists import (
    MeteringPointType,
    QuantityQuality,
    SettlementMethod,
)
from package.constants import Colname

minimum_quantity = Decimal("0.001")
grid_area_code_805 = "805"
grid_area_code_806 = "806"

# Default time series data point values
default_metering_point_type = MeteringPointType.PRODUCTION.value
default_grid_area = grid_area_code_805
default_responsible = "R1"
default_supplier = "S1"
default_quantity = Decimal(1)
default_quality = QuantityQuality.MEASURED.value
default_obs_time_string = "2020-01-01T00:00:00.000Z"


@pytest.fixture
def enriched_time_series_factory(
    spark: SparkSession, timestamp_factory: Callable[[str], Optional[datetime]]
) -> Callable[..., QuarterlyMeteringPointTimeSeries]:
    def factory(
        quantity: Decimal = default_quantity,
        quality: str = default_quality,
        grid_area: str = default_grid_area,
        obs_time_string: str = default_obs_time_string,
        metering_point_type: str = default_metering_point_type,
    ) -> QuarterlyMeteringPointTimeSeries:
        obs_time_datetime = timestamp_factory(obs_time_string)
        rows = [
            {
                Colname.metering_point_id: "metering-point-id",
                Colname.metering_point_type: metering_point_type,
                Colname.grid_area: grid_area,
                Colname.balance_responsible_id: default_responsible,
                Colname.energy_supplier_id: default_supplier,
                Colname.quantity: quantity,
                Colname.observation_time: obs_time_datetime,
                Colname.time_window: obs_time_datetime,
                Colname.quarter_time: obs_time_datetime,
                Colname.quality: quality,
                Colname.settlement_method: SettlementMethod.NON_PROFILED.value,
            }
        ]
        df = spark.createDataFrame(rows).withColumn(
            Colname.time_window, F.window(F.col(Colname.time_window), "15 minutes")
        )
        return QuarterlyMeteringPointTimeSeries(df)

    return factory


@pytest.fixture
def enriched_time_series_quarterly_same_time_factory(
    enriched_time_series_factory: Callable[..., QuarterlyMeteringPointTimeSeries],
) -> Callable[..., QuarterlyMeteringPointTimeSeries]:
    def factory(
        first_quantity: Decimal = Decimal("1"),
        second_quantity: Decimal = Decimal("2"),
        first_obs_time_string: str = default_obs_time_string,
        second_obs_time_string: str = default_obs_time_string,
        first_grid_area_code: str = grid_area_code_805,
        second_grid_area_code: str = grid_area_code_805,
    ) -> QuarterlyMeteringPointTimeSeries:
        df = enriched_time_series_factory(
            quantity=first_quantity,
            obs_time_string=first_obs_time_string,
            grid_area=first_grid_area_code,
        ).df.union(
            enriched_time_series_factory(
                quantity=second_quantity,
                obs_time_string=second_obs_time_string,
                grid_area=second_grid_area_code,
            ).df
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


@pytest.mark.parametrize(
    "metering_point_type",
    [
        pytest.param(
            MeteringPointType.CONSUMPTION.value,
            id="invalid because metering point type is consumption",
        ),
        pytest.param(
            MeteringPointType.EXCHANGE.value,
            id="invalid because metering point type is exchange",
        ),
    ],
)
def test_production_aggregator_filters_out_incorrect_point_type(
    metering_point_type: str,
    enriched_time_series_factory: Callable[..., QuarterlyMeteringPointTimeSeries],
) -> None:
    """
    Aggregator should filter out all non-production MarketEvaluationPointType rows
    """
    enriched_time_series = enriched_time_series_factory(
        metering_point_type=metering_point_type
    )
    aggregated_df = aggregate_production_ga_brp_es(enriched_time_series)
    assert aggregated_df.df.count() == 0


def test_production_aggregator_aggregates_observations_in_same_quarter_hour(
    enriched_time_series_factory: Callable[..., QuarterlyMeteringPointTimeSeries],
) -> None:
    """
    Aggregator should calculate the correct sum of a "grid area" grouping within the
    same quarter hour time window
    """
    row1_df = enriched_time_series_factory(quantity=Decimal(1))
    row2_df = enriched_time_series_factory(quantity=Decimal(2))

    enriched_time_series = QuarterlyMeteringPointTimeSeries(
        row1_df.df.union(row2_df.df)
    )
    aggregated_df = aggregate_production_ga_brp_es(enriched_time_series)

    # Create the start/end datetimes representing the start and end of the 1 hr time period
    # These should be datetime naive in order to compare to the Spark Dataframe
    start_time = datetime(2020, 1, 1, 0, 0, 0)
    end_time = datetime(2020, 1, 1, 0, 15, 0)

    assert aggregated_df.df.count() == 1
    check_aggregation_row(
        aggregated_df,
        0,
        default_grid_area,
        default_responsible,
        default_supplier,
        Decimal(3),
        start_time,
        end_time,
    )


def test_production_aggregator_returns_distinct_rows_for_observations_in_different_hours(
    enriched_time_series_factory: Callable[..., QuarterlyMeteringPointTimeSeries],
) -> None:
    """
    Aggregator can calculate the correct sum of a "grid area"-"responsible"-"supplier" grouping
    within the 2 different quarter hour time windows
    """
    row1_df = enriched_time_series_factory()
    row2_df = enriched_time_series_factory(obs_time_string="2020-01-01T01:00:00.000Z")
    time_series = QuarterlyMeteringPointTimeSeries(row1_df.df.union(row2_df.df))
    aggregated_df = aggregate_production_ga_brp_es(time_series)

    assert aggregated_df.df.count() == 2

    # Create the start/end datetimes representing the start and end of the quarter hour time period for each row's ObservationTime
    # These should be datetime naive in order to compare to the Spark Dataframe
    start_time_row1 = datetime(2020, 1, 1, 0, 0, 0)
    end_time_row1 = datetime(2020, 1, 1, 0, 15, 0)
    check_aggregation_row(
        aggregated_df,
        0,
        default_grid_area,
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
        default_grid_area,
        default_responsible,
        default_supplier,
        default_quantity,
        start_time_row2,
        end_time_row2,
    )


def test_production_test_filter_by_domain_is_present(
    enriched_time_series_factory: Callable[..., QuarterlyMeteringPointTimeSeries],
) -> None:
    df = enriched_time_series_factory()
    aggregated_df = _aggregate_per_ga_and_brp_and_es(
        df, MeteringPointType.PRODUCTION, None
    )
    assert aggregated_df.df.count() == 1


# Test sums with only quarterly can be calculated
def test__quarterly_sums_correctly(
    enriched_time_series_quarterly_same_time_factory: Callable[
        ..., QuarterlyMeteringPointTimeSeries
    ],
) -> None:
    """Test that checks quantity is summed correctly with only quarterly times"""
    df = enriched_time_series_quarterly_same_time_factory(
        first_quantity=Decimal("1"), second_quantity=Decimal("2")
    )
    result_df = _aggregate_per_ga_and_brp_and_es(df, MeteringPointType.PRODUCTION, None)
    assert result_df.df.first().sum_quantity == 3


# TODO: Turn into test of get_enriched_time_series function
# @pytest.mark.parametrize(
#     "quantity, expected_point_quantity",
#     [
#         # 0.001 / 4 = 0.000250 ≈ 0.000
#         (Decimal("0.001"), Decimal("0.000")),
#         # 0.002 / 4 = 0.000500 ≈ 0.001
#         (Decimal("0.002"), Decimal("0.001")),
#         # 0.003 / 4 = 0.000750 ≈ 0.001
#         (Decimal("0.003"), Decimal("0.001")),
#         # 0.004 / 4 = 0.001000 ≈ 0.001
#         (Decimal("0.004"), Decimal("0.001")),
#         # 0.005 / 4 = 0.001250 ≈ 0.001
#         (Decimal("0.005"), Decimal("0.001")),
#         # 0.006 / 4 = 0.001500 ≈ 0.002
#         (Decimal("0.006"), Decimal("0.002")),
#         # 0.007 / 4 = 0.001750 ≈ 0.002
#         (Decimal("0.007"), Decimal("0.002")),
#         # 0.008 / 4 = 0.002000 ≈ 0.002
#         (Decimal("0.008"), Decimal("0.002")),
#     ],
# )
# def test__hourly_sums_are_rounded_correctly(
#     enriched_time_series_factory: Callable[..., QuarterlyMeteringPointTimeSeries],
#     quantity: Decimal,
#     expected_point_quantity: Decimal,
# ) -> None:
#     """Test that checks acceptable rounding erros for hourly quantities summed on a quarterly basis"""
#     df = enriched_time_series_factory(
#         resolution=MeteringPointResolution.hour.value, quantity=quantity
#     )

#     result_df = _aggregate_per_ga_and_brp_and_es(df, MeteringPointType.production, None)

#     assert result_df.count() == 4  # one hourly quantity should yield 4 points
#     assert (
#         result_df.where(col(Colname.sum_quantity) == expected_point_quantity).count()
#         == 4
#     )


# TODO: Turn into test of get_enriched_time_series function
# def test__quarterly_and_hourly_sums_correctly(
#     enriched_time_series_quarterly_same_time_factory: Callable[..., DataFrame],
# ) -> None:
#     """Test that checks quantity is summed correctly with quarterly and hourly times"""
#     first_quantity = Decimal("4")
#     second_quantity = Decimal("2")
#     df = enriched_time_series_quarterly_same_time_factory(
#         first_resolution=MeteringPointResolution.quarter.value,
#         first_quantity=first_quantity,
#         second_resolution=MeteringPointResolution.hour.value,
#         second_quantity=second_quantity,
#     )
#     result_df = _aggregate_per_ga_and_brp_and_es(df, MeteringPointType.production, None)
#     sum_quant = result_df.agg(sum(Colname.sum_quantity).alias("sum_quant"))
#     assert sum_quant.first()["sum_quant"] == first_quantity + second_quantity


# TODO: Turn into test of get_enriched_time_series function
# def test__points_with_same_time_quantities_are_on_same_position(
#     enriched_time_series_quarterly_same_time_factory: Callable[..., DataFrame],
# ) -> None:
#     """Test that points with the same 'time' have added their 'Quantity's together on the same position"""
#     df = enriched_time_series_quarterly_same_time_factory(
#         first_resolution=MeteringPointResolution.quarter.value,
#         first_quantity=Decimal("2"),
#         second_resolution=MeteringPointResolution.hour.value,
#         second_quantity=Decimal("2"),
#     )
#     result_df = _aggregate_per_ga_and_brp_and_es(df, MeteringPointType.production, None)
#     # total 'Quantity' on first position
#     assert result_df.first().sum_quantity == Decimal("2.5")
#     # first point with quarter resolution 'quantity' is 2, second is 2 but is hourly so 0.5 should be added to first position


# TODO: Turn into test of get_enriched_time_series function
# def test__that_hourly_quantity_is_summed_as_quarterly(
#     enriched_time_series_quarterly_same_time_factory: Callable[..., DataFrame],
# ) -> None:
#     "Test that checks if hourly quantities are summed as quarterly"
#     df = enriched_time_series_quarterly_same_time_factory(
#         first_resolution=MeteringPointResolution.hour.value,
#         first_quantity=Decimal("4"),
#         second_resolution=MeteringPointResolution.hour.value,
#         second_quantity=Decimal("8"),
#         first_obs_time_string="2022-06-08T12:09:15.000Z",
#         second_obs_time_string="2022-06-08T13:09:15.000Z",
#     )
#     result_df = _aggregate_per_ga_and_brp_and_es(df, MeteringPointType.production, None)
#     assert result_df.count() == 8
#     actual = result_df.collect()
#     assert actual[0].sum_quantity == Decimal("1")
#     assert actual[4].sum_quantity == Decimal("2")


def test__that_grid_area_code_in_input_is_in_output(
    enriched_time_series_quarterly_same_time_factory: Callable[
        ..., QuarterlyMeteringPointTimeSeries
    ],
) -> None:
    """Test that the grid area codes in input are in result"""
    df = enriched_time_series_quarterly_same_time_factory()
    result_df = _aggregate_per_ga_and_brp_and_es(df, MeteringPointType.PRODUCTION, None)
    actual_row = result_df.df.first()
    assert getattr(actual_row, Colname.grid_area) == str(grid_area_code_805)


def test__each_grid_area_has_a_sum(
    enriched_time_series_quarterly_same_time_factory: Callable[
        ..., QuarterlyMeteringPointTimeSeries
    ],
) -> None:
    """Test that multiple GridAreas receive each their calculation for a period"""
    df = enriched_time_series_quarterly_same_time_factory(second_grid_area_code="806")
    result_df = _aggregate_per_ga_and_brp_and_es(df, MeteringPointType.PRODUCTION, None)
    assert result_df.df.where(F.col(Colname.grid_area) == "805").count() == 1
    assert result_df.df.where(F.col(Colname.grid_area) == "806").count() == 1


# TODO: Turn into test of get_enriched_time_series function
# def test__final_sum_of_different_magnitudes_should_not_lose_precision(
#     enriched_time_series_factory: Callable[..., QuarterlyMeteringPointTimeSeries],
# ) -> None:
#     """Test that values with different magnitudes do not lose precision when accumulated"""
#     df = (
#         enriched_time_series_factory(
#             MeteringPointResolution.hour.value, Decimal("400000000000")
#         )
#         .union(
#             enriched_time_series_factory(
#                 MeteringPointResolution.hour.value, minimum_quantity
#             )
#         )
#         .union(
#             enriched_time_series_factory(
#                 MeteringPointResolution.hour.value, minimum_quantity
#             )
#         )
#         .union(
#             enriched_time_series_factory(
#                 MeteringPointResolution.hour.value, minimum_quantity
#             )
#         )
#     )
#     result_df = _aggregate_per_ga_and_brp_and_es(df, MeteringPointType.production, None)
#     assert result_df.count() == 4
#     assert result_df.where(col(Colname.sum_quantity) == "100000000000.001").count() == 4


# TODO BJM
# @pytest.mark.parametrize(
#     "quality_1, quality_2, quality_3, expected_quality",
#     [
#         (
#             QuantityQuality.MEASURED.value,
#             QuantityQuality.ESTIMATED.value,
#             QuantityQuality.MISSING.value,
#             QuantityQuality.INCOMPLETE.value,
#         ),
#         (
#             QuantityQuality.MEASURED.value,
#             QuantityQuality.ESTIMATED.value,
#             QuantityQuality.INCOMPLETE.value,
#             QuantityQuality.INCOMPLETE.value,
#         ),
#         (
#             QuantityQuality.MEASURED.value,
#             QuantityQuality.ESTIMATED.value,
#             QuantityQuality.MEASURED.value,
#             QuantityQuality.ESTIMATED.value,
#         ),
#         (
#             QuantityQuality.MEASURED.value,
#             QuantityQuality.MEASURED.value,
#             QuantityQuality.MEASURED.value,
#             QuantityQuality.MEASURED.value,
#         ),
#     ],
# )
# def test__quality_is_lowest_common_denominator_among_measured_estimated_and_missing(
#     enriched_time_series_factory: Callable[..., QuarterlyMeteringPointTimeSeries],
#     quality_1: str,
#     quality_2: str,
#     quality_3: str,
#     expected_quality: str,
# ) -> None:
#     df = (
#         enriched_time_series_factory(quality=quality_1)
#         .union(enriched_time_series_factory(quality=quality_2))
#         .union(enriched_time_series_factory(quality=quality_3))
#     )
#     result_df = _aggregate_per_ga_and_brp_and_es(df, MeteringPointType.PRODUCTION, None)
#     assert result_df.first().quality == expected_quality


# TODO BJM
# def test__when_time_series_point_is_missing__quality_has_value_incomplete(
#     enriched_time_series_factory: Callable[..., QuarterlyMeteringPointTimeSeries],
# ) -> None:
#     df = enriched_time_series_factory().withColumn(Colname.quality, F.lit(None))
#
#     result_df = _aggregate_per_ga_and_brp_and_es(df, MeteringPointType.PRODUCTION, None)
#     assert result_df.first().quality == QuantityQuality.MISSING.value
