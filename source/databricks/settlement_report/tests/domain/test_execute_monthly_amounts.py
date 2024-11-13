from pyspark.sql import SparkSession

import pytest
from settlement_report_job.infrastructure.paths import get_report_output_path
from tests.domain.assertion import assert_file_names_and_columns
from tests.dbutils_fixture import DBUtilsFixture
from settlement_report_job.domain.report_generator import ReportGenerator
from settlement_report_job.entry_points.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from settlement_report_job.domain.utils.csv_column_names import (
    CsvColumnNames,
)
from settlement_report_job.domain.utils.market_role import MarketRole


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

    report_generator_instance = ReportGenerator(spark, dbutils, args)

    # Act
    report_generator_instance.execute_monthly_amounts()

    # Assert
    actual_files = dbutils.jobs.taskValues.get("monthly_amounts_files")
    assert_file_names_and_columns(
        path=get_report_output_path(args),
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

    report_generator_instance = ReportGenerator(spark, dbutils, args)

    # Act
    report_generator_instance.execute_monthly_amounts()

    # Assert
    actual_files = dbutils.jobs.taskValues.get("monthly_amounts_files")
    assert_file_names_and_columns(
        path=get_report_output_path(args),
        actual_files=actual_files,
        expected_columns=expected_columns,
        expected_file_names=expected_file_names,
        spark=spark,
    )


@pytest.mark.parametrize(
    "requesting_market_role",
    [MarketRole.GRID_ACCESS_PROVIDER, MarketRole.SYSTEM_OPERATOR],
)
def test_execute_monthly_amounts__when_grid_or_syo__returns_expected_number_of_files_and_content(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_grid_access_provider_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
    requesting_market_role: MarketRole,
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
    ]

    file_name_id = (
        "DDM_" if requesting_market_role == MarketRole.GRID_ACCESS_PROVIDER else "_"
    )

    expected_file_names = [
        f"RESULTMONTHLY_{target_grid_area}_{args.requesting_actor_id}_{file_name_id}02-01-2024_02-01-2024.csv",
    ]

    report_generator_instance = ReportGenerator(spark, dbutils, args)

    # Act
    report_generator_instance.execute_monthly_amounts()

    # Assert
    actual_files = dbutils.jobs.taskValues.get("monthly_amounts_files")
    assert_file_names_and_columns(
        path=get_report_output_path(args),
        actual_files=actual_files,
        expected_columns=expected_columns,
        expected_file_names=expected_file_names,
        spark=spark,
    )
