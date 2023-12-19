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
import sys
import time
import uuid
from datetime import timedelta
from typing import cast, Callable

import pytest
from azure.monitor.query import LogsQueryClient, LogsQueryResult

from package.calculation.calculator_args import CalculatorArgs
import package.calculator_job
from package.calculator_job import start, start_with_deps
from package.common.logger import Logger
from tests.integration_test_configuration import IntegrationTestConfiguration


class TestWhenInvokedWithInvalidArguments:
    def test_exits_with_code_2(self):
        """The exit code 2 originates from the argparse library."""
        with pytest.raises(SystemExit) as system_exit:
            start()

        assert system_exit.value.code == 2


class TestWhenInvokedWithValidArguments:
    def test_does_not_raise(self, any_calculator_args):
        start_with_deps(
            cmd_line_args_reader=lambda: any_calculator_args,
            calculation_executor=lambda args, reader: None,
            is_storage_locked_checker=lambda name, cred: False,
        )

    @pytest.mark.parametrize(
        "log_func, log_level",
        [(Logger.info, 1), (Logger.warning, 2)],
    )
    def test_logs_traces_to_azure_monitor_with_expected_settings(
        self,
        log_func: Callable,
        log_level: int,
        any_calculator_args: CalculatorArgs,
        integration_test_configuration: IntegrationTestConfiguration,
    ):
        """
        Assert that the calculator job logs trace to Azure Monitor with the expected settings:
        - cloud role name = "dbr-calculation-engine"
        - severity level = <log_level>
        - message <the message>
        - operation id has value
        - custom field "Domain" = "wholesale"
        - custom field "calculation_id" = <the calculation id>
        - custom field "CategoryName" = "Energinet.DataHub." + <logger name>

        Debug level is not tested as it is not intended to be logged by default.
        """

        # Arrange
        any_calculator_args.batch_id = str(uuid.uuid4())  # Ensure unique calculation id
        test_message = f"Test message with log level {log_level}"

        def executor(args, reader):
            # A little hacking to invoke the function from the test parameters on a logger object
            logger = Logger(__name__)
            f = getattr(logger, log_func.__name__)
            f(test_message)

        # Act
        start_with_deps(
            cmd_line_args_reader=lambda: any_calculator_args,
            calculation_executor=executor,
            is_storage_locked_checker=lambda name, cred: False,
            applicationinsights_connection_string=integration_test_configuration.get_applicationinsights_connection_string(),
        )

        # Assert
        # noinspection PyTypeChecker
        logs_client = LogsQueryClient(integration_test_configuration.credential)

        query = f"""
AppTraces
| where AppRoleName == "dbr-calculation-engine"
| where SeverityLevel == {log_level}
| where Message == "{test_message}"
| where OperationId != "00000000000000000000000000000000"
| where Properties.Domain == "wholesale"
| where Properties.calculation_id == "{any_calculator_args.batch_id}"
| where Properties.CategoryName == "Energinet.DataHub.{__name__}"
| count
        """

        workspace_id = integration_test_configuration.get_analytics_workspace_id()

        def assert_logged():
            actual = logs_client.query_workspace(
                workspace_id, query, timespan=timedelta(minutes=5)
            )
            assert_row_count(actual, 1)

        # Assert, but timeout after 2 minutes if not succeeded
        wait_for_condition(
            assert_logged, timeout=timedelta(minutes=3), step=timedelta(seconds=10)
        )

    def test_logs_exceptions_to_azure_monitor_with_expected_settings(
        self,
        any_calculator_args: CalculatorArgs,
        integration_test_configuration: IntegrationTestConfiguration,
    ):
        """
        Assert that the calculator job logs to Azure Monitor with the expected settings:
        - cloud role name = "dbr-calculation-engine"
        - exception type = <exception type name>
        - outer message <exception message>
        - operation id has value
        - custom field "Domain" = "wholesale"
        - custom field "calculation_id" = <the calculation id>
        - custom field "CategoryName" = "Energinet.DataHub." + <logger name>
        """

        # Arrange
        any_calculator_args.batch_id = str(uuid.uuid4())

        def raise_exception():
            raise ValueError("Test exception")

        with pytest.raises(ValueError):
            # Act
            start_with_deps(
                cmd_line_args_reader=lambda: any_calculator_args,
                calculation_executor=lambda args, reader: raise_exception(),
                is_storage_locked_checker=lambda name, cred: False,
                applicationinsights_connection_string=integration_test_configuration.get_applicationinsights_connection_string(),
            )

        # Assert
        # noinspection PyTypeChecker
        logs_client = LogsQueryClient(integration_test_configuration.credential)

        query = f"""
AppExceptions
| where AppRoleName == "dbr-calculation-engine"
| where ExceptionType == "ValueError"
| where OuterMessage == "Test exception"
| where OperationId != "00000000000000000000000000000000"
| where Properties.Domain == "wholesale"
| where Properties.calculation_id == "{any_calculator_args.batch_id}"
| where Properties.CategoryName == "Energinet.DataHub.{package.calculator_job.__name__}"
| count
        """

        workspace_id = integration_test_configuration.get_analytics_workspace_id()

        def assert_logged():
            actual = logs_client.query_workspace(
                workspace_id, query, timespan=timedelta(minutes=5)
            )
            assert_row_count(actual, 1)

        # Assert, but timeout after 2 minutes if not succeeded
        wait_for_condition(
            assert_logged, timeout=timedelta(minutes=3), step=timedelta(seconds=10)
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
