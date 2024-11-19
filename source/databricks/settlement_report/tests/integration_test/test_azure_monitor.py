# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
import os
import sys
import time
import uuid
from datetime import timedelta
from typing import cast, Callable
from unittest.mock import Mock, patch

import pytest
from azure.monitor.query import LogsQueryClient, LogsQueryResult
from settlement_report_job.entry_points.job_args.calculation_type import CalculationType
from settlement_report_job.entry_points.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from settlement_report_job.entry_points.entry_point import (
    start_task_with_deps,
)
from settlement_report_job.entry_points.tasks.task_type import TaskType
from integration_test_configuration import IntegrationTestConfiguration


class TestWhenInvokedWithArguments:
    def test_add_info_log_record_to_azure_monitor_with_expected_settings(
        self,
        standard_wholesale_fixing_scenario_args: SettlementReportArgs,
        integration_test_configuration: IntegrationTestConfiguration,
    ) -> None:
        """
        Assert that the settlement report job adds log records to Azure Monitor with the expected settings:
        | where AppRoleName == "dbr-settlement-report"
        | where SeverityLevel == 1
        | where Message startswith_cs "Command line arguments"
        | where OperationId != "00000000000000000000000000000000"
        | where Properties.Subsystem == "wholesale-aggregations"
        - custom field "settlement_report_id" = <the settlement report id>
        - custom field "CategoryName" = "Energinet.DataHub." + <logger name>

        Debug level is not tested as it is not intended to be logged by default.
        """

        # Arrange
        valid_task_type = TaskType.TimeSeriesHourly
        standard_wholesale_fixing_scenario_args.report_id = str(uuid.uuid4())
        applicationinsights_connection_string = (
            integration_test_configuration.get_applicationinsights_connection_string()
        )
        os.environ["CATALOG_NAME"] = "test_catalog"
        task_factory_mock = Mock()
        self.prepare_command_line_arguments(standard_wholesale_fixing_scenario_args)

        # Act
        with patch(
            "settlement_report_job.entry_points.tasks.task_factory.create",
            task_factory_mock,
        ):
            with patch(
                "settlement_report_job.entry_points.tasks.time_series_points_task.TimeSeriesPointsTask.execute",
                return_value=None,
            ):
                start_task_with_deps(
                    task_type=valid_task_type,
                    applicationinsights_connection_string=applicationinsights_connection_string,
                )

        # Assert
        # noinspection PyTypeChecker
        logs_client = LogsQueryClient(integration_test_configuration.credential)

        query = f"""
        AppTraces
        | where AppRoleName == "dbr-settlement-report"
        | where SeverityLevel == 1
        | where Message startswith_cs "Command line arguments"
        | where OperationId != "00000000000000000000000000000000"
        | where Properties.Subsystem == "wholesale-aggregations"
        | where Properties.settlement_report_id == "{standard_wholesale_fixing_scenario_args.report_id}"
        | where Properties.CategoryName == "Energinet.DataHub.settlement_report_job.entry_points.job_args.settlement_report_job_args"
        | count
        """

        workspace_id = integration_test_configuration.get_analytics_workspace_id()

        def assert_logged():
            actual = logs_client.query_workspace(
                workspace_id, query, timespan=timedelta(minutes=5)
            )
            assert_row_count(actual, 1)

        # Assert, but timeout if not succeeded
        wait_for_condition(
            assert_logged, timeout=timedelta(minutes=3), step=timedelta(seconds=10)
        )

    def test_add_exception_log_record_to_azure_monitor_with_unexpected_settings(
        self,
        standard_wholesale_fixing_scenario_args: SettlementReportArgs,
        integration_test_configuration: IntegrationTestConfiguration,
    ) -> None:
        """
        Assert that the settlement report job adds log records to Azure Monitor with the expected settings:
        | where AppRoleName == "dbr-settlement-report"
        | where ExceptionType == "argparse.ArgumentTypeError"
        | where OuterMessage startswith_cs "Grid area codes must consist of 3 digits"
        | where OperationId != "00000000000000000000000000000000"
        | where Properties.Subsystem == "wholesale-aggregations"
        - custom field "settlement_report_id" = <the settlement report id>
        - custom field "CategoryName" = "Energinet.DataHub." + <logger name>

        Debug level is not tested as it is not intended to be logged by default.
        """

        # Arrange
        valid_task_type = TaskType.TimeSeriesHourly
        standard_wholesale_fixing_scenario_args.report_id = str(uuid.uuid4())
        standard_wholesale_fixing_scenario_args.calculation_type = (
            CalculationType.BALANCE_FIXING
        )
        standard_wholesale_fixing_scenario_args.grid_area_codes = [
            "8054"
        ]  # Should produce an error with balance fixing
        applicationinsights_connection_string = (
            integration_test_configuration.get_applicationinsights_connection_string()
        )
        os.environ["CATALOG_NAME"] = "test_catalog"
        task_factory_mock = Mock()
        self.prepare_command_line_arguments(standard_wholesale_fixing_scenario_args)

        # Act
        with pytest.raises(SystemExit):
            with patch(
                "settlement_report_job.entry_points.tasks.task_factory.create",
                task_factory_mock,
            ):
                with patch(
                    "settlement_report_job.entry_points.tasks.time_series_points_task.TimeSeriesPointsTask.execute",
                    return_value=None,
                ):
                    start_task_with_deps(
                        task_type=valid_task_type,
                        applicationinsights_connection_string=applicationinsights_connection_string,
                    )

        # Assert
        # noinspection PyTypeChecker
        logs_client = LogsQueryClient(integration_test_configuration.credential)

        query = f"""
        AppExceptions
        | where AppRoleName == "dbr-settlement-report"
        | where ExceptionType == "argparse.ArgumentTypeError"
        | where OuterMessage startswith_cs "Grid area codes must consist of 3 digits"
        | where OperationId != "00000000000000000000000000000000"
        | where Properties.Subsystem == "wholesale-aggregations"
        | where Properties.settlement_report_id == "{standard_wholesale_fixing_scenario_args.report_id}"
        | where Properties.CategoryName == "Energinet.DataHub.telemetry_logging.span_recording"
        | count
        """

        workspace_id = integration_test_configuration.get_analytics_workspace_id()

        def assert_logged():
            actual = logs_client.query_workspace(
                workspace_id, query, timespan=timedelta(minutes=5)
            )
            # There should be two counts, one from the arg_parser and one
            assert_row_count(actual, 1)

        # Assert, but timeout if not succeeded
        wait_for_condition(
            assert_logged, timeout=timedelta(minutes=3), step=timedelta(seconds=10)
        )

    @staticmethod
    def prepare_command_line_arguments(
        standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    ) -> None:
        standard_wholesale_fixing_scenario_args.report_id = str(
            uuid.uuid4()
        )  # Ensure unique report id
        sys.argv = []
        sys.argv.append(
            "--entry-point=execute_wholesale_results"
        )  # Workaround as the parse command line arguments starts with the second argument
        sys.argv.append(
            f"--report-id={str(standard_wholesale_fixing_scenario_args.report_id)}"
        )
        sys.argv.append(
            f"--period-start={str(standard_wholesale_fixing_scenario_args.period_start.strftime('%Y-%m-%dT%H:%M:%SZ'))}"
        )
        sys.argv.append(
            f"--period-end={str(standard_wholesale_fixing_scenario_args.period_end.strftime('%Y-%m-%dT%H:%M:%SZ'))}"
        )
        sys.argv.append(
            f"--calculation-type={str(standard_wholesale_fixing_scenario_args.calculation_type.value)}"
        )
        sys.argv.append("--requesting-actor-market-role=datahub_administrator")
        sys.argv.append("--requesting-actor-id=1234567890123")
        sys.argv.append(
            f"--grid-area-codes={str(standard_wholesale_fixing_scenario_args.grid_area_codes)}"
        )
        sys.argv.append(
            '--calculation-id-by-grid-area={"804": "bf6e1249-d4c2-4ec2-8ce5-4c7fe8756253"}'
        )


def wait_for_condition(callback: Callable, *, timeout: timedelta, step: timedelta):
    """
    Wait for a condition to be met, or timeout.
    The function keeps invoking the callback until it returns without raising an exception.
    """
    start_time = time.time()
    while True:
        elapsed_ms = int((time.time() - start_time) * 1000)
        # noinspection PyBroadException
        try:
            callback()
            print(f"Condition met in {elapsed_ms} ms")
            return
        except Exception:
            if elapsed_ms > timeout.total_seconds() * 1000:
                print(
                    f"Condition failed to be met before timeout. Timed out after {elapsed_ms} ms",
                    file=sys.stderr,
                )
                raise
            time.sleep(step.seconds)
            print(f"Condition not met after {elapsed_ms} ms. Retrying...")


def assert_row_count(actual, expected_count):
    actual = cast(LogsQueryResult, actual)
    table = actual.tables[0]
    row = table.rows[0]
    value = row["Count"]
    count = cast(int, value)
    assert count == expected_count
