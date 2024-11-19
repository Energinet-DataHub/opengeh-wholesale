from datetime import timedelta
from functools import reduce
import uuid
from unittest.mock import Mock

import pytest
from pyspark.sql import SparkSession, functions as F
from settlement_report_job.entry_points.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from settlement_report_job.infrastructure.wholesale.data_values import (
    CalculationTypeDataProductValue,
)
from tests.test_factories import latest_calculations_factory
import tests.test_factories.default_test_data_spec as default_data
import tests.test_factories.energy_factory as energy_factory

from settlement_report_job.domain.utils.market_role import MarketRole
from settlement_report_job.domain.energy_results.read_and_filter import (
    read_and_filter_from_view,
)
from settlement_report_job.infrastructure.wholesale.column_names import (
    DataProductColumnNames,
)

DEFAULT_FROM_DATE = default_data.DEFAULT_FROM_DATE
DEFAULT_TO_DATE = default_data.DEFAULT_TO_DATE
DATAHUB_ADMINISTRATOR_ID = "1234567890123"
SYSTEM_OPERATOR_ID = "3333333333333"
NOT_SYSTEM_OPERATOR_ID = "4444444444444"
DEFAULT_TIME_ZONE = "Europe/Copenhagen"
DEFAULT_CALCULATION_ID = "12345678-6f20-40c5-9a95-f419a1245d7e"


@pytest.fixture(scope="session")
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
    assert expected_columns == actual_df.columns

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
        (["'ID_WITH_NO_DATA'"], False),
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
    assert expected_columns == actual_df.columns

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
    assert expected_columns == actual_df.columns

    number_of_rows_from_non_chosen_grid_areas = actual_df.filter(
        f"{DataProductColumnNames.grid_area_code} != '{grid_area_code}'"
    ).count()
    number_of_rows_returned = actual_df.count()

    assert number_of_rows_from_non_chosen_grid_areas == 0
    assert number_of_rows_returned > 0


def test_read_and_filter_from_view__when_balance_fixing__returns_only_rows_from_latest_calculations(
    spark: SparkSession,
    standard_balance_fixing_scenario_args: SettlementReportArgs,
) -> None:
    # Arrange
    standard_balance_fixing_scenario_args.requesting_actor_market_role = (
        MarketRole.DATAHUB_ADMINISTRATOR
    )
    standard_balance_fixing_scenario_args.grid_area_codes = ["804"]

    not_latest_calculation_id = "11111111-9fc8-409a-a169-fbd49479d718"
    latest_calculation_id = "22222222-9fc8-409a-a169-fbd49479d718"
    energy_df = reduce(
        lambda df1, df2: df1.union(df2),
        [
            energy_factory.create_energy_per_es_v1(
                spark,
                default_data.create_energy_results_data_spec(
                    calculation_id=calculation_id
                ),
            )
            for calculation_id in [latest_calculation_id, not_latest_calculation_id]
        ],
    )
    latest_calculations = latest_calculations_factory.create(
        spark,
        default_data.create_latest_calculations_per_day_row(
            calculation_id=latest_calculation_id,
            calculation_type=CalculationTypeDataProductValue.BALANCE_FIXING,
        ),
    )

    mock_repository = Mock()
    mock_repository.read_energy_per_es.return_value = energy_df
    mock_repository.read_latest_calculations.return_value = latest_calculations

    # Act
    actual_df = read_and_filter_from_view(
        args=standard_balance_fixing_scenario_args,
        repository=mock_repository,
    )

    # Assert
    actual_calculation_ids = (
        actual_df.select(DataProductColumnNames.calculation_id).distinct().collect()
    )
    assert len(actual_calculation_ids) == 1
    assert (
        actual_calculation_ids[0][DataProductColumnNames.calculation_id]
        == latest_calculation_id
    )


