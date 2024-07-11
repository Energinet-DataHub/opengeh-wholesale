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
from pyspark.sql import SparkSession

from package.codelists import (
    TimeSeriesType,
)
from package.constants import EnergyResultColumnNames
from package.infrastructure import paths
from package.infrastructure.infrastructure_settings import InfrastructureSettings
from . import configuration as c

ALL_ENERGY_RESULT_TYPES = {
    (
        TimeSeriesType.EXCHANGE_PER_NEIGHBOR.value,
        paths.WholesaleResultsInternalDatabase.EXCHANGE_PER_NEIGHBOR_TABLE_NAME,
    ),
    (
        TimeSeriesType.EXCHANGE.value,
        paths.WholesaleResultsInternalDatabase.ENERGY_TABLE_NAME,
    ),
    (
        TimeSeriesType.PRODUCTION.value,
        paths.WholesaleResultsInternalDatabase.ENERGY_TABLE_NAME,
    ),
    (
        TimeSeriesType.PRODUCTION.value,
        paths.WholesaleResultsInternalDatabase.ENERGY_PER_BRP_TABLE_NAME,
    ),
    (
        TimeSeriesType.PRODUCTION.value,
        paths.WholesaleResultsInternalDatabase.ENERGY_PER_ES_TABLE_NAME,
    ),
    (
        TimeSeriesType.NON_PROFILED_CONSUMPTION.value,
        paths.WholesaleResultsInternalDatabase.ENERGY_TABLE_NAME,
    ),
    (
        TimeSeriesType.NON_PROFILED_CONSUMPTION.value,
        paths.WholesaleResultsInternalDatabase.ENERGY_PER_BRP_TABLE_NAME,
    ),
    (
        TimeSeriesType.NON_PROFILED_CONSUMPTION.value,
        paths.WholesaleResultsInternalDatabase.ENERGY_PER_ES_TABLE_NAME,
    ),
    (
        TimeSeriesType.FLEX_CONSUMPTION.value,
        paths.WholesaleResultsInternalDatabase.ENERGY_TABLE_NAME,
    ),
    (
        TimeSeriesType.FLEX_CONSUMPTION.value,
        paths.WholesaleResultsInternalDatabase.ENERGY_PER_BRP_TABLE_NAME,
    ),
    (
        TimeSeriesType.FLEX_CONSUMPTION.value,
        paths.WholesaleResultsInternalDatabase.ENERGY_PER_ES_TABLE_NAME,
    ),
    (
        TimeSeriesType.GRID_LOSS.value,
        paths.WholesaleResultsInternalDatabase.ENERGY_TABLE_NAME,
    ),
    (
        TimeSeriesType.POSITIVE_GRID_LOSS.value,
        paths.WholesaleResultsInternalDatabase.GRID_LOSS_METERING_POINT_TIME_SERIES_TABLE_NAME,
    ),
    (
        TimeSeriesType.NEGATIVE_GRID_LOSS.value,
        paths.WholesaleResultsInternalDatabase.GRID_LOSS_METERING_POINT_TIME_SERIES_TABLE_NAME,
    ),
    (
        TimeSeriesType.TOTAL_CONSUMPTION.value,
        paths.WholesaleResultsInternalDatabase.ENERGY_TABLE_NAME,
    ),
    (
        TimeSeriesType.TEMP_FLEX_CONSUMPTION.value,
        paths.WholesaleResultsInternalDatabase.ENERGY_TABLE_NAME,
    ),
    (
        TimeSeriesType.TEMP_PRODUCTION.value,
        paths.WholesaleResultsInternalDatabase.ENERGY_TABLE_NAME,
    ),
}


@pytest.mark.parametrize(
    "time_series_type, table_name",
    ALL_ENERGY_RESULT_TYPES,
)
def test__balance_fixing_output__is_created(
    spark: SparkSession,
    executed_balance_fixing: None,  # Fixture executing the balance fixing calculation
    time_series_type: str,
    table_name: str,
) -> None:
    actual = (
        spark.read.table(
            f"{paths.WholesaleResultsInternalDatabase.DATABASE_NAME}.{table_name}"
        )
        .where(
            f.col(EnergyResultColumnNames.calculation_id)
            == c.executed_balance_fixing_calculation_id
        )
        .where(f.col(EnergyResultColumnNames.time_series_type) == time_series_type)
    )

    # Assert: Result(s) are created if there are rows
    assert actual.count() > 0


