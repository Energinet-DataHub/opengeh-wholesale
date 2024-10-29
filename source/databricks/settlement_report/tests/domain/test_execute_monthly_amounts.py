from pyspark.sql import SparkSession

from tests.data_seeding import standard_wholesale_fixing_scenario_data_generator
from tests.dbutils_fixture import DBUtilsFixture
from settlement_report_job.domain.report_generator import execute_monthly_amounts
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs
from settlement_report_job.domain.csv_column_names import (
    CsvColumnNames,
)
from settlement_report_job.domain.market_role import MarketRole


def reset_task_values(dbutils: DBUtilsFixture):
    try:
        dbutils.jobs.taskValues.set("monthly_amounts_files", [])
    except Exception:
        pass


def test_execute_monthly_amounts__when_standard_wholesale_fixing_scenario__returns_expected_number_of_files_and_content(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    try:
        # Arrange
        standard_wholesale_fixing_scenario_args.requesting_actor_market_role = (
            MarketRole.ENERGY_SUPPLIER
        )
        standard_wholesale_fixing_scenario_args.requesting_actor_id = "1000000000000"
        standard_wholesale_fixing_scenario_args.energy_supplier_ids = ["1000000000000"]

        expected_file_count = 2  # corresponding to the number of grid areas in standard_wholesale_fixing_scenario
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
            "RESULTMONTHLY_804_1000000000000_DDQ_02-01-2024_02-01-2024.csv",
            "RESULTMONTHLY_805_1000000000000_DDQ_02-01-2024_02-01-2024.csv",
        ]

        # Act
        execute_monthly_amounts(spark, dbutils, standard_wholesale_fixing_scenario_args)

        # Assert
        actual_files = dbutils.jobs.taskValues.get("monthly_amounts_files")
        assert len(actual_files) == expected_file_count
        for file_path in actual_files:
            df = spark.read.csv(file_path, header=True)
            assert df.count() > 0
            assert df.columns == expected_columns

        actual_file_names = [file.split("/")[-1] for file in actual_files]
        for actual_file_name in actual_file_names:
            assert actual_file_name in expected_file_names
    finally:
        reset_task_values(dbutils)


def test_execute_monthly_amounts__when_split_report_by_grid_area_is_false__returns_expected_number_of_files_and_content(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    try:
        standard_wholesale_fixing_scenario_args.requesting_actor_market_role = (
            MarketRole.ENERGY_SUPPLIER
        )
        standard_wholesale_fixing_scenario_args.requesting_actor_id = "1000000000000"
        standard_wholesale_fixing_scenario_args.energy_supplier_ids = ["1000000000000"]
        standard_wholesale_fixing_scenario_args.split_report_by_grid_area = False
        # Arrange
        expected_file_count = 1
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
            "RESULTMONTHLY_flere-net_1000000000000_DDQ_02-01-2024_02-01-2024.csv",
        ]

        # Act
        execute_monthly_amounts(spark, dbutils, standard_wholesale_fixing_scenario_args)

        # Assert
        actual_files = dbutils.jobs.taskValues.get("monthly_amounts_files")

        assert len(actual_files) == expected_file_count
        for file_path in actual_files:
            df = spark.read.csv(file_path, header=True)
            assert df.count() > 0
            assert df.columns == expected_columns

        actual_file_names = [file.split("/")[-1] for file in actual_files]
        for actual_file_name in actual_file_names:
            assert actual_file_name in expected_file_names

    finally:
        reset_task_values(dbutils)
