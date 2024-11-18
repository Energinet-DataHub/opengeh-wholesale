from pyspark.sql import SparkSession
import pytest

from data_seeding import standard_wholesale_fixing_scenario_data_generator
from assertion import assert_file_names_and_columns

from dbutils_fixture import DBUtilsFixture
from settlement_report_job.domain.utils.market_role import MarketRole
from settlement_report_job.domain.utils.report_data_type import ReportDataType
from settlement_report_job.entry_points.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from settlement_report_job.domain.utils.csv_column_names import (
    CsvColumnNames,
)
from settlement_report_job.entry_points.tasks.metering_point_periods_task import (
    MeteringPointPeriodsTask,
)
from settlement_report_job.infrastructure.paths import get_report_output_path
from utils import get_start_date, get_end_date, cleanup_output_path, get_actual_files


@pytest.fixture(scope="function", autouse=True)
def reset_task_values(settlement_reports_output_path: str):
    yield
    cleanup_output_path(
        settlement_reports_output_path=settlement_reports_output_path,
    )


def _get_expected_columns(requesting_actor_market_role: MarketRole) -> list[str]:
    if requesting_actor_market_role == MarketRole.GRID_ACCESS_PROVIDER:
        return [
            CsvColumnNames.metering_point_id,
            CsvColumnNames.metering_point_from_date,
            CsvColumnNames.metering_point_to_date,
            CsvColumnNames.grid_area_code_in_metering_points_csv,
            CsvColumnNames.to_grid_area_code,
            CsvColumnNames.from_grid_area_code,
            CsvColumnNames.metering_point_type,
            CsvColumnNames.settlement_method,
        ]
    elif requesting_actor_market_role is MarketRole.ENERGY_SUPPLIER:
        return [
            CsvColumnNames.metering_point_id,
            CsvColumnNames.metering_point_from_date,
            CsvColumnNames.metering_point_to_date,
            CsvColumnNames.grid_area_code_in_metering_points_csv,
            CsvColumnNames.metering_point_type,
            CsvColumnNames.settlement_method,
        ]
    elif requesting_actor_market_role in [
        MarketRole.SYSTEM_OPERATOR,
        MarketRole.DATAHUB_ADMINISTRATOR,
    ]:
        return [
            CsvColumnNames.metering_point_id,
            CsvColumnNames.metering_point_from_date,
            CsvColumnNames.metering_point_to_date,
            CsvColumnNames.grid_area_code_in_metering_points_csv,
            CsvColumnNames.metering_point_type,
            CsvColumnNames.settlement_method,
            CsvColumnNames.energy_supplier_id,
        ]


