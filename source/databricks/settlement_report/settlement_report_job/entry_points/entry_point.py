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
from argparse import Namespace
from collections.abc import Callable

from opentelemetry.trace import SpanKind

import telemetry_logging.logging_configuration as config
from telemetry_logging.span_recording import span_record_exception
from settlement_report_job.entry_points.tasks import task_factory
from settlement_report_job.entry_points.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from settlement_report_job.entry_points.job_args.settlement_report_job_args import (
    parse_job_arguments,
    parse_command_line_arguments,
)
from settlement_report_job.entry_points.tasks.task_type import TaskType
from settlement_report_job.entry_points.utils.get_dbutils import get_dbutils
from settlement_report_job.infrastructure.spark_initializor import initialize_spark


# The start_x() methods should only have its name updated in correspondence with the
# wheels entry point for it. Further the method must remain parameterless because
# it will be called from the entry point when deployed.
def start_hourly_time_series_points() -> None:
    _start_task(TaskType.TimeSeriesHourly)


def start_quarterly_time_series_points() -> None:
    _start_task(TaskType.TimeSeriesQuarterly)


def start_metering_point_periods() -> None:
    _start_task(TaskType.MeteringPointPeriods)


def start_charge_link_periods() -> None:
    _start_task(TaskType.ChargeLinks)


def start_charge_price_points() -> None:
    _start_task(TaskType.ChargePricePoints)


def start_energy_results() -> None:
    _start_task(TaskType.EnergyResults)


def start_wholesale_results() -> None:
    _start_task(TaskType.WholesaleResults)


def start_monthly_amounts() -> None:
    _start_task(TaskType.MonthlyAmounts)


def start_zip() -> None:
    _start_task(TaskType.Zip)


def _start_task(task_type: TaskType) -> None:
    applicationinsights_connection_string = os.getenv(
        "APPLICATIONINSIGHTS_CONNECTION_STRING"
    )

    start_task_with_deps(
        task_type=task_type,
        applicationinsights_connection_string=applicationinsights_connection_string,
    )


def start_task_with_deps(
    *,
    task_type: TaskType,
    cloud_role_name: str = "dbr-settlement-report",
    applicationinsights_connection_string: str | None = None,
    parse_command_line_args: Callable[..., Namespace] = parse_command_line_arguments,
    parse_job_args: Callable[..., SettlementReportArgs] = parse_job_arguments,
) -> None:
    """Start overload with explicit dependencies for easier testing."""
    config.configure_logging(
        cloud_role_name=cloud_role_name,
        tracer_name="settlement-report-job",
        applicationinsights_connection_string=applicationinsights_connection_string,
        extras={"Subsystem": "wholesale-aggregations"},
    )

    with config.get_tracer().start_as_current_span(
        __name__, kind=SpanKind.SERVER
    ) as span:
        # Try/except added to enable adding custom fields to the exception as
        # the span attributes do not appear to be included in the exception.
        try:

            # The command line arguments are parsed to have necessary information for
            # coming log messages
            command_line_args = parse_command_line_args()

            # Add settlement_report_id to structured logging data to be included in
            # every log message.
            config.add_extras({"settlement_report_id": command_line_args.report_id})
            span.set_attributes(config.get_extras())
            args = parse_job_args(command_line_args)
            spark = initialize_spark()

            task = task_factory.create(task_type, spark, args)
            task.execute()

        # Added as ConfigArgParse uses sys.exit() rather than raising exceptions
        except SystemExit as e:
            if e.code != 0:
                span_record_exception(e, span)
            sys.exit(e.code)

        except Exception as e:
            span_record_exception(e, span)
            sys.exit(4)
