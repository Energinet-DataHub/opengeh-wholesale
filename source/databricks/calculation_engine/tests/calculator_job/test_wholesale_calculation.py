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

import pyspark.sql.functions as f
import pytest
from pyspark.sql import DataFrame, SparkSession
from pyspark.sql.types import StructType

from package.calculation.basis_data.schemas.charge_link_periods_schema import (
    charge_link_periods_schema,
)
from package.calculation.basis_data.schemas.charge_master_data_periods_schema import (
    charge_price_information_periods_schema,
)
from package.calculation.basis_data.schemas.charge_price_points_schema import (
    charge_price_points_schema,
)
from package.calculation.basis_data.schemas.grid_loss_metering_points_schema import (
    grid_loss_metering_points_schema,
)
from package.calculation.basis_data.schemas.metering_point_period_schema import (
    metering_point_period_schema,
)
from package.calculation.basis_data.schemas.time_series_point_schema import (
    time_series_point_schema,
)
from package.codelists import (
    AggregationLevel,
    ChargeType,
    TimeSeriesType,
    WholesaleResultResolution,
)
from package.constants import EnergyResultColumnNames, WholesaleResultColumnNames
from package.infrastructure import paths
from . import configuration as c

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
            f.col(EnergyResultColumnNames.calculation_id)
            == c.executed_wholesale_calculation_id
        )
        .where(f.col(EnergyResultColumnNames.time_series_type) == time_series_type)
        .where(f.col(EnergyResultColumnNames.aggregation_level) == aggregation_level)
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
            f.col(EnergyResultColumnNames.calculation_id)
            == c.executed_wholesale_calculation_id
        )
        .where(
            f.col(EnergyResultColumnNames.calculation_id)
            == c.executed_wholesale_calculation_id
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
            f.col(WholesaleResultColumnNames.calculation_id)
            == c.executed_wholesale_calculation_id
        )
        .where(f.col(WholesaleResultColumnNames.charge_type) == charge_type.value)
        .where(f.col(WholesaleResultColumnNames.resolution) == resolution.value)
    )

    # Act: Calculator job is executed just once per session.
    #      See the fixtures `results_df` and `executed_wholesale_fixing`
    # AJW Testing

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
            f.col(WholesaleResultColumnNames.charge_type) == ChargeType.TARIFF.value
        )
        .where(
            f.col(WholesaleResultColumnNames.resolution)
            == WholesaleResultResolution.MONTH.value
        )
        .where(f.col(WholesaleResultColumnNames.charge_code) == charge_code)
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
        f.col(WholesaleResultColumnNames.charge_type) == charge_type.value
    ).where(
        f.col(WholesaleResultColumnNames.resolution)
        == WholesaleResultResolution.MONTH.value
    )

    # Act: Calculator job is executed just once per session.
    #      See the fixtures `results_df` and `executed_wholesale_fixing`

    # Assert: The result is created if there are rows
    assert result_df.count() > 0


def test__total_monthly_amounts__are_stored(
    spark: SparkSession,
    wholesale_fixing_total_monthly_amounts: DataFrame,
) -> None:
    # Arrange

    # Act: Calculator job is executed just once per session.
    #      See the fixtures `results_df` and `executed_wholesale_fixing`

    # Assert: The result is created if there are rows
    assert wholesale_fixing_total_monthly_amounts.count() > 0


def test__monthly_amounts__are_stored(
    spark: SparkSession,
    wholesale_fixing_monthly_amounts: DataFrame,
) -> None:
    # Arrange

    # Act: Calculator job is executed just once per session.
    #      See the fixtures `results_df` and `executed_wholesale_fixing`

    # Assert: The result is created if there are rows
    assert wholesale_fixing_monthly_amounts.count() > 0


@pytest.mark.parametrize(
    "basis_data_table_name",
    paths.BASIS_DATA_TABLE_NAMES,
)
def test__when_wholesale_calculation__basis_data_is_stored(
    spark: SparkSession,
    executed_wholesale_fixing: None,
    basis_data_table_name: str,
) -> None:
    # Arrange
    actual = spark.read.table(
        f"{paths.BASIS_DATA_DATABASE_NAME}.{basis_data_table_name}"
    ).where(f.col("calculation_id") == c.executed_wholesale_calculation_id)

    # Act: Calculator job is executed just once per session.
    #      See the fixtures `results_df` and `executed_wholesale_fixing`

    # Assert
    assert actual.count() > 0


@pytest.mark.parametrize(
    "basis_data_table_name, expected_schema",
    [
        (
            paths.METERING_POINT_PERIODS_BASIS_DATA_TABLE_NAME,
            metering_point_period_schema,
        ),
        (
            paths.TIME_SERIES_POINTS_BASIS_DATA_TABLE_NAME,
            time_series_point_schema,
        ),
        (paths.CHARGE_LINK_PERIODS_BASIS_DATA_TABLE_NAME, charge_link_periods_schema),
        (
            paths.CHARGE_MASTER_DATA_PERIODS_BASIS_DATA_TABLE_NAME,
            charge_price_information_periods_schema,
        ),
        (paths.CHARGE_PRICE_POINTS_BASIS_DATA_TABLE_NAME, charge_price_points_schema),
        (
            paths.GRID_LOSS_METERING_POINTS_BASIS_DATA_TABLE_NAME,
            grid_loss_metering_points_schema,
        ),
    ],
)
def test__when_wholesale_calculation__basis_data_is_stored_with_correct_schema(
    spark: SparkSession,
    executed_wholesale_fixing: None,
    basis_data_table_name: str,
    expected_schema: StructType,
) -> None:
    # Arrange
    actual = spark.read.table(
        f"{paths.BASIS_DATA_DATABASE_NAME}.{basis_data_table_name}"
    )

    # Act: Calculator job is executed just once per session.
    #      See the fixtures `results_df` and `executed_wholesale_fixing`

    # Assert
    assert actual.schema == expected_schema


@pytest.mark.parametrize(
    "view_name, has_data",
    [
        (
            f"{paths.EdiResults.DATABASE_NAME}.{paths.EdiResults.ENERGY_RESULT_POINTS_PER_GA_V1_VIEW_NAME}",
            True,
        ),
        (
            f"{paths.EdiResults.DATABASE_NAME}.{paths.EdiResults.ENERGY_RESULT_POINTS_PER_BRP_GA_V1_VIEW_NAME}",
            False,
        ),
        (
            f"{paths.EdiResults.DATABASE_NAME}.{paths.EdiResults.ENERGY_RESULT_POINTS_PER_ES_BRP_GA_V1_VIEW_NAME}",
            True,
        ),
    ],
)
def test__when_wholesale_fixing__view_has_data_if_expected(
    spark: SparkSession, executed_wholesale_fixing: None, view_name: str, has_data: bool
) -> None:
    actual = spark.sql(f"SELECT * FROM {view_name}")
    assert actual.count() > 0 if has_data else actual.count() == 0
