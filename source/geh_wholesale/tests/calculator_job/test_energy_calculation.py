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

from geh_wholesale.codelists import (
    MeteringPointType,
    TimeSeriesType,
)
from geh_wholesale.databases.table_column_names import TableColumnNames
from geh_wholesale.infrastructure import paths
from geh_wholesale.infrastructure.infrastructure_settings import InfrastructureSettings

from . import configuration as c


def test__balance_fixing_exchange_per_neighbor_result_type__is_created(
    spark: SparkSession,
    executed_balance_fixing: None,  # Fixture executing the balance fixing calculation
) -> None:
    with pytest.MonkeyPatch.context() as ctx:
        ctx.setenv("DATABASE_WHOLESALE_RESULTS_INTERNAL", "wholesale_results_internal")
        actual = spark.read.table(
            f"{paths.WholesaleResultsInternalDatabase().DATABASE_WHOLESALE_RESULTS_INTERNAL}.{paths.WholesaleResultsInternalDatabase().EXCHANGE_PER_NEIGHBOR_TABLE_NAME}"
        )

        # Assert: Result(s) are created if there are rows
        assert (
            actual.where(f.col(TableColumnNames.calculation_id) == c.executed_balance_fixing_calculation_id).count() > 0
        ), actual.collect()


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
    with pytest.MonkeyPatch.context() as ctx:
        ctx.setenv("DATABASE_WHOLESALE_RESULTS_INTERNAL", "wholesale_results_internal")
        actual = (
            spark.read.table(
                f"{paths.WholesaleResultsInternalDatabase().DATABASE_WHOLESALE_RESULTS_INTERNAL}.{paths.WholesaleResultsInternalDatabase.GRID_LOSS_METERING_POINT_TIME_SERIES_TABLE_NAME}"
            )
            .where(f.col(TableColumnNames.calculation_id) == c.executed_balance_fixing_calculation_id)
            .where(f.col(TableColumnNames.metering_point_type) == metering_point_type)
        )

        # Assert: Result(s) are created if there are rows
        assert actual.count() > 0


@pytest.mark.parametrize(
    ("time_series_type", "table_name"),
    [
        (TimeSeriesType.EXCHANGE.value, "ENERGY_TABLE_NAME"),
        (TimeSeriesType.PRODUCTION.value, "ENERGY_TABLE_NAME"),
        (TimeSeriesType.PRODUCTION.value, "ENERGY_PER_BRP_TABLE_NAME"),
        (TimeSeriesType.PRODUCTION.value, "ENERGY_PER_ES_TABLE_NAME"),
        (TimeSeriesType.NON_PROFILED_CONSUMPTION.value, "ENERGY_TABLE_NAME"),
        (TimeSeriesType.NON_PROFILED_CONSUMPTION.value, "ENERGY_PER_BRP_TABLE_NAME"),
        (TimeSeriesType.NON_PROFILED_CONSUMPTION.value, "ENERGY_PER_ES_TABLE_NAME"),
        (TimeSeriesType.FLEX_CONSUMPTION.value, "ENERGY_TABLE_NAME"),
        (TimeSeriesType.FLEX_CONSUMPTION.value, "ENERGY_PER_BRP_TABLE_NAME"),
        (TimeSeriesType.FLEX_CONSUMPTION.value, "ENERGY_PER_ES_TABLE_NAME"),
        (TimeSeriesType.GRID_LOSS.value, "ENERGY_TABLE_NAME"),
        (TimeSeriesType.TOTAL_CONSUMPTION.value, "ENERGY_TABLE_NAME"),
        (TimeSeriesType.TEMP_FLEX_CONSUMPTION.value, "ENERGY_TABLE_NAME"),
        (TimeSeriesType.TEMP_PRODUCTION.value, "ENERGY_TABLE_NAME"),
    ],
)
def test__balance_fixing_energy_result_type__is_created(
    spark: SparkSession,
    executed_balance_fixing: None,  # Fixture executing the balance fixing calculation
    time_series_type: str,
    table_name: str,
    monkeypatch,
) -> None:
    monkeypatch.setenv("DATABASE_WHOLESALE_RESULTS_INTERNAL", "wholesale_results_internal")
    actual_table_name = paths.WholesaleResultsInternalDatabase().model_dump().get(table_name)
    # actual = (
    #    spark.read.table(
    #        f"{paths.WholesaleResultsInternalDatabase().DATABASE_WHOLESALE_RESULTS_INTERNAL}.{actual_table_name}"
    #    )
    #    .where(f.col(TableColumnNames.calculation_id) == c.executed_balance_fixing_calculation_id)
    #    .where(f.col(TableColumnNames.time_series_type) == time_series_type)
    # )

    # Assert: Result(s) are created if there are rows
    # assert actual.count() > 0


