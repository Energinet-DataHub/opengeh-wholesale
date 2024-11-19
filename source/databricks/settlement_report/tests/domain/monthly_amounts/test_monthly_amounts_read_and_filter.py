from unittest.mock import Mock

import pytest
from pyspark.sql import SparkSession, functions as F
from settlement_report_job.entry_points.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from data_seeding import (
    standard_wholesale_fixing_scenario_data_generator,
)
import test_factories.default_test_data_spec as default_data
import test_factories.monthly_amounts_per_charge_factory as monthly_amounts_per_charge_factory
import test_factories.total_monthly_amounts_factory as total_monthly_amounts_factory

from settlement_report_job.domain.utils.market_role import MarketRole
from settlement_report_job.domain.monthly_amounts.read_and_filter import (
    _filter_monthly_amounts_per_charge,
    read_and_filter_from_view,
)
from settlement_report_job.infrastructure.wholesale.column_names import (
    DataProductColumnNames,
)

DEFAULT_FROM_DATE = default_data.DEFAULT_FROM_DATE
DEFAULT_TO_DATE = default_data.DEFAULT_TO_DATE
DATAHUB_ADMINISTRATOR_ID = "1234567890123"
GRID_ACCESS_PROVIDER_ID = "5555555555555"
SYSTEM_OPERATOR_ID = "3333333333333"
DEFAULT_ENERGY_SUPPLIER_ID = "2222222222222"
NOT_SYSTEM_OPERATOR_ID = "4444444444444"
DEFAULT_TIME_ZONE = "Europe/Copenhagen"
DEFAULT_CALCULATION_ID = "12345678-6f20-40c5-9a95-f419a1245d7e"


@pytest.fixture(scope="session")
def monthly_amounts_read_and_filter_mock_repository(
    spark: SparkSession,
) -> Mock:
    mock_repository = Mock()

    monthly_amounts_per_charge = None
    total_monthly_amounts = None

    for grid_area in ["804", "805"]:
        for energy_supplier_id in [
            "1000000000000",
            DEFAULT_ENERGY_SUPPLIER_ID,
            "3000000000000",
        ]:
            for charge_owner_id in [
                DATAHUB_ADMINISTRATOR_ID,
                GRID_ACCESS_PROVIDER_ID,
                SYSTEM_OPERATOR_ID,
                energy_supplier_id,
                None,
            ]:
                for is_tax in [True, False]:
                    charge_owner_id_for_per_charge = (
                        energy_supplier_id
                        if charge_owner_id is None
                        else charge_owner_id
                    )

                    testing_spec_monthly_per_charge = (
                        default_data.create_monthly_amounts_per_charge_row(
                            energy_supplier_id=energy_supplier_id,
                            calculation_id=DEFAULT_CALCULATION_ID,
                            grid_area_code=grid_area,
                            charge_owner_id=charge_owner_id_for_per_charge,
                            is_tax=is_tax,
                        )
                    )
                    testing_spec_total_monthly = (
                        default_data.create_total_monthly_amounts_row(
                            energy_supplier_id=energy_supplier_id,
                            calculation_id=DEFAULT_CALCULATION_ID,
                            grid_area_code=grid_area,
                            charge_owner_id=charge_owner_id,
                        )
                    )

                    if monthly_amounts_per_charge is None:
                        monthly_amounts_per_charge = (
                            monthly_amounts_per_charge_factory.create(
                                spark, testing_spec_monthly_per_charge
                            )
                        )
                    else:
                        monthly_amounts_per_charge = monthly_amounts_per_charge.union(
                            monthly_amounts_per_charge_factory.create(
                                spark, testing_spec_monthly_per_charge
                            )
                        )

                    if total_monthly_amounts is None:
                        total_monthly_amounts = total_monthly_amounts_factory.create(
                            spark, testing_spec_total_monthly
                        )
                    else:
                        total_monthly_amounts = total_monthly_amounts.union(
                            total_monthly_amounts_factory.create(
                                spark, testing_spec_total_monthly
                            )
                        )

    mock_repository.read_monthly_amounts_per_charge_v1.return_value = (
        monthly_amounts_per_charge
    )
    mock_repository.read_total_monthly_amounts_v1.return_value = total_monthly_amounts

    return mock_repository


def get_expected_unordered_columns() -> list[str]:
    return [
        DataProductColumnNames.calculation_id,
        DataProductColumnNames.calculation_type,
        DataProductColumnNames.calculation_version,
        DataProductColumnNames.grid_area_code,
        DataProductColumnNames.energy_supplier_id,
        DataProductColumnNames.time,
        DataProductColumnNames.resolution,
        DataProductColumnNames.quantity_unit,
        DataProductColumnNames.currency,
        DataProductColumnNames.amount,
        DataProductColumnNames.charge_type,
        DataProductColumnNames.charge_code,
        DataProductColumnNames.is_tax,
        DataProductColumnNames.result_id,
        DataProductColumnNames.charge_owner_id,
    ]


