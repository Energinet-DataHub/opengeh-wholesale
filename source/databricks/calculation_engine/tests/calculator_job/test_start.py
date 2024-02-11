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
import argparse
import sys
import time
import uuid
import pytest
from datetime import timedelta
from typing import cast, Callable
from azure.monitor.query import LogsQueryClient, LogsQueryResult

from package.calculation.calculator_args import CalculatorArgs
from package.calculator_job import start, start_with_deps

from tests.integration_test_configuration import IntegrationTestConfiguration


class TestWhenInvokedWithInvalidArguments:
    def test_exits_with_code_2(self):
        """The exit code 2 originates from the argparse library."""
        with pytest.raises(SystemExit) as system_exit:
            start()

        assert system_exit.value.code == 2


class TestWhenInvokedWithValidArguments:
    def test_does_not_raise(self, any_calculator_args):
        command_line_args = argparse.Namespace()
        command_line_args.calculation_id = any_calculator_args.calculation_id

        start_with_deps(
            parse_command_line_args=lambda: command_line_args,
            create_calculation_args=lambda args: any_calculator_args,
            calculation_executor=lambda args, reader: None,
            is_storage_locked_checker=lambda name, cred: False,
        )

    def test_add_info_log_record_to_azure_monitor_with_expected_settings(
        self,
        any_calculator_args: CalculatorArgs,
        integration_test_configuration: IntegrationTestConfiguration,
    ):
        """
        Assert that the calculator job adds log records to Azure Monitor with the expected settings:
        - cloud role name = "dbr-calculation-engine"
        - severity level = 1
        - message <the message>
        - operation id has value
        - custom field "Subsystem" = "wholesale"
        - custom field "calculation_id" = <the calculation id>
        - custom field "CategoryName" = "Energinet.DataHub." + <logger name>

        Debug level is not tested as it is not intended to be logged by default.
        """

        # Arrange
        self.prepare_command_line_arguments(any_calculator_args)

        # Act
        with pytest.raises(SystemExit):
            start_with_deps(
                applicationinsights_connection_string=integration_test_configuration
                .get_applicationinsights_connection_string(),
            )

        # Assert
        # noinspection PyTypeChecker
        logs_client = LogsQueryClient(integration_test_configuration.credential)

        query = f"""
AppTraces
| where AppRoleName == "dbr-calculation-engine"
| where SeverityLevel == 1
| where Message startswith_cs "Command line arguments"
| where OperationId != "00000000000000000000000000000000"
| where Properties.Subsystem == "wholesale"
| where Properties.calculation_id == "{any_calculator_args.calculation_id}"
| where Properties.CategoryName == "Energinet.DataHub.package.calculator_job_args"
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

    def test_add_trace_log_record_to_azure_monitor_with_expected_settings(
        self,
        any_calculator_args: CalculatorArgs,
        integration_test_configuration: IntegrationTestConfiguration,
    ):
        """
        Assert that the calculator job logs to Azure Monitor with the expected settings:
        - app role name = "dbr-calculation-engine"
        - name = "calculation.create_calculation_arguments"
        - operation id has value
        - custom field "Subsystem" = "wholesale"
        - custom field "calculation_id" = <the calculation id>
        """

        # Arrange
        self.prepare_command_line_arguments(any_calculator_args)

        # Act
        with pytest.raises(SystemExit):
            start_with_deps(
                applicationinsights_connection_string=integration_test_configuration.get_applicationinsights_connection_string(),
            )

        # Assert
        # noinspection PyTypeChecker
        logs_client = LogsQueryClient(integration_test_configuration.credential)

        query = f"""
AppDependencies
| where AppRoleName == "dbr-calculation-engine"
| where Name == "calculation.create_calculation_arguments"
| where OperationId != "00000000000000000000000000000000"
| where Properties.Subsystem == "wholesale"
| where Properties.calculation_id == "{any_calculator_args.calculation_id}"
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

    def test_adds_exception_log_record_to_azure_monitor_with_expected_settings(
        self,
        any_calculator_args: CalculatorArgs,
        integration_test_configuration: IntegrationTestConfiguration,
    ):
        """
        Assert that the calculator job logs to Azure Monitor with the expected settings:
        - app role name = "dbr-calculation-engine"
        - exception type = <exception type name>
        - outer message <exception message>
        - operation id has value
        - custom field "Subsystem" = "wholesale"
        - custom field "calculation_id" = <the calculation id>
        - custom field "CategoryName" = "Energinet.DataHub." + <logger name>
        """

        # Arrange
        self.prepare_command_line_arguments(any_calculator_args)

        with pytest.raises(SystemExit):
            # Act
            start_with_deps(
                applicationinsights_connection_string=integration_test_configuration.get_applicationinsights_connection_string(),
            )

        # Assert
        # noinspection PyTypeChecker
        logs_client = LogsQueryClient(integration_test_configuration.credential)

        query = f"""
AppExceptions
| where AppRoleName == "dbr-calculation-engine"
| where ExceptionType == "ValueError"
| where OuterMessage == "Environment variable not found: TIME_ZONE"
| where OperationId != "00000000000000000000000000000000"
| where Properties.Subsystem == "wholesale"
| where Properties.calculation_id == "{any_calculator_args.calculation_id}"
| where Properties.CategoryName == "Energinet.DataHub.package.calculator_job"
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

    @staticmethod
    def prepare_command_line_arguments(any_calculator_args):
        any_calculator_args.calculation_id = str(
            uuid.uuid4()
        )  # Ensure unique calculation id
        sys.argv = []
        sys.argv.append("--dummy=")
        sys.argv.append(f"--calculation-id={str(any_calculator_args.calculation_id)}")
        sys.argv.append("--grid-areas=[123]")
        sys.argv.append("--period-start-datetime=2023-01-31T23:00:00Z")
        sys.argv.append("--period-end-datetime=2023-01-31T23:00:00Z")
        sys.argv.append("--calculation-type=BalanceFixing")
        sys.argv.append("--execution-time-start=2023-01-31T23:00:00Z")


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
