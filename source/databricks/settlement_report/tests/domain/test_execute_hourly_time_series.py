import pytest
from pyspark.sql import SparkSession

from tests.fixtures import DBUtilsFixture
from settlement_report_job.domain.report_generator import execute_hourly_time_series
from settlement_report_job.domain.settlement_report_args import SettlementReportArgs
from settlement_report_job.domain.csv_column_names import (
    CsvColumnNames,
)

# NOTE: The tests in test_execute_quarterly_time_series.py should cover execute_hourly also, so we don't need to test
# all the same things again here also.


@pytest.fixture(scope="function", autouse=True)
def reset_task_values(dbutils: DBUtilsFixture):
    print("Resetting task values before test")
    yield
    print("Resetting task values")
    dbutils.jobs.taskValues.reset()


def test_execute_hourly_time_series__when_standard_wholesale_fixing_scenario__returns_expected_number_of_files_and_content(
    spark: SparkSession,
    dbutils: DBUtilsFixture,
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    standard_wholesale_fixing_scenario_data_written_to_delta: None,
):
    # Arrange
    expected_file_names = [
        "TSSD60_804_02-01-2024_02-01-2024.csv",
        "TSSD60_805_02-01-2024_02-01-2024.csv",
    ]
    expected_columns = [
        CsvColumnNames.energy_supplier_id,
        CsvColumnNames.metering_point_id,
        CsvColumnNames.metering_point_type,
        CsvColumnNames.start_of_day,
    ] + [f"ENERGYQUANTITY{i}" for i in range(1, 26)]

    # Act
    execute_hourly_time_series(spark, dbutils, standard_wholesale_fixing_scenario_args)

    # Assert
    actual_files = dbutils.jobs.taskValues.get("hourly_time_series_files")
    assert len(actual_files) == len(expected_file_names)
    for file_path in actual_files:
        df = spark.read.option("delimiter", ";").csv(file_path, header=True)
        assert df.count() > 0
        assert df.columns == expected_columns
        assert any(file_name in file_path for file_name in expected_file_names)