def test_read_and_filter_from_view__when_balance_fixing_with_two_calculations_with_time_overlap__returns_only_latest_calculation_data(
    spark: SparkSession,
    standard_balance_fixing_scenario_args: SettlementReportArgs,
) -> None:
    # Arrange
    standard_balance_fixing_scenario_args.requesting_actor_market_role = (
        MarketRole.DATAHUB_ADMINISTRATOR
    )
    day_1 = DEFAULT_FROM_DATE
    day_2 = day_1 + timedelta(days=1)
    day_3 = day_1 + timedelta(days=2)
    day_4 = day_1 + timedelta(days=3)  # exclusive
    calculation_id_1 = "11111111-9fc8-409a-a169-fbd49479d718"
    calculation_id_2 = "22222222-9fc8-409a-a169-fbd49479d718"
    calc_type = CalculationTypeDataProductValue.BALANCE_FIXING

    time_series_points = reduce(
        lambda df1, df2: df1.union(df2),
        [
            energy_factory.create_energy_per_es_v1(
                spark,
                default_data.create_energy_results_data_spec(
                    calculation_id=calc_id,
                    calculation_type=calc_type,
                    from_date=from_date,
                    to_date=to_date,
                ),
            )
            for calc_id, from_date, to_date in [
                (calculation_id_1, day_1, day_3),
                (calculation_id_2, day_2, day_4),
            ]
        ],
    )

    latest_calculations = latest_calculations_factory.create(
        spark,
        [
            default_data.create_latest_calculations_per_day_row(
                calculation_id=calc_id,
                calculation_type=calc_type,
                start_of_day=start_of_day,
            )
            for calc_id, start_of_day in [
                (calculation_id_1, day_1),
                (calculation_id_1, day_2),
                (calculation_id_2, day_3),
            ]
        ],
    )

    mock_repository = Mock()
    mock_repository.read_energy_per_es.return_value = time_series_points
    mock_repository.read_latest_calculations.return_value = latest_calculations

    standard_balance_fixing_scenario_args.period_start = day_1
    standard_balance_fixing_scenario_args.period_end = day_4
    standard_balance_fixing_scenario_args.grid_area_codes = [
        default_data.DEFAULT_GRID_AREA_CODE
    ]

    # Act
    actual_df = read_and_filter_from_view(
        args=standard_balance_fixing_scenario_args,
        repository=mock_repository,
    )

    # Assert

    for day, expected_calculation_id in zip(
        [day_1, day_2, day_3], [calculation_id_1, calculation_id_1, calculation_id_2]
    ):
        actual_calculation_ids = (
            actual_df.where(
                (F.col(DataProductColumnNames.time) >= day)
                & (F.col(DataProductColumnNames.time) < day + timedelta(days=1))
            )
            .select(DataProductColumnNames.calculation_id)
            .distinct()
            .collect()
        )
        assert len(actual_calculation_ids) == 1
        assert actual_calculation_ids[0][0] == expected_calculation_id


def test_read_and_filter_from_view__when_balance_fixing__latest_calculation_for_grid_area(
    spark: SparkSession, standard_balance_fixing_scenario_args: SettlementReportArgs
) -> None:
    # Arrange
    standard_balance_fixing_scenario_args.requesting_actor_market_role = (
        MarketRole.DATAHUB_ADMINISTRATOR
    )
    day_1 = DEFAULT_FROM_DATE
    day_2 = day_1 + timedelta(days=1)  # exclusive
    grid_area_1 = "805"
    grid_area_2 = "806"
    calculation_id_1 = "11111111-9fc8-409a-a169-fbd49479d718"
    calculation_id_2 = "22222222-9fc8-409a-a169-fbd49479d718"
    calc_type = CalculationTypeDataProductValue.BALANCE_FIXING

    energy_per_es = reduce(
        lambda df1, df2: df1.union(df2),
        [
            energy_factory.create_energy_per_es_v1(
                spark,
                default_data.create_energy_results_data_spec(
                    calculation_id=calc_id,
                    calculation_type=calc_type,
                    grid_area_code=grid_area,
                    from_date=day_1,
                    to_date=day_2,
                ),
            )
            for calc_id, grid_area in [
                (calculation_id_1, grid_area_1),
                (calculation_id_1, grid_area_2),
                (calculation_id_2, grid_area_2),
            ]
        ],
    )

    latest_calculations = latest_calculations_factory.create(
        spark,
        [
            default_data.create_latest_calculations_per_day_row(
                calculation_id=calculation_id_1,
                calculation_type=calc_type,
                grid_area_code=grid_area_1,
                start_of_day=day_1,
            ),
            default_data.create_latest_calculations_per_day_row(
                calculation_id=calculation_id_2,
                calculation_type=calc_type,
                grid_area_code=grid_area_2,
                start_of_day=day_1,
            ),
        ],
    )

    mock_repository = Mock()
    mock_repository.read_energy_per_es.return_value = energy_per_es
    mock_repository.read_latest_calculations.return_value = latest_calculations

    standard_balance_fixing_scenario_args.period_start = day_1
    standard_balance_fixing_scenario_args.period_end = day_2
    standard_balance_fixing_scenario_args.grid_area_codes = [grid_area_1, grid_area_2]

    # Act
    actual_df = read_and_filter_from_view(
        args=standard_balance_fixing_scenario_args,
        repository=mock_repository,
    )

    # Assert
    assert all(
        row[DataProductColumnNames.calculation_id] == calculation_id_1
        for row in actual_df.where(
            F.col(DataProductColumnNames.grid_area_code) == grid_area_1
        )
        .select(DataProductColumnNames.calculation_id)
        .distinct()
        .collect()
    )

    assert all(
        row[DataProductColumnNames.calculation_id] == calculation_id_2
        for row in actual_df.where(
            F.col(DataProductColumnNames.grid_area_code) == grid_area_2
        )
        .select(DataProductColumnNames.calculation_id)
        .distinct()
        .collect()
    )


