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
    MeteringPointType,
)
from package.databases.table_column_names import TableColumnNames
from package.infrastructure import paths
from package.infrastructure.infrastructure_settings import InfrastructureSettings
from . import configuration as c


def test__balance_fixing_exchange_per_neighbor_result_type__is_created(
    spark: SparkSession,
    executed_balance_fixing: None,  # Fixture executing the balance fixing calculation
) -> None:
    actual = spark.read.table(
        f"{paths.WholesaleResultsInternalDatabase.DATABASE_NAME}.{paths.WholesaleResultsInternalDatabase.EXCHANGE_PER_NEIGHBOR_TABLE_NAME}"
    ).where(
        f.col(TableColumnNames.calculation_id)
        == c.executed_balance_fixing_calculation_id
    )

    # Assert: Result(s) are created if there are rows
    assert actual.count() > 0


@pytest.mark.parametrize(
    "metering_point_type",
    [
        MeteringPointType.CONSUMPTION.value,
        MeteringPointType.PRODUCTION.value,
    ],
)
def test__balance_fixing_grid_loss_time_series_result_type__is_created(
    spark: SparkSession,
    executed_balance_fixing: None,  # Fixture executing the balance fixing calculation
    metering_point_type: str,
) -> None:
    actual = (
        spark.read.table(
            f"{paths.WholesaleResultsInternalDatabase.DATABASE_NAME}.{paths.WholesaleResultsInternalDatabase.GRID_LOSS_METERING_POINT_TIME_SERIES_TABLE_NAME}"
        )
        .where(
            f.col(TableColumnNames.calculation_id)
            == c.executed_balance_fixing_calculation_id
        )
        .where(f.col(TableColumnNames.metering_point_type) == metering_point_type)
    )

    # Assert: Result(s) are created if there are rows
    assert actual.count() > 0


@pytest.mark.parametrize(
    "time_series_type, table_name",
    [
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
    ],
)
def test__balance_fixing_energy_result_type__is_created(
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
            f.col(TableColumnNames.calculation_id)
            == c.executed_balance_fixing_calculation_id
        )
        .where(f.col(TableColumnNames.time_series_type) == time_series_type)
    )

    # Assert: Result(s) are created if there are rows
    assert actual.count() > 0


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
    ).where(
        f.col(TableColumnNames.calculation_id)
        == c.executed_balance_fixing_calculation_id
    )

    # Act: Calculator job is executed just once per session.
    #      See the fixtures `results_df` and `executed_wholesale_fixing`

    # Assert: The result is created if there are rows
    assert actual.count() > 0


def test__when_calculation_is_stored__contains_calculation_succeeded_time(
    spark: SparkSession,
    executed_balance_fixing: None,
) -> None:
    # Arrange
    actual = spark.read.table(
        f"{paths.WholesaleInternalDatabase.DATABASE_NAME}.{paths.WholesaleInternalDatabase.CALCULATIONS_TABLE_NAME}"
    ).where(
        f.col(TableColumnNames.calculation_id)
        == c.executed_balance_fixing_calculation_id
    )

    # Act: Calculator job is executed just once per session.
    #      See the fixtures `results_df` and `executed_wholesale_fixing`

    # Assert
    assert actual.count() == 1
    assert actual.collect()[0][TableColumnNames.calculation_succeeded_time] is not None


def test__when_energy_calculation__calculation_grid_areas_are_stored(
    spark: SparkSession,
    executed_balance_fixing: None,
) -> None:
    # Arrange
    actual = spark.read.table(
        f"{paths.WholesaleInternalDatabase.DATABASE_NAME}.{paths.WholesaleInternalDatabase.CALCULATION_GRID_AREAS_TABLE_NAME}"
    ).where(
        f.col(TableColumnNames.calculation_id)
        == c.executed_balance_fixing_calculation_id
    )

    # Act: Calculator job is executed just once per session.
    #      See the fixtures `results_df` and `executed_wholesale_fixing`

    # Assert: The result is created if there are rows
    assert actual.count() > 0


@pytest.mark.parametrize(
    "view_name, has_data",
    [
        (
            f"{paths.WholesaleInternalDatabase.DATABASE_NAME}.{paths.WholesaleInternalDatabase.SUCCEEDED_EXTERNAL_CALCULATIONS_V1_VIEW_NAME}",
            True,
        ),
        (
            f"{paths.WholesaleResultsDatabase.DATABASE_NAME}.{paths.WholesaleResultsDatabase.ENERGY_V1_VIEW_NAME}",
            True,
        ),
        (
            f"{paths.WholesaleResultsDatabase.DATABASE_NAME}.{paths.WholesaleResultsDatabase.ENERGY_PER_BRP_V1_VIEW_NAME}",
            True,
        ),
        (
            f"{paths.WholesaleResultsDatabase.DATABASE_NAME}.{paths.WholesaleResultsDatabase.ENERGY_PER_ES_V1_VIEW_NAME}",
            True,
        ),
        (
            f"{paths.WholesaleResultsDatabase.DATABASE_NAME}.{paths.WholesaleResultsDatabase.GRID_LOSS_METERING_POINT_TIME_SERIES_VIEW_NAME}",
            True,
        ),
        (
            f"{paths.WholesaleResultsDatabase.DATABASE_NAME}.{paths.WholesaleResultsDatabase.EXCHANGE_PER_NEIGHBOR_V1_VIEW_NAME}",
            True,
        ),
        (
            f"{paths.WholesaleSapDatabase.DATABASE_NAME}.{paths.WholesaleSapDatabase.LATEST_CALCULATIONS_HISTORY_V1_VIEW_NAME}",
            True,
        ),
        (
            f"{paths.WholesaleSapDatabase.DATABASE_NAME}.{paths.WholesaleSapDatabase.ENERGY_V1_VIEW_NAME}",
            True,
        ),
    ],
)
def test__when_balance_fixing__view_has_data_if_expected(
    spark: SparkSession, executed_balance_fixing: None, view_name: str, has_data: bool
) -> None:
    actual = spark.sql(f"SELECT * FROM {view_name}").where(
        f.col(TableColumnNames.calculation_id)
        == c.executed_balance_fixing_calculation_id
    )

    assert actual.count() > 0 if has_data else actual.count() == 0
