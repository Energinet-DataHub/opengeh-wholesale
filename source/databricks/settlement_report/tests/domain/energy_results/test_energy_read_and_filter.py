import uuid
from unittest.mock import Mock

import pytest
from pyspark.sql import SparkSession
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs
import test_factories.default_test_data_spec as default_data
import test_factories.energy_factory as energy_factory

from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.domain.energy_results.read_and_filter import (
    read_and_filter_from_view,
)
from settlement_report_job.wholesale.column_names import DataProductColumnNames

DEFAULT_FROM_DATE = default_data.DEFAULT_FROM_DATE
DEFAULT_TO_DATE = default_data.DEFAULT_TO_DATE
DATAHUB_ADMINISTRATOR_ID = "1234567890123"
SYSTEM_OPERATOR_ID = "3333333333333"
NOT_SYSTEM_OPERATOR_ID = "4444444444444"
DEFAULT_TIME_ZONE = "Europe/Copenhagen"
DEFAULT_CALCULATION_ID = "12345678-6f20-40c5-9a95-f419a1245d7e"


@pytest.fixture(scope="function")
def energy_read_and_filter_mock_repository(
    spark: SparkSession,
) -> Mock:
    mock_repository = Mock()

    df_energy_v1 = None
    df_energy_per_es_v1 = None

    # The inner-most loop generates 24 entries, so in total each view will contain:
    # 2 * 3 * 24 = 144 rows.
    for grid_area in ["804", "805"]:
        for energy_supplier_id in ["1000000000000", "2000000000000", "3000000000000"]:
            testing_spec = default_data.create_energy_results_data_spec(
                energy_supplier_id=energy_supplier_id,
                calculation_id=DEFAULT_CALCULATION_ID,
                grid_area_code=grid_area,
            )

            if df_energy_v1 is None:
                df_energy_v1 = energy_factory.create_energy_v1(spark, testing_spec)
            else:
                df_energy_v1 = df_energy_v1.union(
                    energy_factory.create_energy_v1(spark, testing_spec)
                )

            if df_energy_per_es_v1 is None:
                df_energy_per_es_v1 = energy_factory.create_energy_per_es_v1(
                    spark, testing_spec
                )
            else:
                df_energy_per_es_v1 = df_energy_per_es_v1.union(
                    energy_factory.create_energy_per_es_v1(spark, testing_spec)
                )

    mock_repository.read_energy.return_value = df_energy_v1
    mock_repository.read_energy_per_es.return_value = df_energy_per_es_v1

    return mock_repository


@pytest.mark.parametrize(
    "requesting_energy_supplier_id, contains_data",
    [("1000000000000", True), ("2000000000000", True), ("ID_WITH_NO_DATA", False)],
)
def test_read_and_filter_from_view__when_requesting_actor_is_energy_supplier__returns_results_only_for_that_energy_supplier(
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    energy_read_and_filter_mock_repository: Mock,
    requesting_energy_supplier_id: str,
    contains_data: bool,
) -> None:
    # Arrange
    standard_wholesale_fixing_scenario_args.requesting_actor_market_role = (
        MarketRole.ENERGY_SUPPLIER
    )
    standard_wholesale_fixing_scenario_args.requesting_actor_id = (
        requesting_energy_supplier_id
    )
    standard_wholesale_fixing_scenario_args.energy_supplier_ids = [
        requesting_energy_supplier_id
    ]

    expected_columns = (
        energy_read_and_filter_mock_repository.read_energy_per_es.return_value.columns
    )

    # Act
    actual_df = read_and_filter_from_view(
        args=standard_wholesale_fixing_scenario_args,
        repository=energy_read_and_filter_mock_repository,
    )

    # Assert
    for i, column in enumerate(actual_df.columns):
        assert expected_columns[i] == column

    assert (
        actual_df.filter(
            f"{DataProductColumnNames.energy_supplier_id} != '{requesting_energy_supplier_id}'"
        ).count()
        == 0
    )

    if contains_data:
        assert actual_df.count() > 0
    else:
        assert actual_df.count() == 0


@pytest.mark.parametrize(
    "energy_supplier_ids, contains_data",
    [
        (None, True),
        (["1000000000000"], True),
        (["2000000000000", "3000000000000"], True),
        ("'ID_WITH_NO_DATA'", False),
    ],
)
def test_read_and_filter_from_view__when_datahub_admin__returns_results_for_expected_energy_suppliers(
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    energy_read_and_filter_mock_repository: Mock,
    energy_supplier_ids: list[str] | None,
    contains_data: bool,
) -> None:
    # Arrange
    standard_wholesale_fixing_scenario_args.requesting_actor_market_role = (
        MarketRole.DATAHUB_ADMINISTRATOR
    )
    standard_wholesale_fixing_scenario_args.energy_supplier_ids = energy_supplier_ids

    expected_columns = (
        energy_read_and_filter_mock_repository.read_energy_per_es.return_value.columns
    )

    # Act
    actual_df = read_and_filter_from_view(
        args=standard_wholesale_fixing_scenario_args,
        repository=energy_read_and_filter_mock_repository,
    )

    # Assert
    for i, column in enumerate(actual_df.columns):
        assert expected_columns[i] == column

    if energy_supplier_ids is not None:
        energy_supplier_ids_as_string = ", ".join(energy_supplier_ids)
        assert (
            actual_df.filter(
                f"{DataProductColumnNames.energy_supplier_id} not in ({energy_supplier_ids_as_string})"
            ).count()
            == 0
        )

    if contains_data:
        assert actual_df.count() > 0
    else:
        assert actual_df.count() == 0


@pytest.mark.parametrize(
    "grid_area_code",
    [
        ("804"),
        ("805"),
    ],
)
def test_read_and_filter_from_view__when_grid_access_provider__returns_expected_results(
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    energy_read_and_filter_mock_repository: Mock,
    grid_area_code: str,
) -> None:
    # Arrange
    standard_wholesale_fixing_scenario_args.requesting_actor_market_role = (
        MarketRole.GRID_ACCESS_PROVIDER
    )
    standard_wholesale_fixing_scenario_args.energy_supplier_ids = None
    standard_wholesale_fixing_scenario_args.calculation_id_by_grid_area = {
        grid_area_code: uuid.UUID(DEFAULT_CALCULATION_ID),
    }

    expected_columns = (
        energy_read_and_filter_mock_repository.read_energy.return_value.columns
    )

    # Act
    actual_df = read_and_filter_from_view(
        args=standard_wholesale_fixing_scenario_args,
        repository=energy_read_and_filter_mock_repository,
    )

    # Assert
    for i, column in enumerate(actual_df.columns):
        assert expected_columns[i] == column

    assert (
        actual_df.filter(
            f"{DataProductColumnNames.grid_area_code} != '{grid_area_code}'"
        ).count()
        == 0
    )

    assert actual_df.count() > 0
