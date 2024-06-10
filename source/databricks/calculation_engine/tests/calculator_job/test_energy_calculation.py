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

from package.codelists import (
    AggregationLevel,
    TimeSeriesType,
)
from package.constants import EnergyResultColumnNames
from package.infrastructure import paths
from . import configuration as c

ALL_ENERGY_RESULT_TYPES = {
    (
        TimeSeriesType.NET_EXCHANGE_PER_NEIGHBORING_GA.value,
        AggregationLevel.TOTAL_GA.value,
    ),
    (
        TimeSeriesType.NET_EXCHANGE_PER_GA.value,
        AggregationLevel.TOTAL_GA.value,
    ),
    (
        TimeSeriesType.PRODUCTION.value,
        AggregationLevel.ES_PER_BRP_PER_GA.value,
    ),
    (
        TimeSeriesType.PRODUCTION.value,
        AggregationLevel.ES_PER_GA.value,
    ),
    (
        TimeSeriesType.PRODUCTION.value,
        AggregationLevel.BRP_PER_GA.value,
    ),
    (
        TimeSeriesType.PRODUCTION.value,
        AggregationLevel.TOTAL_GA.value,
    ),
    (
        TimeSeriesType.NON_PROFILED_CONSUMPTION.value,
        AggregationLevel.ES_PER_BRP_PER_GA.value,
    ),
    (
        TimeSeriesType.NON_PROFILED_CONSUMPTION.value,
        AggregationLevel.ES_PER_GA.value,
    ),
    (
        TimeSeriesType.NON_PROFILED_CONSUMPTION.value,
        AggregationLevel.BRP_PER_GA.value,
    ),
    (
        TimeSeriesType.NON_PROFILED_CONSUMPTION.value,
        AggregationLevel.TOTAL_GA.value,
    ),
    (
        TimeSeriesType.FLEX_CONSUMPTION.value,
        AggregationLevel.ES_PER_BRP_PER_GA.value,
    ),
    (
        TimeSeriesType.FLEX_CONSUMPTION.value,
        AggregationLevel.ES_PER_GA.value,
    ),
    (
        TimeSeriesType.FLEX_CONSUMPTION.value,
        AggregationLevel.BRP_PER_GA.value,
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
    ALL_ENERGY_RESULT_TYPES,
)
def test__balance_fixing_result__is_created(
    balance_fixing_results_df: DataFrame,
    time_series_type: str,
    aggregation_level: str,
) -> None:
    # Arrange
    result_df = (
        balance_fixing_results_df.where(
            f.col(EnergyResultColumnNames.calculation_id)
            == c.executed_balance_fixing_calculation_id
        )
        .where(f.col(EnergyResultColumnNames.time_series_type) == time_series_type)
        .where(f.col(EnergyResultColumnNames.aggregation_level) == aggregation_level)
    )

    # Act: Calculator job is executed just once per session. See the fixtures `balance_fixing_results_df` and `executed_balance_fixing`

    # Assert: The result is created if there are rows
    assert result_df.count() > 0


def test__balance_fixing_result__has_expected_number_of_result_types(
    balance_fixing_results_df: DataFrame,
) -> None:
    # Arrange
    actual_result_type_count = (
        balance_fixing_results_df.where(
            f.col(EnergyResultColumnNames.calculation_id)
            == c.executed_balance_fixing_calculation_id
        )
        .where(
            f.col(EnergyResultColumnNames.calculation_id)
            == c.executed_balance_fixing_calculation_id
        )
        .select(
            EnergyResultColumnNames.time_series_type,
            EnergyResultColumnNames.aggregation_level,
        )
        .distinct()
        .count()
    )

    # Act: Calculator job is executed just once per session. See the fixtures `results_df` and `executed_wholesale_fixing`

    # Assert: The result is created if there are rows
    assert actual_result_type_count == len(ALL_ENERGY_RESULT_TYPES)


@pytest.mark.parametrize(
    "basis_data_table_name",
    [
        paths.BasisDataDatabase.CALCULATIONS_TABLE_NAME,
        paths.BasisDataDatabase.METERING_POINT_PERIODS_TABLE_NAME,
        paths.BasisDataDatabase.TIME_SERIES_POINTS_TABLE_NAME,
    ],
)
def test__when_energy_calculation__basis_data_is_stored(
    spark: SparkSession,
    executed_balance_fixing: None,
    basis_data_table_name: str,
) -> None:
    # Arrange
    actual = spark.read.table(
        f"{paths.BasisDataDatabase.DATABASE_NAME}.{basis_data_table_name}"
    ).where(f.col("calculation_id") == c.executed_balance_fixing_calculation_id)

    # Act: Calculator job is executed just once per session.
    #      See the fixtures `results_df` and `executed_wholesale_fixing`

    # Assert: The result is created if there are rows
    assert actual.count() > 0


@pytest.mark.parametrize(
    "view_name, has_data",
    [
        (
            f"{paths.CalculationResultsPublicDataModel.DATABASE_NAME}.{paths.CalculationResultsPublicDataModel.ENERGY_RESULT_POINTS_PER_GA_V1_VIEW_NAME}",
            True,
        ),
        (
            f"{paths.CalculationResultsPublicDataModel.DATABASE_NAME}.{paths.CalculationResultsPublicDataModel.ENERGY_RESULT_POINTS_PER_BRP_GA_V1_VIEW_NAME}",
            True,
        ),
        (
            f"{paths.CalculationResultsPublicDataModel.DATABASE_NAME}.{paths.CalculationResultsPublicDataModel.ENERGY_RESULT_POINTS_PER_ES_BRP_GA_V1_VIEW_NAME}",
            True,
        ),
        (
            f"{paths.SettlementReportPublicDataModel.DATABASE_NAME}.{paths.SettlementReportPublicDataModel.METERING_POINT_PERIODS_VIEW_NAME_V1}",
            True,
        ),
        (
            f"{paths.SettlementReportPublicDataModel.DATABASE_NAME}.{paths.SettlementReportPublicDataModel.METERING_POINT_TIME_SERIES_VIEW_NAME_V1}",
            True,
        ),
        (
            f"{paths.SettlementReportPublicDataModel.DATABASE_NAME}.{paths.SettlementReportPublicDataModel.ENERGY_RESULT_POINTS_PER_GA_VIEW_NAME_V1}",
            True,
        ),
        (
            f"{paths.SettlementReportPublicDataModel.DATABASE_NAME}.{paths.SettlementReportPublicDataModel.ENERGY_RESULT_POINTS_PER_ES_GA_SETTLEMENT_REPORT_VIEW_NAME_V1}",
            True,
        ),
        (
            f"{paths.SettlementReportPublicDataModel.DATABASE_NAME}.{paths.SettlementReportPublicDataModel.CURRENT_BALANCE_FIXING_CALCULATION_VERSION_VIEW_NAME_V1}",
            True,
        ),
    ],
)
def test__when_balance_fixing__view_has_data_if_expected(
    spark: SparkSession, executed_balance_fixing: None, view_name: str, has_data: bool
) -> None:
    actual = spark.sql(f"SELECT * FROM {view_name}")
    assert actual.count() > 0 if has_data else actual.count() == 0