@pytest.mark.parametrize(
    "basis_data_table_name",
    [
        "METERING_POINT_PERIODS_TABLE_NAME",
        "TIME_SERIES_POINTS_TABLE_NAME",
    ],
)
def test__when_energy_calculation__basis_data_is_stored(
    spark: SparkSession,
    executed_balance_fixing: None,
    basis_data_table_name: str,
    monkeypatch,
    infrastructure_settings: InfrastructureSettings,
) -> None:
    monkeypatch.setenv("DATABASE_WHOLESALE_BASIS_DATA_INTERNAL", "wholesale_basis_data_internal")
    actual_table_name = paths.WholesaleBasisDataInternalDatabase().model_dump().get(basis_data_table_name)
    # Arrange
    actual = spark.read.table(
        f"{infrastructure_settings.catalog_name}.{paths.WholesaleBasisDataInternalDatabase().DATABASE_WHOLESALE_BASIS_DATA_INTERNAL}.{actual_table_name}"
    ).where(f.col(TableColumnNames.calculation_id) == c.executed_balance_fixing_calculation_id)

    # Act: Calculator job is executed just once per session.
    #      See the fixtures `results_df` and `executed_wholesale_fixing`

    # Assert: The result is created if there are rows
    assert actual.count() > 0


def test__when_calculation_is_stored__contains_calculation_succeeded_time(
    spark: SparkSession,
    executed_balance_fixing: None,
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    monkeypatch.setenv("DATABASE_WHOLESALE_INTERNAL", "wholesale_internal")
    # Arrange
    actual = spark.read.table(
        f"{paths.WholesaleInternalDatabase().DATABASE_WHOLESALE_INTERNAL}.{paths.WholesaleInternalDatabase().CALCULATIONS_TABLE_NAME}"
    ).where(f.col(TableColumnNames.calculation_id) == c.executed_balance_fixing_calculation_id)

    # Act: Calculator job is executed just once per session.
    #      See the fixtures `results_df` and `executed_wholesale_fixing`

    # Assert
    assert actual.count() == 1
    assert actual.collect()[0][TableColumnNames.calculation_succeeded_time] is not None


def test__when_energy_calculation__calculation_grid_areas_are_stored(
    spark: SparkSession, executed_balance_fixing: None, monkeypatch: pytest.MonkeyPatch
) -> None:
    monkeypatch.setenv("DATABASE_WHOLESALE_INTERNAL", "wholesale_internal")
    # Arrange
    actual = spark.read.table(
        f"{paths.WholesaleInternalDatabase().DATABASE_WHOLESALE_INTERNAL}.{paths.WholesaleInternalDatabase().CALCULATION_GRID_AREAS_TABLE_NAME}"
    ).where(f.col(TableColumnNames.calculation_id) == c.executed_balance_fixing_calculation_id)

    # Act: Calculator job is executed just once per session.
    #      See the fixtures `results_df` and `executed_wholesale_fixing`

    # Assert: The result is created if there are rows
    assert actual.count() > 0


@pytest.mark.parametrize(
    ("database", "table_name", "view_name"),
    [
        (
            paths.WholesaleInternalDatabase,
            "DATABASE_WHOLESALE_INTERNAL",
            "SUCCEEDED_EXTERNAL_CALCULATIONS_V1_VIEW_NAME",
        ),
        (paths.WholesaleResultsDatabase, "DATABASE_WHOLESALE_RESULTS", "ENERGY_V1_VIEW_NAME"),
        (paths.WholesaleResultsDatabase, "DATABASE_WHOLESALE_RESULTS", "ENERGY_PER_BRP_V1_VIEW_NAME"),
        (paths.WholesaleResultsDatabase, "DATABASE_WHOLESALE_RESULTS", "ENERGY_PER_ES_V1_VIEW_NAME"),
        (
            paths.WholesaleResultsDatabase,
            "DATABASE_WHOLESALE_RESULTS",
            "GRID_LOSS_METERING_POINT_TIME_SERIES_VIEW_NAME",
        ),
        (paths.WholesaleResultsDatabase, "DATABASE_WHOLESALE_RESULTS", "EXCHANGE_PER_NEIGHBOR_V1_VIEW_NAME"),
        (paths.WholesaleSapDatabase, "DATABASE_WHOLESALE_SAP", "LATEST_CALCULATIONS_HISTORY_V1_VIEW_NAME,"),
        (paths.WholesaleSapDatabase, "DATABASE_WHOLESALE_SAP", "ENERGY_V1_VIEW_NAME"),
    ],
)
def test__when_balance_fixing__view_has_data_if_expected_result(
    spark: SparkSession,
    executed_balance_fixing: None,
    database,
    table_name: str,
    view_name: str,
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    monkeypatch.setenv("DATABASE_WHOLESALE_RESULTS", "wholesale_results")
    monkeypatch.setenv("DATABASE_WHOLESALE_SAP", "wholesale_sap")
    monkeypatch.setenv("DATABASE_WHOLESALE_INTERNAL", "wholesale_internal")

    actual_table_name = database().model_dump().get(table_name)
    actual_view_name = database().model_dump().get(view_name)

    # full_view = f"{actual_table_name}.{actual_view_name}"
    # actual = spark.sql(f"SELECT * FROM {full_view}").where(
    #    f.col(TableColumnNames.calculation_id) == c.executed_balance_fixing_calculation_id
    # )
    # assert actual.count() > 0 if True else actual.count() == 0