def test_read_and_filter_from_view__when_balance_fixing__returns_only_balance_fixing_results(
    spark: SparkSession, standard_balance_fixing_scenario_args: SettlementReportArgs
) -> None:
    # Arrange
    calculation_id_and_type = {
        "11111111-9fc8-409a-a169-fbd49479d718": CalculationTypeDataProductValue.AGGREGATION,
        "22222222-9fc8-409a-a169-fbd49479d718": CalculationTypeDataProductValue.BALANCE_FIXING,
        "33333333-9fc8-409a-a169-fbd49479d718": CalculationTypeDataProductValue.WHOLESALE_FIXING,
        "44444444-9fc8-409a-a169-fbd49479d718": CalculationTypeDataProductValue.FIRST_CORRECTION_SETTLEMENT,
        "55555555-9fc8-409a-a169-fbd49479d718": CalculationTypeDataProductValue.SECOND_CORRECTION_SETTLEMENT,
        "66666666-9fc8-409a-a169-fbd49479d718": CalculationTypeDataProductValue.THIRD_CORRECTION_SETTLEMENT,
    }
    energy_per_es = reduce(
        lambda df1, df2: df1.union(df2),
        [
            energy_factory.create_energy_per_es_v1(
                spark,
                default_data.create_energy_results_data_spec(
                    calculation_id=calc_id, calculation_type=calc_type
                ),
            )
            for calc_id, calc_type in calculation_id_and_type.items()
        ],
    )

    latest_calculations = reduce(
        lambda df1, df2: df1.union(df2),
        [
            latest_calculations_factory.create(
                spark,
                default_data.create_latest_calculations_per_day_row(
                    calculation_id=calc_id, calculation_type=calc_type
                ),
            )
            for calc_id, calc_type in calculation_id_and_type.items()
        ],
    )

    mock_repository = Mock()
    mock_repository.read_energy_per_es.return_value = energy_per_es
    mock_repository.read_latest_calculations.return_value = latest_calculations

    standard_balance_fixing_scenario_args.requesting_actor_market_role = (
        MarketRole.DATAHUB_ADMINISTRATOR
    )
    standard_balance_fixing_scenario_args.grid_area_codes = [
        default_data.DEFAULT_GRID_AREA_CODE
    ]

    # Act
    actual_df = read_and_filter_from_view(
        args=standard_balance_fixing_scenario_args,
        repository=mock_repository,
    )

    # Assert
    actual_calculation_ids = (
        actual_df.select(DataProductColumnNames.calculation_id).distinct().collect()
    )
    assert len(actual_calculation_ids) == 1
    assert (
        actual_calculation_ids[0][DataProductColumnNames.calculation_id]
        == "22222222-9fc8-409a-a169-fbd49479d718"
    )
