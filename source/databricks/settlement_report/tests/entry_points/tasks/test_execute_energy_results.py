import pytest
from pyspark.sql import SparkSession

from data_seeding import standard_wholesale_fixing_scenario_data_generator
from dbutils_fixture import DBUtilsFixture

from assertion import assert_file_names_and_columns
from settlement_report_job.domain.utils.report_data_type import ReportDataType
from settlement_report_job.entry_points.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from settlement_report_job.domain.utils.csv_column_names import (
    CsvColumnNames,
)
from settlement_report_job.domain.utils.market_role import MarketRole
from settlement_report_job.entry_points.tasks.energy_resuls_task import (
    EnergyResultsTask,
)
from settlement_report_job.infrastructure.paths import get_report_output_path
from utils import get_actual_files, cleanup_output_path


@pytest.fixture(scope="function", autouse=True)
def reset_task_values(settlement_reports_output_path: str):
    yield
    cleanup_output_path(
        settlement_reports_output_path=settlement_reports_output_path,
    )


def test_execute_energy_results__when_standard_wholesale_fixing_scenario__returns_expected_number_of_files_and_content(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    standard_wholesale_fixing_scenario_args.requesting_actor_market_role = (
        MarketRole.DATAHUB_ADMINISTRATOR
    )
    standard_wholesale_fixing_scenario_args.energy_supplier_ids = ["1000000000000"]
    expected_columns = [
        CsvColumnNames.grid_area_code,
        CsvColumnNames.energy_supplier_id,
        CsvColumnNames.calculation_type,
        CsvColumnNames.time,
        CsvColumnNames.resolution,
        CsvColumnNames.metering_point_type,
        CsvColumnNames.settlement_method,
        CsvColumnNames.energy_quantity,
    ]

    expected_file_names = [
        "RESULTENERGY_804_1000000000000_02-01-2024_02-01-2024.csv",
        "RESULTENERGY_805_1000000000000_02-01-2024_02-01-2024.csv",
    ]
    task = EnergyResultsTask(spark, dbutils, standard_wholesale_fixing_scenario_args)

    # Act
    task.execute()

    # Assert
    actual_files = get_actual_files(
        report_data_type=ReportDataType.EnergyResults,
        args=standard_wholesale_fixing_scenario_args,
    )
    assert_file_names_and_columns(
        path=get_report_output_path(standard_wholesale_fixing_scenario_args),
        actual_files=actual_files,
        expected_columns=expected_columns,
        expected_file_names=expected_file_names,
        spark=spark,
    )


def test_execute_energy_results__when_split_report_by_grid_area_is_false__returns_expected_number_of_files_and_content(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    standard_wholesale_fixing_scenario_args.requesting_actor_market_role = (
        MarketRole.DATAHUB_ADMINISTRATOR
    )
    standard_wholesale_fixing_scenario_args.calculation_id_by_grid_area = {
        standard_wholesale_fixing_scenario_data_generator.GRID_AREAS[
            0
        ]: standard_wholesale_fixing_scenario_args.calculation_id_by_grid_area[
            standard_wholesale_fixing_scenario_data_generator.GRID_AREAS[0]
        ]
    }
    standard_wholesale_fixing_scenario_args.energy_supplier_ids = None
    standard_wholesale_fixing_scenario_args.split_report_by_grid_area = True
    expected_columns = [
        CsvColumnNames.grid_area_code,
        CsvColumnNames.energy_supplier_id,
        CsvColumnNames.calculation_type,
        CsvColumnNames.time,
        CsvColumnNames.resolution,
        CsvColumnNames.metering_point_type,
        CsvColumnNames.settlement_method,
        CsvColumnNames.energy_quantity,
    ]

    expected_file_names = [
        "RESULTENERGY_804_02-01-2024_02-01-2024.csv",
    ]
    task = EnergyResultsTask(spark, dbutils, standard_wholesale_fixing_scenario_args)

    # Act
    task.execute()

    # Assert
    actual_files = get_actual_files(
        report_data_type=ReportDataType.EnergyResults,
        args=standard_wholesale_fixing_scenario_args,
    )
    assert_file_names_and_columns(
        path=get_report_output_path(standard_wholesale_fixing_scenario_args),
        actual_files=actual_files,
        expected_columns=expected_columns,
        expected_file_names=expected_file_names,
        spark=spark,
    )


def test_execute_energy_results__when_standard_wholesale_fixing_scenario_grid_access__returns_expected_number_of_files_and_content(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    standard_wholesale_fixing_scenario_args.requesting_actor_market_role = (
        MarketRole.GRID_ACCESS_PROVIDER
    )
    standard_wholesale_fixing_scenario_args.requesting_actor_id = "1234567890123"

    expected_columns = [
        CsvColumnNames.grid_area_code,
        CsvColumnNames.calculation_type,
        CsvColumnNames.time,
        CsvColumnNames.resolution,
        CsvColumnNames.metering_point_type,
        CsvColumnNames.settlement_method,
        CsvColumnNames.energy_quantity,
    ]

    expected_file_names = [
        "RESULTENERGY_804_1234567890123_DDM_02-01-2024_02-01-2024.csv",
        "RESULTENERGY_805_1234567890123_DDM_02-01-2024_02-01-2024.csv",
    ]
    task = EnergyResultsTask(spark, dbutils, standard_wholesale_fixing_scenario_args)

    # Act
    task.execute()

    # Assert
    actual_files = get_actual_files(
        report_data_type=ReportDataType.EnergyResults,
        args=standard_wholesale_fixing_scenario_args,
    )
    assert_file_names_and_columns(
        path=get_report_output_path(standard_wholesale_fixing_scenario_args),
        actual_files=actual_files,
        expected_columns=expected_columns,
        expected_file_names=expected_file_names,
        spark=spark,
    )


def test_execute_energy_results__when_standard_wholesale_fixing_scenario_energy_supplier__returns_expected_number_of_files_and_content(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    standard_wholesale_fixing_scenario_args.requesting_actor_market_role = (
        MarketRole.ENERGY_SUPPLIER
    )
    standard_wholesale_fixing_scenario_args.requesting_actor_id = "1000000000000"
    standard_wholesale_fixing_scenario_args.energy_supplier_ids = ["1000000000000"]
    expected_columns = [
        CsvColumnNames.grid_area_code,
        CsvColumnNames.calculation_type,
        CsvColumnNames.time,
        CsvColumnNames.resolution,
        CsvColumnNames.metering_point_type,
        CsvColumnNames.settlement_method,
        CsvColumnNames.energy_quantity,
    ]

    expected_file_names = [
        "RESULTENERGY_804_1000000000000_DDQ_02-01-2024_02-01-2024.csv",
        "RESULTENERGY_805_1000000000000_DDQ_02-01-2024_02-01-2024.csv",
    ]
    task = EnergyResultsTask(spark, dbutils, standard_wholesale_fixing_scenario_args)

    # Act
    task.execute()

    # Assert
    actual_files = get_actual_files(
        report_data_type=ReportDataType.EnergyResults,
        args=standard_wholesale_fixing_scenario_args,
    )
    assert_file_names_and_columns(
        path=get_report_output_path(standard_wholesale_fixing_scenario_args),
        actual_files=actual_files,
        expected_columns=expected_columns,
        expected_file_names=expected_file_names,
        spark=spark,
    )
