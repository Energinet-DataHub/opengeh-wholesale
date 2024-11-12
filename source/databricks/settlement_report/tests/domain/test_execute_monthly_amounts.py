from pyspark.sql import SparkSession

import pytest
from settlement_report_job.infrastructure.paths import get_report_output_path
from tests.domain.assertion import assert_file_names_and_columns
from tests.dbutils_fixture import DBUtilsFixture
from settlement_report_job.domain.report_generator import ReportGenerator
from settlement_report_job.entry_points.job_args.settlement_report_args import SettlementReportArgs
from settlement_report_job.domain.csv_column_names import (
    CsvColumnNames,
)


@pytest.fixture(scope="function", autouse=True)
def reset_task_values(dbutils: DBUtilsFixture):
    yield
    print("Resetting task values")
    dbutils.jobs.taskValues.reset()


def test_execute_monthly_amounts__when_standard_wholesale_fixing_scenario__returns_expected_number_of_files_and_content(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_energy_supplier_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
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
        f"RESULTMONTHLY_804_{standard_wholesale_fixing_scenario_energy_supplier_args.requesting_actor_id}_DDQ_02-01-2024_02-01-2024.csv",
        f"RESULTMONTHLY_805_{standard_wholesale_fixing_scenario_energy_supplier_args.requesting_actor_id}_DDQ_02-01-2024_02-01-2024.csv",
    ]

    report_generator_instance = ReportGenerator(
        spark, dbutils, standard_wholesale_fixing_scenario_energy_supplier_args
    )

    # Act
    report_generator_instance.execute_monthly_amounts()

    # Assert
    actual_files = dbutils.jobs.taskValues.get("monthly_amounts_files")
    assert_file_names_and_columns(
        path=get_report_output_path(
            standard_wholesale_fixing_scenario_energy_supplier_args
        ),
        actual_files=actual_files,
        expected_columns=expected_columns,
        expected_file_names=expected_file_names,
        spark=spark,
    )


def test_execute_monthly_amounts__when_split_report_by_grid_area_is_false__returns_expected_number_of_files_and_content(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_energy_supplier_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    args = standard_wholesale_fixing_scenario_energy_supplier_args
    args.split_report_by_grid_area = False
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
        f"RESULTMONTHLY_flere-net_{args.requesting_actor_id}_DDQ_02-01-2024_02-01-2024.csv",
    ]

    report_generator_instance = ReportGenerator(
        spark, dbutils, standard_wholesale_fixing_scenario_energy_supplier_args
    )

    # Act
    report_generator_instance.execute_monthly_amounts()

    # Assert
    actual_files = dbutils.jobs.taskValues.get("monthly_amounts_files")
    assert_file_names_and_columns(
        path=get_report_output_path(
            standard_wholesale_fixing_scenario_energy_supplier_args
        ),
        actual_files=actual_files,
        expected_columns=expected_columns,
        expected_file_names=expected_file_names,
        spark=spark,
    )