@pytest.mark.parametrize(
    "table_name, expected_result_types_count",
    [
        (
            paths.WholesaleResultsInternalDatabase.ENERGY_TABLE_NAME,
            len(
                [
                    TimeSeriesType.EXCHANGE,
                    TimeSeriesType.FLEX_CONSUMPTION,
                    TimeSeriesType.GRID_LOSS,
                    TimeSeriesType.NON_PROFILED_CONSUMPTION,
                    TimeSeriesType.PRODUCTION,
                    TimeSeriesType.TEMP_FLEX_CONSUMPTION,
                    TimeSeriesType.TEMP_PRODUCTION,
                    TimeSeriesType.TOTAL_CONSUMPTION,
                ]
            ),
        ),
        (
            paths.WholesaleResultsInternalDatabase.ENERGY_PER_BRP_TABLE_NAME,
            len(
                [
                    TimeSeriesType.EXCHANGE,
                    TimeSeriesType.FLEX_CONSUMPTION,
                    TimeSeriesType.GRID_LOSS,
                    TimeSeriesType.NON_PROFILED_CONSUMPTION,
                    TimeSeriesType.PRODUCTION,
                    TimeSeriesType.TEMP_FLEX_CONSUMPTION,
                    TimeSeriesType.TEMP_PRODUCTION,
                    TimeSeriesType.TOTAL_CONSUMPTION,
                ]
            ),
        ),
        (
            paths.WholesaleResultsInternalDatabase.ENERGY_PER_ES_TABLE_NAME,
            len(
                [
                    TimeSeriesType.EXCHANGE,
                    TimeSeriesType.FLEX_CONSUMPTION,
                    TimeSeriesType.GRID_LOSS,
                    TimeSeriesType.NON_PROFILED_CONSUMPTION,
                    TimeSeriesType.PRODUCTION,
                    TimeSeriesType.TEMP_FLEX_CONSUMPTION,
                    TimeSeriesType.TEMP_PRODUCTION,
                    TimeSeriesType.TOTAL_CONSUMPTION,
                ]
            ),
        ),
        (
            paths.WholesaleResultsInternalDatabase.GRID_LOSS_METERING_POINT_TIME_SERIES_TABLE_NAME,
            len(
                [
                    TimeSeriesType.NEGATIVE_GRID_LOSS,
                    TimeSeriesType.POSITIVE_GRID_LOSS,
                ]
            ),
        ),
        (
            paths.WholesaleResultsInternalDatabase.EXCHANGE_PER_NEIGHBOR_TABLE_NAME,
            len(
                [
                    TimeSeriesType.EXCHANGE_PER_NEIGHBOR,
                ]
            ),
        ),
    ],
)
def test__balance_fixing_result__has_expected_number_of_result_types(
    spark: SparkSession,
    executed_balance_fixing: None,
    table_name: str,
    expected_result_types_count: int,
) -> None:
    # Arrange
    actual = (
        spark.read.table(
            f"{paths.WholesaleResultsInternalDatabase.DATABASE_NAME}.{table_name}"
        )
        .where(  # Be resilient to other tests writing to same table
            f.col(EnergyResultColumnNames.calculation_id)
            == c.executed_balance_fixing_calculation_id
        )
        .select(
            EnergyResultColumnNames.time_series_type,
        )
        .distinct()
    )

    # Assert
    assert actual.count() == expected_result_types_count


@pytest.mark.parametrize(
    "basis_data_table_name",
    [
        paths.WholesaleBasisDataInternalDatabase.METERING_POINT_PERIODS_TABLE_NAME,
        paths.WholesaleBasisDataInternalDatabase.TIME_SERIES_POINTS_TABLE_NAME,
    ],
)
def test__when_energy_calculation__basis_data_is_stored(
    spark: SparkSession,
    executed_balance_fixing: None,
    basis_data_table_name: str,
    infrastructure_settings: InfrastructureSettings,
) -> None:
    # Arrange
    actual = spark.read.table(
        f"{infrastructure_settings.catalog_name}.{paths.WholesaleBasisDataInternalDatabase.DATABASE_NAME}.{basis_data_table_name}"
    ).where(f.col("calculation_id") == c.executed_balance_fixing_calculation_id)

    # Act: Calculator job is executed just once per session.
    #      See the fixtures `results_df` and `executed_wholesale_fixing`

    # Assert: The result is created if there are rows
    assert actual.count() > 0


def test__when_energy_calculation__calculation_is_stored(
    spark: SparkSession,
    executed_balance_fixing: None,
) -> None:
    # Arrange
    actual = spark.read.table(
        f"{paths.HiveBasisDataDatabase.DATABASE_NAME}.{paths.HiveBasisDataDatabase.CALCULATIONS_TABLE_NAME}"
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
            f"{paths.HiveSettlementReportPublicDataModel.DATABASE_NAME}.{paths.HiveSettlementReportPublicDataModel.METERING_POINT_PERIODS_VIEW_NAME_V1}",
            True,
        ),
        (
            f"{paths.HiveSettlementReportPublicDataModel.DATABASE_NAME}.{paths.HiveSettlementReportPublicDataModel.METERING_POINT_TIME_SERIES_VIEW_NAME_V1}",
            True,
        ),
        (
            f"{paths.HiveSettlementReportPublicDataModel.DATABASE_NAME}.{paths.HiveSettlementReportPublicDataModel.ENERGY_RESULT_POINTS_PER_GA_VIEW_NAME_V1}",
            True,
        ),
        (
            f"{paths.HiveSettlementReportPublicDataModel.DATABASE_NAME}.{paths.HiveSettlementReportPublicDataModel.ENERGY_RESULT_POINTS_PER_ES_GA_SETTLEMENT_REPORT_VIEW_NAME_V1}",
            True,
        ),
        (
            f"{paths.HiveSettlementReportPublicDataModel.DATABASE_NAME}.{paths.HiveSettlementReportPublicDataModel.CURRENT_BALANCE_FIXING_CALCULATION_VERSION_VIEW_NAME_V1}",
            True,
        ),
    ],
)
def test__when_balance_fixing__view_has_data_if_expected(
    spark: SparkSession, executed_balance_fixing: None, view_name: str, has_data: bool
) -> None:
    actual = spark.sql(f"SELECT * FROM {view_name}")
    assert actual.count() > 0 if has_data else actual.count() == 0
