import uuid
from unittest.mock import Mock

from pyspark.sql import SparkSession, DataFrame
import test_factories.default_test_data_spec as default_data
import test_factories.charge_link_periods_factory as input_charge_link_periods_factory
import test_factories.metering_point_periods_factory as input_metering_point_periods_factory
import test_factories.charge_price_information_periods_factory as input_charge_price_information_periods_factory
from settlement_report_job.domain.metering_point_periods.metering_point_periods_factory import (
    create_metering_point_periods,
)
from settlement_report_job.entry_points.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from settlement_report_job.infrastructure.wholesale.data_values import (
    MeteringPointTypeDataProductValue,
    SettlementMethodDataProductValue,
)


def _get_repository_mock(
    metering_point_period: DataFrame,
    charge_link_periods: DataFrame | None = None,
    charge_price_information_periods: DataFrame | None = None,
) -> Mock:
    mock_repository = Mock()
    mock_repository.read_metering_point_periods.return_value = metering_point_period

    if charge_link_periods:
        mock_repository.read_charge_link_periods.return_value = charge_link_periods

    if charge_price_information_periods:
        mock_repository.read_charge_price_information_periods.return_value = (
            charge_price_information_periods
        )

    return mock_repository


def test_create_metering_point_periods__when_datahub_admin__returns_expected_values(
    spark: SparkSession,
    standard_wholesale_fixing_scenario_datahub_admin_args: SettlementReportArgs,
) -> None:
    # Arrange
    args = standard_wholesale_fixing_scenario_datahub_admin_args
    args.period_start = default_data.DEFAULT_PERIOD_START
    args.period_end = default_data.DEFAULT_PERIOD_END
    args.calculation_id_by_grid_area = {
        default_data.DEFAULT_GRID_AREA_CODE: uuid.UUID(
            default_data.DEFAULT_CALCULATION_ID
        )
    }
    expected = {
        "grid_area_code_partitioning": default_data.DEFAULT_GRID_AREA_CODE,
        "METERINGPOINTID": default_data.DEFAULT_METERING_POINT_ID,
        "VALIDFROM": default_data.DEFAULT_PERIOD_START,
        "VALIDTO": default_data.DEFAULT_PERIOD_END,
        "GRIDAREAID": default_data.DEFAULT_GRID_AREA_CODE,
        "TYPEOFMP": "E17",
        "SETTLEMENTMETHOD": "D01",
        "ENERGYSUPPLIERID": default_data.DEFAULT_ENERGY_SUPPLIER_ID,
    }
    metering_point_periods = input_metering_point_periods_factory.create(
        spark,
        default_data.create_metering_point_periods_row(
            metering_point_type=MeteringPointTypeDataProductValue.CONSUMPTION,
            settlement_method=SettlementMethodDataProductValue.FLEX,
        ),
    )
    mock_repository = _get_repository_mock(metering_point_periods)

    # Act
    actual = create_metering_point_periods(
        args=args,
        repository=mock_repository,
    )

    # Assert
    print(actual.collect())
    print(expected)
    assert actual.count() == 1
    assert actual.collect()[0].asDict() == expected


def test_create_metering_point_periods_for_wholesale__when_system_operator__returns_expected_columns(
    spark: SparkSession,
    standard_wholesale_fixing_scenario_system_operator_args: SettlementReportArgs,
) -> None:
    # Arrange
    expected_columns = [
        "grid_area_code_partitioning",
        "METERINGPOINTID",
        "VALIDFROM",
        "VALIDTO",
        "GRIDAREAID",
        "TYPEOFMP",
        "SETTLEMENTMETHOD",
        "ENERGYSUPPLIERID",
    ]
    metering_point_periods = input_metering_point_periods_factory.create(
        spark,
        default_data.create_metering_point_periods_row(),
    )
    charge_price_information_periods = input_charge_price_information_periods_factory.create(
        spark,
        default_data.create_charge_price_information_periods_row(
            charge_owner_id=standard_wholesale_fixing_scenario_system_operator_args.requesting_actor_id,
            is_tax=False,
        ),
    )
    charge_link_periods = input_charge_link_periods_factory.create(
        spark,
        default_data.create_charge_link_periods_row(
            charge_owner_id=standard_wholesale_fixing_scenario_system_operator_args.requesting_actor_id
        ),
    )
    mock_repository = _get_repository_mock(
        metering_point_periods, charge_link_periods, charge_price_information_periods
    )

    # Act
    actual = create_metering_point_periods(
        args=standard_wholesale_fixing_scenario_system_operator_args,
        repository=mock_repository,
    )

    # Assert
    assert actual.columns == expected_columns


def test_create_metering_point_periods__when_energy_supplier__returns_expected_columns(
    spark: SparkSession,
    standard_wholesale_fixing_scenario_energy_supplier_args: SettlementReportArgs,
) -> None:

    # Arrange
    expected_columns = [
        "grid_area_code_partitioning",
        "METERINGPOINTID",
        "VALIDFROM",
        "VALIDTO",
        "GRIDAREAID",
        "TYPEOFMP",
        "SETTLEMENTMETHOD",
    ]

    metering_point_periods = input_metering_point_periods_factory.create(
        spark,
        default_data.create_metering_point_periods_row(),
    )
    mock_repository = _get_repository_mock(metering_point_periods)

    # Act
    actual = create_metering_point_periods(
        args=standard_wholesale_fixing_scenario_energy_supplier_args,
        repository=mock_repository,
    )

    # Assert
    assert actual.columns == expected_columns


def test_create_metering_point_periods__when_grid_access_provider__returns_expected_columns(
    spark: SparkSession,
    standard_wholesale_fixing_scenario_grid_access_provider_args: SettlementReportArgs,
) -> None:

    # Arrange
    expected_columns = [
        "grid_area_code_partitioning",
        "METERINGPOINTID",
        "VALIDFROM",
        "VALIDTO",
        "GRIDAREAID",
        "TOGRIDAREAID",
        "FROMGRIDAREAID",
        "TYPEOFMP",
        "SETTLEMENTMETHOD",
    ]

    metering_point_periods = input_metering_point_periods_factory.create(
        spark,
        default_data.create_metering_point_periods_row(),
    )
    mock_repository = _get_repository_mock(metering_point_periods)

    # Act
    actual = create_metering_point_periods(
        args=standard_wholesale_fixing_scenario_grid_access_provider_args,
        repository=mock_repository,
    )

    # Assert
    assert actual.columns == expected_columns