def test_read_and_filter_from_view__returns_expected_columns(
    standard_wholesale_fixing_scenario_energy_supplier_args: SettlementReportArgs,
    monthly_amounts_read_and_filter_mock_repository: Mock,
) -> None:
    # Arrange
    expected_unordered_columns = get_expected_unordered_columns()

    # Act
    actual_df = read_and_filter_from_view(
        args=standard_wholesale_fixing_scenario_energy_supplier_args,
        repository=monthly_amounts_read_and_filter_mock_repository,
    )

    # Assert
    assert set(expected_unordered_columns) == set(actual_df.columns)


def test_read_and_filter_from_view__when_energy_supplier__returns_only_data_from_itself_but_all_charge_owners(
    standard_wholesale_fixing_scenario_energy_supplier_args: SettlementReportArgs,
    monthly_amounts_read_and_filter_mock_repository: Mock,
) -> None:
    # Arrange
    args = standard_wholesale_fixing_scenario_energy_supplier_args
    expected_unordered_columns = get_expected_unordered_columns()

    # Act
    actual_df = read_and_filter_from_view(
        args=args,
        repository=monthly_amounts_read_and_filter_mock_repository,
    )

    # Assert
    assert set(expected_unordered_columns) == set(actual_df.columns)
    assert (
        actual_df.where(
            F.col(DataProductColumnNames.energy_supplier_id).isin(
                [args.requesting_actor_id]
            )
        ).count()
        > 0
    )
    assert (
        actual_df.where(
            ~F.col(DataProductColumnNames.energy_supplier_id).isin(
                [args.requesting_actor_id]
            )
        ).count()
        == 0
    )
    assert (
        actual_df.select(F.col(DataProductColumnNames.charge_owner_id))
        .distinct()
        .count()
        > 1
    )


def test_read_and_filter_from_view__when_datahub_administrator__returns_all_suppliers_and_charge_owners(
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    monthly_amounts_read_and_filter_mock_repository: Mock,
) -> None:
    # Arrange
    standard_wholesale_fixing_scenario_args.requesting_actor_market_role = (
        MarketRole.DATAHUB_ADMINISTRATOR
    )
    standard_wholesale_fixing_scenario_args.requesting_actor_id = (
        DATAHUB_ADMINISTRATOR_ID
    )
    standard_wholesale_fixing_scenario_args.energy_supplier_ids = None
    expected_unordered_columns = get_expected_unordered_columns()

    # Act
    actual_df = read_and_filter_from_view(
        args=standard_wholesale_fixing_scenario_args,
        repository=monthly_amounts_read_and_filter_mock_repository,
    )

    # Assert
    assert set(expected_unordered_columns) == set(actual_df.columns)
    assert (
        actual_df.select(F.col(DataProductColumnNames.energy_supplier_id)).count() > 1
    )
    assert (
        actual_df.select(F.col(DataProductColumnNames.charge_owner_id))
        .distinct()
        .count()
        > 1
    )


@pytest.mark.parametrize(
    "requesting_actor_market_role,actor_id",
    [
        (MarketRole.GRID_ACCESS_PROVIDER, GRID_ACCESS_PROVIDER_ID),
        (MarketRole.SYSTEM_OPERATOR, SYSTEM_OPERATOR_ID),
    ],
)
def test_read_and_filter_from_view__when_grid_or_system_operator__returns_multiple_energy_suppliers(
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    monthly_amounts_read_and_filter_mock_repository: Mock,
    requesting_actor_market_role: MarketRole,
    actor_id: str,
) -> None:
    # Arrange
    standard_wholesale_fixing_scenario_args.requesting_actor_market_role = (
        requesting_actor_market_role
    )
    standard_wholesale_fixing_scenario_args.requesting_actor_id = actor_id
    standard_wholesale_fixing_scenario_args.energy_supplier_ids = None

    standard_wholesale_fixing_scenario_args.calculation_id_by_grid_area = dict(
        list(
            standard_wholesale_fixing_scenario_args.calculation_id_by_grid_area.items()
        )[:-1]
    )
    targeted_grid_area = list(
        standard_wholesale_fixing_scenario_args.calculation_id_by_grid_area
    )[0]

    expected_unordered_columns = get_expected_unordered_columns()

    # Act
    actual_df = read_and_filter_from_view(
        args=standard_wholesale_fixing_scenario_args,
        repository=monthly_amounts_read_and_filter_mock_repository,
    )

    # Assert
    assert set(expected_unordered_columns) == set(actual_df.columns)
    assert actual_df.count() > 0
    assert (
        actual_df.where(
            F.col(DataProductColumnNames.grid_area_code).isin([targeted_grid_area])
        ).count()
        > 0
    )
    assert (
        actual_df.where(
            ~F.col(DataProductColumnNames.grid_area_code).isin([targeted_grid_area])
        ).count()
        == 0
    )
    assert (
        actual_df.select(F.col(DataProductColumnNames.energy_supplier_id))
        .distinct()
        .count()
        > 1
    )


