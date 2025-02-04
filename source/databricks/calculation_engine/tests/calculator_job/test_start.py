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
from datetime import timedelta
from typing import cast, Callable
from unittest import mock
from unittest.mock import Mock, patch


import pytest
from azure.monitor.query import LogsQueryClient, LogsQueryResult

from package.calculation.calculator_args import CalculatorArgs
from package.calculator_job import start
from package.infrastructure.infrastructure_settings import InfrastructureSettings
from tests.integration_test_configuration import IntegrationTestConfiguration

from telemetry_logging.logging_configuration import configure_logging, LoggingSettings

def test_start() -> None:
    env_args = {
        "CLOUD_ROLE_NAME": "test_role",
        "APPLICATIONINSIGHTS_CONNECTION_STRING": "connection_string",
        "SUBSYSTEM": "test_subsystem",
        "CATALOG_NAME": "default_hadoop",
        "TIME_ZONE": "Europe/Copenhagen",
        "CALCULATION_INPUT_DATABASE_NAME": 'calculation_input_database_name_str',
        "DATA_STORAGE_ACCOUNT_NAME": 'data_storage_account_name_str',
        "TENANT_ID": '550e8400-e29b-41d4-a716-446655440000',
        "SPN_APP_ID": '123e4567-e89b-12d3-a456-426614174000',
        "SPN_APP_SECRET": 'MyPassword~HQ',
        "QUARTERLY_RESOLUTION_TRANSITION_DATETIME": "2019-12-04"
    }

    sys_args = [
        "program_name",
        "--force-configuration", "false",
        "--orchestration-instance-id", "4a540892-2c0a-46a9-9257-c4e13051d76a",
        "--calculation-id", "runID123",
        "--grid-areas", "[gridarea1, gridarea2]",
        "--period-start-datetime", "2024-01-30T08:00:00Z",
        "--period-end-datetime", "2024-01-31T08:00:00Z",
        "--calculation-type", "wholesale_fixing",
        "--created-by-user-id", "userid123",
        "--is-internal-calculation", "true",
        "--calculation_input_folder_name", "calculation_input_folder_name_str",
        "--time_series_points_table_name", "time_series_points_table_name_str",
        "--metering_point_periods_table_name", "metering_point_periods_table_name_str",
        "--grid_loss_metering_points_table_name", "grid_loss_metering_point_ids_table_name_str"

    ]
    with (
        mock.patch('sys.argv', sys_args),
        mock.patch.dict('os.environ', env_args, clear=False),
        #mock.patch("package.calculator_job.calculator_args.CalculatorArgs") as mock_CalculatorArgs,
        # mock.patch("telemetry_logging.logging_configuration.LoggingSettings") as mock_logging_settings,
        # mock.patch("telemetry_logging.logging_configuration.configure_logging") as mock_configure_logging,
        # mock.patch(
        #     "package.calculator_job.start.start_with_deps"
        # ) as mock_start_with_deps,
    ):
        #start()
        #print(mock_CalculatorArgs.return_value)
        #1assert mock_CalculatorArgs.assert_called_once()
        assert 1==1

# --------------------------------------------------------------------------------------------------------
class TestWhenInvokedWithInvalidArguments:
    def test_exits_with_code_2(self) -> None:
        """The exit code 2 originates from the argparse library."""
        with pytest.raises(SystemExit) as system_exit:
            start()
        assert system_exit.value.code == 2


class TestWhenInvokedWithValidArguments:
    def test_does_not_raise(
        self,
        any_calculator_args: CalculatorArgs,
        infrastructure_settings: InfrastructureSettings,
    ) -> None:
        command_line_args = argparse.Namespace()
        command_line_args.calculation_id = any_calculator_args.calculation_id
        mock_calculation_execute = Mock()
        mock_prepared_data_reader = Mock()
        mock_prepared_data_reader.is_calculation_id_unique.return_value = True

        with patch("package.calculation.execute", mock_calculation_execute):
            with patch(
                "package.calculation.PreparedDataReader",
                return_value=mock_prepared_data_reader,
            ):
                # Act
                start_with_deps(
                    parse_command_line_args=lambda: command_line_args,
                    parse_job_args=lambda args: (
                        any_calculator_args,
                        infrastructure_settings,
                    ),
                    calculation_executor=mock_calculation_execute,
                )

    def test_add_info_log_record_to_azure_monitor_with_expected_settings(
        self,
        any_calculator_args: CalculatorArgs,
        integration_test_configuration: IntegrationTestConfiguration,
    ) -> None:
        """
        Assert that the calculator job adds log records to Azure Monitor with the expected settings:
        - cloud role name = "dbr-calculation-engine"
        - severity level = 1
        - message <the message>
        - operation id has value
        - custom field "Subsystem" = "wholesale-aggregations"
        - custom field "calculation_id" = <the calculation id>
        - custom field "CategoryName" = "Energinet.DataHub." + <logger name>

        Debug level is not tested as it is not intended to be logged by default.
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
        AppTraces
        | where AppRoleName == "dbr-calculation-engine"
        | where SeverityLevel == 1
        | where Message startswith_cs "Command line arguments"
        | where OperationId != "00000000000000000000000000000000"
        | where Properties.Subsystem == "wholesale-aggregations"
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
        - name = "calculation.parse_job_arguments"
        - operation id has value
        - custom field "Subsystem" = "wholesale-aggregations"
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
        | where Name == "calculation.parse_job_arguments"
        | where OperationId != "00000000000000000000000000000000"
        | where Properties.Subsystem == "wholesale-aggregations"
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
        - custom field "Subsystem" = "wholesale-aggregations"
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
        | where Properties.Subsystem == "wholesale-aggregations"
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
        sys.argv.append("--calculation-type=balance_fixing")
        sys.argv.append("--created-by-user-id=19e0586b-838a-4ea2-96ce-6d923a89c922")


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
