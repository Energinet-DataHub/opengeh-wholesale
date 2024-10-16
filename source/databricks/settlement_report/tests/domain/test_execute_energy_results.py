from pyspark.sql import SparkSession
import pytest

from tests.fixtures import DBUtilsFixture
from settlement_report_job.domain.report_generator import execute_energy_results
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs
from settlement_report_job.domain.csv_column_names import (
    EnergyResultsCsvColumnNames,
)
from settlement_report_job.domain.market_role import MarketRole


def reset_task_values(dbutils: DBUtilsFixture):
    try:
        dbutils.jobs.taskValues.set("energy_result_files", [])
    except Exception:
        pass


def test_execute_energy_results__when_standard_wholesale_fixing_scenario__returns_expected_number_of_files_and_content(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    try:
        standard_wholesale_fixing_scenario_args.requesting_actor_market_role = (
            MarketRole.DATAHUB_ADMINISTRATOR
        )
        # Arrange
        expected_file_count = 2  # corresponding to the number of grid areas in standard_wholesale_fixing_scenario
        expected_columns = [
            # EnergyResultsCsvColumnNames.grid_area_code,
            EnergyResultsCsvColumnNames.calculation_type,
            EnergyResultsCsvColumnNames.time,
            EnergyResultsCsvColumnNames.resolution,
            EnergyResultsCsvColumnNames.metering_point_type,
            EnergyResultsCsvColumnNames.settlement_method,
            EnergyResultsCsvColumnNames.quantity,
        ]

        # Act
        execute_energy_results(spark, dbutils, standard_wholesale_fixing_scenario_args)

        # Assert
        actual_files = dbutils.jobs.taskValues.get("energy_result_files")
        assert len(actual_files) == expected_file_count
        for file_path in actual_files:
            df = spark.read.option("delimiter", ";").csv(file_path, header=True)
            assert df.count() > 0
            assert df.columns == expected_columns
    finally:
        reset_task_values(dbutils)
