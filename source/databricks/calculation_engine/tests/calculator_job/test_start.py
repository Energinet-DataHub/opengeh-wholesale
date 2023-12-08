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

from package.calculator_job import start, start_with_deps
from tests.integration_test_configuration import IntegrationTestConfiguration


def wait_for_condition(callback: Callable, *, timeout: timedelta, step: timedelta):
    """
    Wait for a condition to be met, or timeout.
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

    # TODO BJM: Consider splitting into multiple tests
    # TODO BJM: Add tests for traces and exceptions as well
    def test_logs_to_azure_monitor(
        self,
        any_calculator_args,
        integration_test_configuration: IntegrationTestConfiguration,
    ):
        # Arrange
        any_calculator_args.batch_id = str(uuid.uuid4())

        # Act
        start_with_deps(
            cmd_line_args_reader=lambda: any_calculator_args,
            calculation_executor=lambda args, reader: None,
            is_storage_locked_checker=lambda name, cred: False,
            applicationinsights_connection_string=integration_test_configuration.get_applicationinsights_connection_string(),
        )

        # Assert
        # noinspection PyTypeChecker
        # From https://youtrack.jetbrains.com/issue/PY-59279/Type-checking-detects-an-error-when-passing-an-instance-implicitly-conforming-to-a-Protocol-to-a-function-expecting-that:
        #    DefaultAzureCredential does not conform to protocol TokenCredential, because its method get_token is missing
        #    the arguments claims and tenant_id. Surely, they might appear among the arguments passed as **kwargs, but it's
        #    not guaranteed. In other words, you can make a call to get_token which will typecheck fine for
        #    DefaultAzureCredential, but not for TokenCredential.
        logs_client = LogsQueryClient(integration_test_configuration.credential)

        # Kusto query to get logs with message "Calculator job started" and custom field "Domain" set to "wholesale" and cloud role name set to "dbr-calculation-engine" and severity level set to "Informational"
        query = f"""
AppTraces
| where Properties.Domain == "wholesale"
| where Properties.calculation_id == "{any_calculator_args.batch_id}"
| where AppRoleName == "dbr-calculation-engine"
| where Message == "Calculator job started"
| where SeverityLevel == 1 // Informational
| count
//| project message
        """

        # TODO BJM: Remove before merge
        print(
            "%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%% "
            + repr(any_calculator_args.batch_id)
        )
        workspace_id = integration_test_configuration.get_analytics_workspace_id()

        def assert_logged():
            actual = logs_client.query_workspace(
                workspace_id, query, timespan=timedelta(minutes=5)
            )
            actual = cast(LogsQueryResult, actual)
            table = actual.tables[0]
            row = table.rows[0]
            value = row["Count"]
            count = cast(int, value)
            assert count > 0

        # Assert, but timeout after 2 minutes
        wait_for_condition(
            assert_logged, timeout=timedelta(minutes=3), step=timedelta(seconds=10)
        )
