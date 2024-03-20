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

from pyspark.sql import DataFrame, SparkSession
import pyspark.sql.functions as F
import pytest

from . import configuration as C
from package.codelists import (
    AggregationLevel,
    ChargeType,
    TimeSeriesType,
    WholesaleResultResolution,
)
from package.constants import EnergyResultColumnNames, WholesaleResultColumnNames


ENERGY_RESULT_TYPES = {
    (
        TimeSeriesType.NET_EXCHANGE_PER_GA.value,
        AggregationLevel.TOTAL_GA.value,
    ),
    (
        TimeSeriesType.PRODUCTION.value,
        AggregationLevel.ES_PER_GA.value,
    ),
    (
        TimeSeriesType.PRODUCTION.value,
        AggregationLevel.TOTAL_GA.value,
    ),
    (
        TimeSeriesType.NON_PROFILED_CONSUMPTION.value,
        AggregationLevel.ES_PER_GA.value,
    ),
    (
        TimeSeriesType.NON_PROFILED_CONSUMPTION.value,
        AggregationLevel.TOTAL_GA.value,
    ),
    (
        TimeSeriesType.FLEX_CONSUMPTION.value,
        AggregationLevel.ES_PER_GA.value,
    ),
    (
        TimeSeriesType.FLEX_CONSUMPTION.value,
        AggregationLevel.TOTAL_GA.value,
    ),
    (
        TimeSeriesType.GRID_LOSS.value,
        AggregationLevel.TOTAL_GA.value,
    ),
    (
        TimeSeriesType.POSITIVE_GRID_LOSS.value,
        AggregationLevel.TOTAL_GA.value,
    ),
    (
        TimeSeriesType.NEGATIVE_GRID_LOSS.value,
        AggregationLevel.TOTAL_GA.value,
    ),
    (
        TimeSeriesType.TOTAL_CONSUMPTION.value,
        AggregationLevel.TOTAL_GA.value,
    ),
    (
        TimeSeriesType.TEMP_FLEX_CONSUMPTION.value,
        AggregationLevel.TOTAL_GA.value,
    ),
    (
        TimeSeriesType.TEMP_PRODUCTION.value,
        AggregationLevel.TOTAL_GA.value,
    ),
}


@pytest.mark.parametrize(
    "time_series_type, aggregation_level",
    ENERGY_RESULT_TYPES,
)
def test__energy_result__is_created(
    wholesale_fixing_energy_results_df: DataFrame,
    time_series_type: str,
    aggregation_level: str,
) -> None:
    # Arrange
    result_df = (
        wholesale_fixing_energy_results_df.where(
            F.col(EnergyResultColumnNames.calculation_id)
            == C.executed_wholesale_calculation_id
        )
        .where(F.col(EnergyResultColumnNames.time_series_type) == time_series_type)
        .where(F.col(EnergyResultColumnNames.aggregation_level) == aggregation_level)
    )

    # Act: Calculator job is executed just once per session.
    #      See the fixtures `results_df` and `executed_wholesale_fixing`

    # Assert: The result is created if there are rows
    assert result_df.count() > 0


def test__energy_result__has_expected_number_of_types(
    wholesale_fixing_energy_results_df: DataFrame,
) -> None:
    # Arrange
    actual_result_type_count = (
        wholesale_fixing_energy_results_df.where(
            F.col(EnergyResultColumnNames.calculation_id)
            == C.executed_wholesale_calculation_id
        )
        .where(
            F.col(EnergyResultColumnNames.calculation_id)
            == C.executed_wholesale_calculation_id
        )
        .select(
            EnergyResultColumnNames.time_series_type,
            EnergyResultColumnNames.aggregation_level,
        )
        .distinct()
        .count()
    )

    # Act: Calculator job is executed just once per session.
    #      See the fixtures `results_df` and `executed_wholesale_fixing`

    # Assert: The result is created if there are rows
    assert actual_result_type_count == len(ENERGY_RESULT_TYPES)


WHOLESALE_RESULT_TYPES = [
    (ChargeType.TARIFF, WholesaleResultResolution.HOUR),
    (ChargeType.TARIFF, WholesaleResultResolution.DAY),
    (ChargeType.SUBSCRIPTION, WholesaleResultResolution.DAY),
    (ChargeType.FEE, WholesaleResultResolution.DAY),
]


@pytest.mark.parametrize(
    "charge_type, resolution",
    WHOLESALE_RESULT_TYPES,
)
def test__wholesale_result__amount_per_charge_is_created(
    wholesale_fixing_wholesale_results_df: DataFrame,
    charge_type: ChargeType,
    resolution: WholesaleResultResolution,
) -> None:
    # Arrange
    result_df = (
        wholesale_fixing_wholesale_results_df.where(
            F.col(WholesaleResultColumnNames.calculation_id)
            == C.executed_wholesale_calculation_id
        )
        .where(F.col(WholesaleResultColumnNames.charge_type) == charge_type.value)
        .where(F.col(WholesaleResultColumnNames.resolution) == resolution.value)
    )

    # Act: Calculator job is executed just once per session.
    #      See the fixtures `results_df` and `executed_wholesale_fixing`

    # Assert: The result is created if there are rows
    assert result_df.count() > 0


@pytest.mark.parametrize(
    "charge_code",
    ["40000", "41000"],
    # charge_code 40000 is for hourly charge resolution
    # charge_code 41000 is for daily charge resolution
    # see "test_files/ChargeMasterDataPeriods.csv"
)
def test__monthly_amount_for_tariffs__is_created(
    spark: SparkSession,
    wholesale_fixing_wholesale_results_df: DataFrame,
    charge_code: str,
) -> None:
    # Arrange

    result_df = (
        wholesale_fixing_wholesale_results_df.where(
            F.col(WholesaleResultColumnNames.charge_type) == ChargeType.TARIFF.value
        )
        .where(
            F.col(WholesaleResultColumnNames.resolution)
            == WholesaleResultResolution.MONTH.value
        )
        .where(F.col(WholesaleResultColumnNames.charge_code) == charge_code)
    )

    # Act: Calculator job is executed just once per session.
    #      See the fixtures `results_df` and `executed_wholesale_fixing`

    # Assert: The result is created if there are rows
    assert result_df.count() > 0


@pytest.mark.parametrize("charge_type", [ChargeType.SUBSCRIPTION, ChargeType.FEE])
def test__monthly_amount_for_subscriptions_and_fees__is_created(
    spark: SparkSession,
    wholesale_fixing_wholesale_results_df: DataFrame,
    charge_type: ChargeType,
) -> None:
    # Arrange

    result_df = wholesale_fixing_wholesale_results_df.where(
        F.col(WholesaleResultColumnNames.charge_type) == charge_type.value
    ).where(
        F.col(WholesaleResultColumnNames.resolution)
        == WholesaleResultResolution.DAY.value
    )

    # Act: Calculator job is executed just once per session.
    #      See the fixtures `results_df` and `executed_wholesale_fixing`

    # Assert: The result is created if there are rows
    assert result_df.count() > 0
