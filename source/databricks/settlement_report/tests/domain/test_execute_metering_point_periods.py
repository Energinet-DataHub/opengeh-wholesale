from pyspark.sql import SparkSession
import pytest

from tests.dbutils_fixture import DBUtilsFixture

from tests.data_seeding import standard_wholesale_fixing_scenario_data_generator
from tests.domain.assertion import assert_file_names_and_columns
from settlement_report_job.domain.market_role import MarketRole
import settlement_report_job.domain.report_generator as report_generator
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs
from settlement_report_job.domain.csv_column_names import (
    CsvColumnNames,
)
from settlement_report_job.infrastructure.paths import get_report_output_path
from utils import get_market_role_in_file_name, get_start_date, get_end_date


@pytest.fixture(scope="function", autouse=True)
def reset_task_values(dbutils: DBUtilsFixture):
    yield
    print("Resetting task values")
    dbutils.jobs.taskValues.reset()


def get_expected_filenmaes(args: SettlementReportArgs) -> list[str]:
    market_role_in_file_name = get_market_role_in_file_name(
        args.requesting_actor_market_role
    )
    start_time = get_start_date(args.period_start)
    end_time = get_end_date(args.period_end)
    grid_area_codes = list(args.calculation_id_by_grid_area.keys())
    grid_area_code_1 = grid_area_codes[0]
    grid_area_code_2 = grid_area_codes[1]
    energy_supplier_id = args.energy_supplier_ids[0]

    expected_file_names = [
        f"MDMP_{grid_area_code_1}_{energy_supplier_id}_{market_role_in_file_name}_{start_time}_{end_time}.csv",
        f"MDMP_{grid_area_code_2}_{energy_supplier_id}_{market_role_in_file_name}_{start_time}_{end_time}.csv",
    ]
    return expected_file_names


def _get_expected_columns(requesting_actor_market_role: MarketRole) -> list[str]:
    expected_column_names = [
        CsvColumnNames.metering_point_id,
        CsvColumnNames.metering_point_from_date,
        CsvColumnNames.metering_point_to_date,
        CsvColumnNames.grid_area_code_in_metering_points_csv,
        CsvColumnNames.metering_point_type,
        CsvColumnNames.settlement_method,
    ]

    if requesting_actor_market_role in [
        MarketRole.SYSTEM_OPERATOR,
        MarketRole.DATAHUB_ADMINISTRATOR,
    ]:
        expected_column_names.append(CsvColumnNames.energy_supplier_id)

    return expected_column_names


def test_execute_metering_point_periods__when_energy_supplier__returns_expected(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_energy_supplier_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    expected_file_names = get_expected_filenmaes(
        standard_wholesale_fixing_scenario_energy_supplier_args
    )
    expected_columns = _get_expected_columns(
        standard_wholesale_fixing_scenario_energy_supplier_args.requesting_actor_market_role
    )
    report_generator_instance = report_generator.ReportGenerator(
        spark, dbutils, standard_wholesale_fixing_scenario_energy_supplier_args
    )

    # Act
    report_generator_instance.execute_metering_point_periods()

    # Assert
    actual_files = dbutils.jobs.taskValues.get(key="metering_point_periods_files")
    assert_file_names_and_columns(
        path=get_report_output_path(
            standard_wholesale_fixing_scenario_energy_supplier_args
        ),
        actual_files=actual_files,
        expected_columns=expected_columns,
        expected_file_names=expected_file_names,
        spark=spark,
    )


def test_execute_metering_point_periods__when_grid_access_provider__returns_expected(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_grid_access_provider_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    expected_file_names = get_expected_filenmaes(
        standard_wholesale_fixing_scenario_grid_access_provider_args
    )
    expected_columns = _get_expected_columns(
        standard_wholesale_fixing_scenario_grid_access_provider_args.requesting_actor_market_role
    )
    report_generator_instance = report_generator.ReportGenerator(
        spark, dbutils, standard_wholesale_fixing_scenario_grid_access_provider_args
    )

    # Act
    report_generator_instance.execute_metering_point_periods()

    # Assert
    actual_files = dbutils.jobs.taskValues.get("metering_point_periods_files")
    assert_file_names_and_columns(
        path=get_report_output_path(
            standard_wholesale_fixing_scenario_grid_access_provider_args
        ),
        actual_files=actual_files,
        expected_columns=expected_columns,
        expected_file_names=expected_file_names,
        spark=spark,
    )


@pytest.mark.parametrize(
    "market_role",
    [MarketRole.SYSTEM_OPERATOR, MarketRole.DATAHUB_ADMINISTRATOR],
)
def test_execute_metering_point_periods__when_system_operator_or_datahub_admin_with_one_energy_supplier_id__returns_expected(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
    market_role: MarketRole,
):
    # Arrange
    args = standard_wholesale_fixing_scenario_args
    args.requesting_actor_market_role = market_role
    energy_supplier_id = (
        standard_wholesale_fixing_scenario_data_generator.ENERGY_SUPPLIER_IDS[0]
    )
    args.energy_supplier_ids = [energy_supplier_id]
    expected_file_names = get_expected_filenmaes(args)
    expected_columns = _get_expected_columns(
        standard_wholesale_fixing_scenario_data_generator.requesting_actor_market_role
    )
    report_generator_instance = report_generator.ReportGenerator(spark, dbutils, args)

    # Act
    report_generator_instance.execute_metering_point_periods()

    # Assert
    actual_files = dbutils.jobs.taskValues.get("metering_point_periods_files")
    assert_file_names_and_columns(
        path=get_report_output_path(args),
        actual_files=actual_files,
        expected_columns=expected_columns,
        expected_file_names=expected_file_names,
        spark=spark,
    )


@pytest.mark.parametrize(
    "market_role",
    [MarketRole.SYSTEM_OPERATOR, MarketRole.DATAHUB_ADMINISTRATOR],
)
def test_execute_metering_point_periods__when_system_operator_or_datahub_admin_with_none_energy_supplier_id__returns_expected(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
    market_role: MarketRole,
):
    # Arrange
    args = standard_wholesale_fixing_scenario_args
    args.requesting_actor_market_role = market_role
    args.energy_supplier_ids = None
    expected_file_names = get_expected_filenmaes(args)
    expected_columns = _get_expected_columns(
        standard_wholesale_fixing_scenario_args.requesting_actor_market_role
    )
    report_generator_instance = report_generator.ReportGenerator(spark, dbutils, args)

    # Act
    report_generator_instance.execute_metering_point_periods()

    # Assert
    actual_files = dbutils.jobs.taskValues.get("metering_point_periods_files")
    assert_file_names_and_columns(
        path=get_report_output_path(args),
        actual_files=actual_files,
        expected_columns=expected_columns,
        expected_file_names=expected_file_names,
        spark=spark,
    )


def test_execute_metering_point_periods__when_include_basis_data_false__returns_no_file_paths(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    args = standard_wholesale_fixing_scenario_args
    args.include_basis_data = False
    report_generator_instance = report_generator.ReportGenerator(spark, dbutils, args)

    # Act
    report_generator_instance.execute_metering_point_periods()

    # Assert
    actual_files = dbutils.jobs.taskValues.get("metering_point_periods_files")
    assert actual_files is None or len(actual_files) == 0
