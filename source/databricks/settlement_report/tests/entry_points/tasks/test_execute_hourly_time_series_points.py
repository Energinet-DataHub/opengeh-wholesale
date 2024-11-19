import pytest
from pyspark.sql import SparkSession

from dbutils_fixture import DBUtilsFixture

from assertion import assert_file_names_and_columns
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


# NOTE: The tests in test_execute_quarterly_time_series_points.py should cover execute_hourly also, so we don't need to test
# all the same things again here also.


@pytest.fixture(scope="function", autouse=True)
def reset_task_values(settlement_reports_output_path: str):
    yield
    cleanup_output_path(
        settlement_reports_output_path=settlement_reports_output_path,
    )


def test_execute_hourly_time_series_points__when_standard_wholesale_fixing_scenario__returns_expected_number_of_files_and_content(
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
        CsvColumnNames.time,
    ] + [f"ENERGYQUANTITY{i}" for i in range(1, 26)]
    task = TimeSeriesPointsTask(
        spark,
        dbutils,
        standard_wholesale_fixing_scenario_args,
        TaskType.TimeSeriesHourly,
    )

    # Act
    task.execute()

    # Assert
    actual_files = get_actual_files(
        report_data_type=ReportDataType.TimeSeriesHourly,
        args=standard_wholesale_fixing_scenario_args,
    )
    assert_file_names_and_columns(
        path=get_report_output_path(standard_wholesale_fixing_scenario_args),
        actual_files=actual_files,
        expected_columns=expected_columns,
        expected_file_names=expected_file_names,
        spark=spark,
    )
