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

from geh_wholesale.codelists import (
    ChargeType,
    TimeSeriesType,
    WholesaleResultResolution,
)
from geh_wholesale.databases.table_column_names import TableColumnNames
from geh_wholesale.databases.wholesale_basis_data_internal.schemas import (
    charge_link_periods_schema,
    charge_price_information_periods_schema,
    charge_price_points_schema,
    grid_loss_metering_point_ids_schema,
    metering_point_periods_schema,
    time_series_points_schema,
)
from geh_wholesale.infrastructure import paths
from geh_wholesale.infrastructure.infrastructure_settings import InfrastructureSettings

from . import configuration as c


@pytest.mark.parametrize(
    ("time_series_type", "table_name"),
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
            paths.WholesaleResultsInternalDatabase.ENERGY_PER_ES_TABLE_NAME,
        ),
        (
            TimeSeriesType.NON_PROFILED_CONSUMPTION.value,
            paths.WholesaleResultsInternalDatabase.ENERGY_TABLE_NAME,
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
def test__wholesale_fixing_result_type__is_created(
    spark: SparkSession,
    wholesale_fixing_energy_results_df: None,
    time_series_type: str,
    table_name: str,
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    monkeypatch.setenv("DATABASE_WHOLESALE_RESULTS_INTERNAL", "wholesale_results_internal")
    actual = (
        spark.read.table(f"{paths.WholesaleResultsInternalDatabase().DATABASE_WHOLESALE_RESULTS_INTERNAL}.{table_name}")
        .where(f.col(TableColumnNames.calculation_id) == c.executed_wholesale_calculation_id)
        .where(f.col(TableColumnNames.time_series_type) == time_series_type)
    )

    # Assert: Result(s) are created if there are rows
    assert actual.count() > 0


ENERGY_RESULT_TYPES = {
    TimeSeriesType.EXCHANGE.value,
    TimeSeriesType.PRODUCTION.value,
    TimeSeriesType.NON_PROFILED_CONSUMPTION.value,
    TimeSeriesType.FLEX_CONSUMPTION.value,
    TimeSeriesType.GRID_LOSS.value,
    TimeSeriesType.TOTAL_CONSUMPTION.value,
    TimeSeriesType.TEMP_FLEX_CONSUMPTION.value,
    TimeSeriesType.TEMP_PRODUCTION.value,
}


@pytest.mark.parametrize(
    "time_series_type",
    ENERGY_RESULT_TYPES,
)
def test__energy_result__is_created(
    wholesale_fixing_energy_results_df: DataFrame,
    time_series_type: str,
) -> None:
    # Arrange
    result_df = wholesale_fixing_energy_results_df.where(
        f.col(TableColumnNames.calculation_id) == c.executed_wholesale_calculation_id
    ).where(f.col(TableColumnNames.time_series_type) == time_series_type)

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
            f.col(TableColumnNames.calculation_id) == c.executed_wholesale_calculation_id
        )
        .where(f.col(TableColumnNames.calculation_id) == c.executed_wholesale_calculation_id)
        .select(
            TableColumnNames.time_series_type,
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
    ("charge_type", "resolution"),
    WHOLESALE_RESULT_TYPES,
)
def test__wholesale_result__amount_per_charge_is_created(
    wholesale_fixing_amounts_per_charge_df: DataFrame,
    charge_type: ChargeType,
    resolution: WholesaleResultResolution,
) -> None:
    # Arrange
    result_df = (
        wholesale_fixing_amounts_per_charge_df.where(
            f.col(TableColumnNames.calculation_id) == c.executed_wholesale_calculation_id
        )
        .where(f.col(TableColumnNames.charge_type) == charge_type.value)
        .where(f.col(TableColumnNames.resolution) == resolution.value)
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
    # see "test_files/ChargePriceInformationPeriods.csv"
)
def test__monthly_amount_for_tariffs__is_created(
    spark: SparkSession,
    wholesale_fixing_monthly_amounts_per_charge_df: DataFrame,
    charge_code: str,
) -> None:
    # Arrange

    result_df = wholesale_fixing_monthly_amounts_per_charge_df.where(
        f.col(TableColumnNames.charge_type) == ChargeType.TARIFF.value
    ).where(f.col(TableColumnNames.charge_code) == charge_code)

    # Act: Calculator job is executed just once per session.
    #      See the fixtures `results_df` and `executed_wholesale_fixing`

    # Assert: The result is created if there are rows
    assert result_df.count() > 0


@pytest.mark.parametrize("charge_type", [ChargeType.SUBSCRIPTION, ChargeType.FEE])
def test__monthly_amount_for_subscriptions_and_fees__is_created(
    spark: SparkSession,
    wholesale_fixing_monthly_amounts_per_charge_df: DataFrame,
    charge_type: ChargeType,
) -> None:
    # Arrange

    result_df = wholesale_fixing_monthly_amounts_per_charge_df.where(
        f.col(TableColumnNames.charge_type) == charge_type.value
    )

    # Act: Calculator job is executed just once per session.
    #      See the fixtures `results_df` and `executed_wholesale_fixing`

    # Assert: The result is created if there are rows
    assert result_df.count() > 0


def test__total_monthly_amounts__are_stored(
    spark: SparkSession,
    wholesale_fixing_total_monthly_amounts_df: DataFrame,
) -> None:
    # Arrange

    # Act: Calculator job is executed just once per session.
    #      See the fixtures `results_df` and `executed_wholesale_fixing`

    # Assert: The result is created if there are rows
    assert wholesale_fixing_total_monthly_amounts_df.count() > 0


def test__monthly_amounts__are_stored(
    spark: SparkSession,
    wholesale_fixing_monthly_amounts_per_charge_df: DataFrame,
) -> None:
    # Arrange

    # Act: Calculator job is executed just once per session.
    #      See the fixtures `results_df` and `executed_wholesale_fixing`

    # Assert: The result is created if there are rows
    assert wholesale_fixing_monthly_amounts_per_charge_df.count() > 0


@pytest.mark.parametrize(
    "basis_data_table_name",
    paths.WholesaleBasisDataInternalDatabase.TABLE_NAMES,
)
def test__when_wholesale_calculation__basis_data_is_stored(
    spark: SparkSession,
    executed_wholesale_fixing: None,
    basis_data_table_name: str,
) -> None:
    with pytest.MonkeyPatch.context() as ctx:
        ctx.setenv("DATABASE_WHOLESALE_BASIS_DATA_INTERNAL", "wholesale_basis_data_internal")
        # Arrange
        actual = spark.read.table(
            f"{paths.WholesaleBasisDataInternalDatabase().DATABASE_WHOLESALE_BASIS_DATA_INTERNAL}.{basis_data_table_name}"
        ).where(f.col(TableColumnNames.calculation_id) == c.executed_wholesale_calculation_id)

        # Act: Calculator job is executed just once per session.
        #      See the fixtures `results_df` and `executed_wholesale_fixing`

        # Assert
        assert actual.count() > 0


def test__when_calculation_is_stored__contains_calculation_succeeded_time(
    spark: SparkSession,
    executed_wholesale_fixing: None,
) -> None:
    with pytest.MonkeyPatch.context() as ctx:
        ctx.setenv("DATABASE_WHOLESALE_INTERNAL", "wholesale_internal")
        # Arrange
        actual = spark.read.table(
            f"{paths.WholesaleInternalDatabase().DATABASE_WHOLESALE_INTERNAL}.{paths.WholesaleInternalDatabase().CALCULATIONS_TABLE_NAME}"
        ).where(f.col(TableColumnNames.calculation_id) == c.executed_wholesale_calculation_id)

        # Act: Calculator job is executed just once per session.
        #      See the fixtures `results_df` and `executed_wholesale_fixing`

        # Assert
        assert actual.count() == 1
        assert actual.collect()[0][TableColumnNames.calculation_succeeded_time] is not None


def test__when_wholesale_calculation__calculation_grid_areas_are_stored(
    spark: SparkSession,
    executed_wholesale_fixing: None,
) -> None:
    with pytest.MonkeyPatch.context() as ctx:
        ctx.setenv("DATABASE_WHOLESALE_INTERNAL", "wholesale_internal")
        # Arrange
        actual = spark.read.table(
            f"{paths.WholesaleInternalDatabase().DATABASE_WHOLESALE_INTERNAL}.{paths.WholesaleInternalDatabase().CALCULATION_GRID_AREAS_TABLE_NAME}"
        ).where(f.col(TableColumnNames.calculation_id) == c.executed_wholesale_calculation_id)

        # Act: Calculator job is executed just once per session.
        #      See the fixtures `results_df` and `executed_wholesale_fixing`

        # Assert: The result is created if there are rows
        assert actual.count() > 0


@pytest.mark.parametrize(
    ("basis_data_table_name", "expected_schema"),
    [
        (
            paths.WholesaleBasisDataInternalDatabase.METERING_POINT_PERIODS_TABLE_NAME,
            metering_point_periods_schema,
        ),
        (
            paths.WholesaleBasisDataInternalDatabase.TIME_SERIES_POINTS_TABLE_NAME,
            time_series_points_schema,
        ),
        (
            paths.WholesaleBasisDataInternalDatabase.CHARGE_LINK_PERIODS_TABLE_NAME,
            charge_link_periods_schema,
        ),
        (
            paths.WholesaleBasisDataInternalDatabase.CHARGE_PRICE_INFORMATION_PERIODS_TABLE_NAME,
            charge_price_information_periods_schema,
        ),
        (
            paths.WholesaleBasisDataInternalDatabase.CHARGE_PRICE_POINTS_TABLE_NAME,
            charge_price_points_schema,
        ),
        (
            paths.WholesaleBasisDataInternalDatabase.GRID_LOSS_METERING_POINT_IDS_TABLE_NAME,
            grid_loss_metering_point_ids_schema,
        ),
    ],
)
def test__when_wholesale_calculation__basis_data_is_stored_with_correct_schema(
    spark: SparkSession,
    executed_wholesale_fixing: None,
    basis_data_table_name: str,
    expected_schema: StructType,
    infrastructure_settings: InfrastructureSettings,
) -> None:
    with pytest.MonkeyPatch.context() as ctx:
        ctx.setenv("DATABASE_WHOLESALE_BASIS_DATA_INTERNAL", "wholesale_basis_data_internal")
        # Arrange
        actual = spark.read.table(
            f"{infrastructure_settings.catalog_name}.{paths.WholesaleBasisDataInternalDatabase().DATABASE_WHOLESALE_BASIS_DATA_INTERNAL}.{basis_data_table_name}"
        )

        # Act: Calculator job is executed just once per session.
        #      See the fixtures `results_df` and `executed_wholesale_fixing`

        # Assert
        assert actual.schema == expected_schema


@pytest.mark.parametrize(
    ("database_class", "database_name", "view_name"),
    [
        (
            paths.WholesaleResultsDatabase,
            "DATABASE_WHOLESALE_RESULTS",
            paths.WholesaleResultsDatabase.ENERGY_V1_VIEW_NAME,
        ),
        # ( TODO: FIX TEST
        #    paths.WholesaleResultsDatabase,
        #    "DATABASE_WHOLESALE_RESULTS",
        #    paths.WholesaleResultsDatabase.ENERGY_PER_BRP_V1_VIEW_NAME,  # fails
        # ),
        (
            paths.WholesaleResultsDatabase,
            "DATABASE_WHOLESALE_RESULTS",
            paths.WholesaleResultsDatabase.ENERGY_PER_ES_V1_VIEW_NAME,
        ),
        (
            paths.WholesaleResultsDatabase,
            "DATABASE_WHOLESALE_RESULTS",
            paths.WholesaleResultsDatabase.GRID_LOSS_METERING_POINT_TIME_SERIES_VIEW_NAME,
        ),
        # ( TODO: FIX TEST
        #    paths.WholesaleResultsDatabase,
        #    "DATABASE_WHOLESALE_RESULTS",
        #    paths.WholesaleResultsDatabase.EXCHANGE_PER_NEIGHBOR_V1_VIEW_NAME,  # per_neighbor_v1
        # ),
        (
            paths.WholesaleResultsDatabase,
            "DATABASE_WHOLESALE_RESULTS",
            paths.WholesaleResultsDatabase.AMOUNTS_PER_CHARGE_V1_VIEW_NAME,
        ),
        (
            paths.WholesaleResultsDatabase,
            "DATABASE_WHOLESALE_RESULTS",
            paths.WholesaleResultsDatabase.MONTHLY_AMOUNTS_PER_CHARGE_V1_VIEW_NAME,
        ),
        (
            paths.WholesaleResultsDatabase,
            "DATABASE_WHOLESALE_RESULTS",
            paths.WholesaleResultsDatabase.TOTAL_MONTHLY_AMOUNTS_V1_VIEW_NAME,
        ),
        (
            paths.WholesaleSapDatabase,
            "DATABASE_WHOLESALE_SAP",
            paths.WholesaleSapDatabase.LATEST_CALCULATIONS_HISTORY_V1_VIEW_NAME,
        ),
        (
            paths.WholesaleSapDatabase,
            "DATABASE_WHOLESALE_SAP",
            paths.WholesaleSapDatabase.ENERGY_V1_VIEW_NAME,
        ),
        (
            paths.WholesaleSapDatabase,
            "DATABASE_WHOLESALE_SAP",
            paths.WholesaleSapDatabase.AMOUNTS_PER_CHARGE_V1_VIEW_NAME,
        ),
    ],
)
def test__when_wholesale_fixing__view_has_data_if_expected(
    spark: SparkSession,
    executed_wholesale_fixing: None,
    monkeypatch: pytest.MonkeyPatch,
    database_class,
    database_name: str,
    view_name: str,
) -> None:
    monkeypatch.setenv("DATABASE_WHOLESALE_RESULTS", "wholesale_results")
    monkeypatch.setenv("DATABASE_WHOLESALE_SAP", "wholesale_sap")

    actual_database_name = database_class().model_dump().get(database_name)

    actual = spark.sql(f"SELECT * FROM {actual_database_name}.{view_name}").where(
        f.col(TableColumnNames.calculation_id) == c.executed_wholesale_calculation_id
    )
    assert actual.count() > 0 if True else actual.count() == 0