def test_filter_monthly_amounts_per_charge__when_grid_access_provider__returns_their_charges_and_correct_tax(
    spark: SparkSession,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
) -> None:
    # Arrange
    args = standard_wholesale_fixing_scenario_args
    args.requesting_actor_market_role = MarketRole.GRID_ACCESS_PROVIDER
    args.requesting_actor_id = GRID_ACCESS_PROVIDER_ID
    args.energy_supplier_ids = None
    targeted_grid_area = standard_wholesale_fixing_scenario_data_generator.GRID_AREAS[0]
    calc_id = standard_wholesale_fixing_scenario_data_generator.CALCULATION_ID

    passing_row_because_of_charge_owner_id = monthly_amounts_per_charge_factory.create(
        spark,
        default_data.create_monthly_amounts_per_charge_row(
            calculation_id=calc_id,
            is_tax=False,
            charge_owner_id=GRID_ACCESS_PROVIDER_ID,
        ),
    )
    passing_row_due_to_tax = monthly_amounts_per_charge_factory.create(
        spark,
        default_data.create_monthly_amounts_per_charge_row(
            calculation_id=calc_id,
            is_tax=True,
            charge_owner_id="Not our requesting actor",
        ),
    )
    failing_row_due_to_charge_owner = monthly_amounts_per_charge_factory.create(
        spark,
        default_data.create_monthly_amounts_per_charge_row(
            calculation_id=calc_id,
            is_tax=False,
            charge_owner_id="Not our requesting actor",
        ),
    )
    failing_row_due_to_grid_area = monthly_amounts_per_charge_factory.create(
        spark,
        default_data.create_monthly_amounts_per_charge_row(
            calculation_id=calc_id,
            grid_area_code="Not our grid area",
            is_tax=False,
            charge_owner_id=GRID_ACCESS_PROVIDER_ID,
        ),
    )
    testing_data = (
        passing_row_because_of_charge_owner_id.union(passing_row_due_to_tax)
        .union(failing_row_due_to_charge_owner)
        .union(failing_row_due_to_grid_area)
    )

    expected_count = 2

    # Act
    actual_df = _filter_monthly_amounts_per_charge(
        testing_data,
        args,
    )

    # Assert
    assert actual_df.count() == expected_count
    assert (
        actual_df.where(
            F.col(DataProductColumnNames.grid_area_code).isin([targeted_grid_area])
        ).count()
        == expected_count
    )
    assert actual_df.select(DataProductColumnNames.is_tax).distinct().count() == 2


def test_filter_monthly_amounts_per_charge__when_system_operator__returns_their_charges_and_correct_tax(
    spark: SparkSession,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
) -> None:
    # Arrange
    args = standard_wholesale_fixing_scenario_args
    args.requesting_actor_market_role = MarketRole.SYSTEM_OPERATOR
    args.requesting_actor_id = SYSTEM_OPERATOR_ID
    args.energy_supplier_ids = None
    targeted_grid_area = standard_wholesale_fixing_scenario_data_generator.GRID_AREAS[0]
    calc_id = standard_wholesale_fixing_scenario_data_generator.CALCULATION_ID

    passing_row = monthly_amounts_per_charge_factory.create(
        spark,
        default_data.create_monthly_amounts_per_charge_row(
            calculation_id=calc_id,
            is_tax=False,
            charge_owner_id=SYSTEM_OPERATOR_ID,
        ),
    )
    failing_row_due_to_tax = monthly_amounts_per_charge_factory.create(
        spark,
        default_data.create_monthly_amounts_per_charge_row(
            calculation_id=calc_id,
            is_tax=True,
            charge_owner_id=SYSTEM_OPERATOR_ID,
        ),
    )
    failing_row_due_to_charge_owner = monthly_amounts_per_charge_factory.create(
        spark,
        default_data.create_monthly_amounts_per_charge_row(
            calculation_id=calc_id,
            is_tax=False,
            charge_owner_id="Not our requesting actor",
        ),
    )
    failing_row_due_to_grid_area = monthly_amounts_per_charge_factory.create(
        spark,
        default_data.create_monthly_amounts_per_charge_row(
            calculation_id=calc_id,
            grid_area_code="Not our grid area",
            is_tax=False,
            charge_owner_id=SYSTEM_OPERATOR_ID,
        ),
    )
    testing_data = (
        passing_row.union(failing_row_due_to_tax)
        .union(failing_row_due_to_charge_owner)
        .union(failing_row_due_to_grid_area)
    )

    expected_count = 1

    # Act
    actual_df = _filter_monthly_amounts_per_charge(
        testing_data,
        args,
    )

    # Assert
    assert actual_df.count() == expected_count
    assert (
        actual_df.where(
            F.col(DataProductColumnNames.grid_area_code).isin([targeted_grid_area])
        ).count()
        == expected_count
    )
    assert (
        actual_df.filter(~F.col(DataProductColumnNames.is_tax)).count()
        == expected_count
    )