def test_execute_metering_point_periods__when_energy_supplier__returns_expected(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_energy_supplier_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    args = standard_wholesale_fixing_scenario_energy_supplier_args
    start_time = get_start_date(args.period_start)
    end_time = get_end_date(args.period_end)
    grid_area_codes = list(args.calculation_id_by_grid_area.keys())
    grid_area_code_1 = grid_area_codes[0]
    grid_area_code_2 = grid_area_codes[1]
    expected_file_names = [
        f"MDMP_{grid_area_code_1}_{args.requesting_actor_id}_DDQ_{start_time}_{end_time}.csv",
        f"MDMP_{grid_area_code_2}_{args.requesting_actor_id}_DDQ_{start_time}_{end_time}.csv",
    ]
    expected_columns = _get_expected_columns(
        standard_wholesale_fixing_scenario_energy_supplier_args.requesting_actor_market_role
    )
    task = MeteringPointPeriodsTask(
        spark, dbutils, standard_wholesale_fixing_scenario_energy_supplier_args
    )

    # Act
    task.execute()

    # Assert
    actual_files = get_actual_files(
        report_data_type=ReportDataType.MeteringPointPeriods,
        args=standard_wholesale_fixing_scenario_energy_supplier_args,
    )
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
    args = standard_wholesale_fixing_scenario_grid_access_provider_args
    start_time = get_start_date(args.period_start)
    end_time = get_end_date(args.period_end)
    grid_area_codes = list(args.calculation_id_by_grid_area.keys())
    expected_file_names = [
        f"MDMP_{grid_area_codes[0]}_{args.requesting_actor_id}_DDM_{start_time}_{end_time}.csv",
        f"MDMP_{grid_area_codes[1]}_{args.requesting_actor_id}_DDM_{start_time}_{end_time}.csv",
    ]
    expected_columns = _get_expected_columns(
        standard_wholesale_fixing_scenario_grid_access_provider_args.requesting_actor_market_role
    )
    task = MeteringPointPeriodsTask(
        spark, dbutils, standard_wholesale_fixing_scenario_grid_access_provider_args
    )

    # Act
    task.execute()

    # Assert
    actual_files = get_actual_files(
        report_data_type=ReportDataType.MeteringPointPeriods,
        args=standard_wholesale_fixing_scenario_grid_access_provider_args,
    )
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
    start_time = get_start_date(args.period_start)
    end_time = get_end_date(args.period_end)
    grid_area_codes = list(args.calculation_id_by_grid_area.keys())
    expected_file_names = [
        f"MDMP_{grid_area_codes[0]}_{energy_supplier_id}_{start_time}_{end_time}.csv",
        f"MDMP_{grid_area_codes[1]}_{energy_supplier_id}_{start_time}_{end_time}.csv",
    ]
    expected_columns = _get_expected_columns(args.requesting_actor_market_role)
    task = MeteringPointPeriodsTask(spark, dbutils, args)

    # Act
    task.execute()

    # Assert
    actual_files = get_actual_files(
        report_data_type=ReportDataType.MeteringPointPeriods,
        args=standard_wholesale_fixing_scenario_args,
    )
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
    start_time = get_start_date(args.period_start)
    end_time = get_end_date(args.period_end)
    grid_area_codes = list(args.calculation_id_by_grid_area.keys())
    expected_file_names = [
        f"MDMP_{grid_area_codes[0]}_{start_time}_{end_time}.csv",
        f"MDMP_{grid_area_codes[1]}_{start_time}_{end_time}.csv",
    ]
    expected_columns = _get_expected_columns(
        standard_wholesale_fixing_scenario_args.requesting_actor_market_role
    )
    task = MeteringPointPeriodsTask(spark, dbutils, args)

    # Act
    task.execute()

    # Assert
    actual_files = get_actual_files(
        report_data_type=ReportDataType.MeteringPointPeriods,
        args=standard_wholesale_fixing_scenario_args,
    )
    assert_file_names_and_columns(
        path=get_report_output_path(args),
        actual_files=actual_files,
        expected_columns=expected_columns,
        expected_file_names=expected_file_names,
        spark=spark,
    )


def test_execute_metering_point_periods__when_balance_fixing__returns_expected(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_balance_fixing_scenario_args: SettlementReportArgs,
    standard_balance_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    args = standard_balance_fixing_scenario_args
    args.energy_supplier_ids = None
    start_time = get_start_date(args.period_start)
    end_time = get_end_date(args.period_end)
    expected_file_names = [
        f"MDMP_{args.grid_area_codes[0]}_{start_time}_{end_time}.csv",
        f"MDMP_{args.grid_area_codes[1]}_{start_time}_{end_time}.csv",
    ]
    expected_columns = _get_expected_columns(args.requesting_actor_market_role)
    task = MeteringPointPeriodsTask(spark, dbutils, args)

    # Act
    task.execute()

    # Assert
    actual_files = get_actual_files(
        report_data_type=ReportDataType.MeteringPointPeriods,
        args=args,
    )
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
    task = MeteringPointPeriodsTask(spark, dbutils, args)

    # Act
    task.execute()

    # Assert
    actual_files = get_actual_files(
        report_data_type=ReportDataType.MeteringPointPeriods,
        args=standard_wholesale_fixing_scenario_args,
    )
    assert actual_files is None or len(actual_files) == 0
