from pyspark.sql import SparkSession

import pytest

from settlement_report_job.domain.utils.report_data_type import ReportDataType
from settlement_report_job.entry_points.tasks.monthly_amounts_task import (
    MonthlyAmountsTask,
)
from settlement_report_job.infrastructure.paths import get_report_output_path
from assertion import assert_file_names_and_columns
from dbutils_fixture import DBUtilsFixture
from settlement_report_job.entry_points.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from settlement_report_job.domain.utils.csv_column_names import (
    CsvColumnNames,
)
from data_seeding import standard_wholesale_fixing_scenario_data_generator
from utils import cleanup_output_path, get_actual_files


@pytest.fixture(scope="function", autouse=True)
def reset_task_values(settlement_reports_output_path: str):
    yield
    cleanup_output_path(
        settlement_reports_output_path=settlement_reports_output_path,
    )


def test_execute_monthly_amounts__when_standard_wholesale_fixing_scenario__returns_expected_number_of_files_and_content(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_energy_supplier_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    args = standard_wholesale_fixing_scenario_energy_supplier_args
    expected_columns = [
        CsvColumnNames.calculation_type,
        CsvColumnNames.correction_settlement_number,
        CsvColumnNames.grid_area_code,
        CsvColumnNames.energy_supplier_id,
        CsvColumnNames.time,
        CsvColumnNames.resolution,
        CsvColumnNames.quantity_unit,
        CsvColumnNames.currency,
        CsvColumnNames.amount,
        CsvColumnNames.charge_type,
        CsvColumnNames.charge_code,
        CsvColumnNames.charge_owner_id,
    ]

    expected_file_names = [
        f"RESULTMONTHLY_804_{args.requesting_actor_id}_DDQ_02-01-2024_02-01-2024.csv",
        f"RESULTMONTHLY_805_{args.requesting_actor_id}_DDQ_02-01-2024_02-01-2024.csv",
    ]

    task = MonthlyAmountsTask(spark, dbutils, args)

    # Act
    task.execute()

    # Assert
    actual_files = get_actual_files(
        report_data_type=ReportDataType.MonthlyAmounts,
        args=standard_wholesale_fixing_scenario_energy_supplier_args,
    )
    assert_file_names_and_columns(
        path=get_report_output_path(args),
        actual_files=actual_files,
        expected_columns=expected_columns,
        expected_file_names=expected_file_names,
        spark=spark,
    )


def test_execute_monthly_amounts__when_administrator_and_split_report_by_grid_area_is_false__returns_expected_number_of_files_and_content(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_datahub_admin_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    args = standard_wholesale_fixing_scenario_datahub_admin_args
    args.split_report_by_grid_area = False
    args.energy_supplier_ids = [
        standard_wholesale_fixing_scenario_data_generator.ENERGY_SUPPLIER_IDS[0]
    ]

    # Arrange
    expected_columns = [
        CsvColumnNames.calculation_type,
        CsvColumnNames.correction_settlement_number,
        CsvColumnNames.grid_area_code,
        CsvColumnNames.energy_supplier_id,
        CsvColumnNames.time,
        CsvColumnNames.resolution,
        CsvColumnNames.quantity_unit,
        CsvColumnNames.currency,
        CsvColumnNames.amount,
        CsvColumnNames.charge_type,
        CsvColumnNames.charge_code,
        CsvColumnNames.charge_owner_id,
    ]

    expected_file_names = [
        f"RESULTMONTHLY_flere-net_{args.energy_supplier_ids[0]}_02-01-2024_02-01-2024.csv",
    ]

    task = MonthlyAmountsTask(spark, dbutils, args)

    # Act
    task.execute()

    # Assert
    actual_files = get_actual_files(
        report_data_type=ReportDataType.MonthlyAmounts,
        args=args,
    )
    assert_file_names_and_columns(
        path=get_report_output_path(args),
        actual_files=actual_files,
        expected_columns=expected_columns,
        expected_file_names=expected_file_names,
        spark=spark,
    )


def test_execute_monthly_amounts__when_grid_access_provider__returns_expected_number_of_files_and_content(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_grid_access_provider_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    args = standard_wholesale_fixing_scenario_grid_access_provider_args

    # Get just one of the grid_areas of the dictionary.
    for key, value in args.calculation_id_by_grid_area.items():
        target_grid_area = key
        target_calc_id = value
        break
    args.calculation_id_by_grid_area = {target_grid_area: target_calc_id}

    # Arrange
    expected_columns = [
        CsvColumnNames.calculation_type,
        CsvColumnNames.correction_settlement_number,
        CsvColumnNames.grid_area_code,
        CsvColumnNames.energy_supplier_id,
        CsvColumnNames.time,
        CsvColumnNames.resolution,
        CsvColumnNames.quantity_unit,
        CsvColumnNames.currency,
        CsvColumnNames.amount,
        CsvColumnNames.charge_type,
        CsvColumnNames.charge_code,
        CsvColumnNames.charge_owner_id,
    ]

    expected_file_names = [
        f"RESULTMONTHLY_{target_grid_area}_{args.requesting_actor_id}_DDM_02-01-2024_02-01-2024.csv",
    ]

    task = MonthlyAmountsTask(spark, dbutils, args)

    # Act
    task.execute()

    # Assert
    actual_files = get_actual_files(
        report_data_type=ReportDataType.MonthlyAmounts,
        args=args,
    )
    assert_file_names_and_columns(
        path=get_report_output_path(args),
        actual_files=actual_files,
        expected_columns=expected_columns,
        expected_file_names=expected_file_names,
        spark=spark,
    )


def test_execute_monthly_amounts__when_system_operator__returns_expected_number_of_files_and_content(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_system_operator_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    args = standard_wholesale_fixing_scenario_system_operator_args

    # Get just one of the grid_areas of the dictionary.
    for key, value in args.calculation_id_by_grid_area.items():
        target_grid_area = key
        target_calc_id = value
        break
    args.calculation_id_by_grid_area = {target_grid_area: target_calc_id}

    # Arrange
    expected_columns = [
        CsvColumnNames.calculation_type,
        CsvColumnNames.correction_settlement_number,
        CsvColumnNames.grid_area_code,
        CsvColumnNames.energy_supplier_id,
        CsvColumnNames.time,
        CsvColumnNames.resolution,
        CsvColumnNames.quantity_unit,
        CsvColumnNames.currency,
        CsvColumnNames.amount,
        CsvColumnNames.charge_type,
        CsvColumnNames.charge_code,
        CsvColumnNames.charge_owner_id,
    ]

    expected_file_names = [
        f"RESULTMONTHLY_{target_grid_area}_02-01-2024_02-01-2024.csv",
    ]

    task = MonthlyAmountsTask(spark, dbutils, args)

    # Act
    task.execute()

    # Assert
    actual_files = get_actual_files(
        report_data_type=ReportDataType.MonthlyAmounts,
        args=args,
    )

    assert_file_names_and_columns(
        path=get_report_output_path(args),
        actual_files=actual_files,
        expected_columns=expected_columns,
        expected_file_names=expected_file_names,
        spark=spark,
    )
