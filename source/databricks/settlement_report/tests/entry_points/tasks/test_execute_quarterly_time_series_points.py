from pyspark.sql import SparkSession
import pytest


from data_seeding import (
    standard_wholesale_fixing_scenario_data_generator,
    standard_balance_fixing_scenario_data_generator,
)
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
from settlement_report_job.entry_points.tasks.task_type import TaskType
from settlement_report_job.entry_points.tasks.time_series_points_task import (
    TimeSeriesPointsTask,
)
from settlement_report_job.infrastructure.paths import get_report_output_path
from utils import cleanup_output_path, get_actual_files


@pytest.fixture(scope="function", autouse=True)
def reset_task_values(settlement_reports_output_path: str):
    yield
    cleanup_output_path(
        settlement_reports_output_path=settlement_reports_output_path,
    )


def test_execute_quarterly_time_series_points__when_energy_supplier__returns_expected(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    args = standard_wholesale_fixing_scenario_args
    args.requesting_actor_market_role = MarketRole.ENERGY_SUPPLIER
    energy_supplier_id = (
        standard_wholesale_fixing_scenario_data_generator.ENERGY_SUPPLIER_IDS[0]
    )
    args.requesting_actor_id = energy_supplier_id
    args.energy_supplier_ids = [energy_supplier_id]
    expected_file_names = [
        f"TSSD15_804_{energy_supplier_id}_DDQ_02-01-2024_02-01-2024.csv",
        f"TSSD15_805_{energy_supplier_id}_DDQ_02-01-2024_02-01-2024.csv",
    ]
    expected_columns = [
        CsvColumnNames.metering_point_id,
        CsvColumnNames.metering_point_type,
        CsvColumnNames.time,
    ] + [f"ENERGYQUANTITY{i}" for i in range(1, 101)]
    task = TimeSeriesPointsTask(spark, dbutils, args, TaskType.TimeSeriesQuarterly)

    # Act
    task.execute()

    # Assert
    actual_files = get_actual_files(
        report_data_type=ReportDataType.TimeSeriesQuarterly,
        args=standard_wholesale_fixing_scenario_args,
    )
    assert_file_names_and_columns(
        path=get_report_output_path(args),
        actual_files=actual_files,
        expected_columns=expected_columns,
        expected_file_names=expected_file_names,
        spark=spark,
    )


def test_execute_quarterly_time_series_points__when_grid_access_provider__returns_expected(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    args = standard_wholesale_fixing_scenario_args
    args.requesting_actor_market_role = MarketRole.GRID_ACCESS_PROVIDER
    args.energy_supplier_ids = None
    expected_file_names = [
        f"TSSD15_804_{args.requesting_actor_id}_DDM_02-01-2024_02-01-2024.csv",
        f"TSSD15_805_{args.requesting_actor_id}_DDM_02-01-2024_02-01-2024.csv",
    ]
    expected_columns = [
        CsvColumnNames.metering_point_id,
        CsvColumnNames.metering_point_type,
        CsvColumnNames.time,
    ] + [f"ENERGYQUANTITY{i}" for i in range(1, 101)]
    task = TimeSeriesPointsTask(spark, dbutils, args, TaskType.TimeSeriesQuarterly)

    # Act
    task.execute()

    # Assert
    actual_files = get_actual_files(
        report_data_type=ReportDataType.TimeSeriesQuarterly,
        args=args,
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
def test_execute_quarterly_time_series_points__when_system_operator_or_datahub_admin_with_one_energy_supplier_id__returns_expected(
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
    expected_file_names = [
        f"TSSD15_804_{energy_supplier_id}_02-01-2024_02-01-2024.csv",
        f"TSSD15_805_{energy_supplier_id}_02-01-2024_02-01-2024.csv",
    ]
    expected_columns = [
        CsvColumnNames.energy_supplier_id,
        CsvColumnNames.metering_point_id,
        CsvColumnNames.metering_point_type,
        CsvColumnNames.time,
    ] + [f"ENERGYQUANTITY{i}" for i in range(1, 101)]
    task = TimeSeriesPointsTask(spark, dbutils, args, TaskType.TimeSeriesQuarterly)

    # Act
    task.execute()

    # Assert
    actual_files = get_actual_files(
        report_data_type=ReportDataType.TimeSeriesQuarterly,
        args=args,
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
def test_execute_quarterly_time_series_points__when_system_operator_or_datahub_admin_with_none_energy_supplier_id__returns_expected(
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
    expected_file_names = [
        "TSSD15_804_02-01-2024_02-01-2024.csv",
        "TSSD15_805_02-01-2024_02-01-2024.csv",
    ]
    expected_columns = [
        CsvColumnNames.energy_supplier_id,
        CsvColumnNames.metering_point_id,
        CsvColumnNames.metering_point_type,
        CsvColumnNames.time,
    ] + [f"ENERGYQUANTITY{i}" for i in range(1, 101)]
    task = TimeSeriesPointsTask(spark, dbutils, args, TaskType.TimeSeriesQuarterly)

    # Act
    task.execute()

    # Assert
    actual_files = get_actual_files(
        report_data_type=ReportDataType.TimeSeriesQuarterly,
        args=args,
    )
    assert_file_names_and_columns(
        path=get_report_output_path(args),
        actual_files=actual_files,
        expected_columns=expected_columns,
        expected_file_names=expected_file_names,
        spark=spark,
    )


def test_execute_quarterly_time_series_points__when_include_basis_data_false__returns_no_file_paths(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    args = standard_wholesale_fixing_scenario_args
    args.include_basis_data = False
    task = TimeSeriesPointsTask(spark, dbutils, args, TaskType.TimeSeriesQuarterly)

    # Act
    task.execute()

    # Assert
    actual_files = get_actual_files(
        report_data_type=ReportDataType.TimeSeriesQuarterly,
        args=args,
    )
    assert actual_files is None or len(actual_files) == 0


def test_execute_quarterly_time_series_points__when_energy_supplier_and_balance_fixing__returns_expected(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_balance_fixing_scenario_args: SettlementReportArgs,
    standard_balance_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    args = standard_balance_fixing_scenario_args
    args.requesting_actor_market_role = MarketRole.ENERGY_SUPPLIER
    energy_supplier_id = (
        standard_balance_fixing_scenario_data_generator.ENERGY_SUPPLIER_IDS[0]
    )
    args.requesting_actor_id = energy_supplier_id
    args.energy_supplier_ids = [energy_supplier_id]
    expected_file_names = [
        f"TSSD15_804_{energy_supplier_id}_DDQ_02-01-2024_02-01-2024.csv",
        f"TSSD15_805_{energy_supplier_id}_DDQ_02-01-2024_02-01-2024.csv",
    ]
    expected_columns = [
        CsvColumnNames.metering_point_id,
        CsvColumnNames.metering_point_type,
        CsvColumnNames.time,
    ] + [f"ENERGYQUANTITY{i}" for i in range(1, 101)]
    task = TimeSeriesPointsTask(spark, dbutils, args, TaskType.TimeSeriesQuarterly)

    # Act
    task.execute()

    # Assert
    actual_files = get_actual_files(
        report_data_type=ReportDataType.TimeSeriesQuarterly,
        args=args,
    )
    assert_file_names_and_columns(
        path=get_report_output_path(args),
        actual_files=actual_files,
        expected_columns=expected_columns,
        expected_file_names=expected_file_names,
        spark=spark,
    )
