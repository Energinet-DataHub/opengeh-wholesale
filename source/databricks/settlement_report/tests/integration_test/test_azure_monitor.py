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
from unittest.mock import Mock, patch

import pytest
from azure.monitor.query import LogsQueryClient, LogsQueryResult

from settlement_report_job.entry_points.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from settlement_report_job.domain.report_generator import ReportGenerator
from settlement_report_job.entry_points.entry_point import (
    start_task_with_deps,
    start_hourly_time_series,
)
from settlement_report_job.entry_points.job_args.settlement_report_job_args import (
    parse_job_arguments,
)
from tests.integration_test_configuration import IntegrationTestConfiguration


class TestWhenInvokedWithValidArguments:
    def test_add_info_log_record_to_azure_monitor_with_expected_settings(
        self,
        standard_wholesale_fixing_scenario_args: SettlementReportArgs,
        integration_test_configuration: IntegrationTestConfiguration,
    ) -> None:
        """
        Assert that the calculator job adds log records to Azure Monitor with the expected settings:
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
        self.prepare_command_line_arguments(standard_wholesale_fixing_scenario_args)

        # Act
        with pytest.raises(SystemExit):
            start_task_with_deps(
                ReportGenerator.execute_wholesale_results(),
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
        | where Properties.CategoryName == "Energinet.DataHub.settlement_report_job.infrastructure.settlement_report_job_args"
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
    def prepare_command_line_arguments(
        standard_wholesale_fixing_scenario_args: SettlementReportArgs,
    ) -> None:
        standard_wholesale_fixing_scenario_args.report_id = str(
            uuid.uuid4()
        )  # Ensure unique report id
        sys.argv = []
        sys.argv.append(
            f"--report-id={str(standard_wholesale_fixing_scenario_args.report_id)}"
        )
        sys.argv.append(
            f"--period-start-datetime={str(standard_wholesale_fixing_scenario_args.period_start)}"
        )
        sys.argv.append(
            f"--period-end-datetime={str(standard_wholesale_fixing_scenario_args.period_end)}"
        )
        sys.argv.append(
            f"--calculation-type={str(standard_wholesale_fixing_scenario_args.calculation_type)}"
        )
        sys.argv.append(
            f"--requesting-actor-market-role={str(standard_wholesale_fixing_scenario_args.requesting_actor_market_role)}"
        )
        sys.argv.append(
            f"--requesting-actor-id={str(standard_wholesale_fixing_scenario_args.requesting_actor_id)}"
        )
        sys.argv.append(
            f"--calculation-id-by-grid-area={str(standard_wholesale_fixing_scenario_args.calculation_id_by_grid_area)}"
        )
        sys.argv.append(
            f"--grid-areas-codes={str(standard_wholesale_fixing_scenario_args.grid_area_codes)}"
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
