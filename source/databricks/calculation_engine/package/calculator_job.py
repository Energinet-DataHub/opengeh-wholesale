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
from typing import Callable, Tuple

from opentelemetry.trace import SpanKind, Status, StatusCode, Span

import package.infrastructure.logging_configuration as config
from package import calculation
from package.calculation import input
from package.calculation.calculator_args import CalculatorArgs
from package.calculator_job_args import (
    parse_job_arguments,
    parse_command_line_arguments,
)
from package.container import create_and_configure_container, Container
from package.infrastructure.infrastructure_settings import InfrastructureSettings
from package.infrastructure.storage_account_access import islocked


# The start() method should only have its name updated in correspondence with the
# wheels entry point for it. Further the method must remain parameterless because
# it will be called from the entry point when deployed.
def start() -> None:
    applicationinsights_connection_string = os.getenv(
        "APPLICATIONINSIGHTS_CONNECTION_STRING"
    )

    start_with_deps(
        applicationinsights_connection_string=applicationinsights_connection_string
    )


def start_with_deps(
    *,
    cloud_role_name: str = "dbr-calculation-engine",
    applicationinsights_connection_string: str | None = None,
    parse_command_line_args: Callable[..., Namespace] = parse_command_line_arguments,
    parse_job_args: Callable[
        ..., Tuple[CalculatorArgs, InfrastructureSettings]
    ] = parse_job_arguments,
    calculation_executor: Callable[..., None] = calculation.execute,
    is_storage_locked_checker: Callable[..., bool] = islocked,
) -> None:
    """Start overload with explicit dependencies for easier testing."""

    config.configure_logging(
        cloud_role_name=cloud_role_name,
        applicationinsights_connection_string=applicationinsights_connection_string,
        extras={"Subsystem": "wholesale"},
    )

    with config.get_tracer().start_as_current_span(
        __name__, kind=SpanKind.SERVER
    ) as span:
        # Try/except added to enable adding custom fields to the exception as
        # the span attributes do not appear to be included in the exception.
        try:
            # The command line arguments are parsed to have necessary information for coming log messages
            command_line_args = parse_command_line_args()

            # Add calculation_id to structured logging data to be included in every log message.
            config.add_extras({"calculation_id": command_line_args.calculation_id})
            span.set_attributes(config.get_extras())

            args, infrastructure_settings = parse_job_args(command_line_args)
            create_and_configure_container(infrastructure_settings)

            raise_if_storage_is_locked(
                is_storage_locked_checker, infrastructure_settings
            )

            prepared_data_reader = create_prepared_data_reader(infrastructure_settings)
            calculation_executor(args, prepared_data_reader)

        # Added as ConfigArgParse uses sys.exit() rather than raising exceptions
        except SystemExit as e:
            if e.code != 0:
                record_exception(e, span)
            sys.exit(e.code)

        except Exception as e:
            record_exception(e, span)
            sys.exit(4)


def record_exception(exception: SystemExit | Exception, span: Span) -> None:
    span.set_status(Status(StatusCode.ERROR))
    span.record_exception(
        exception,
        attributes=config.get_extras()
        | {"CategoryName": f"Energinet.DataHub.{__name__}"},
    )


def create_prepared_data_reader(
    settings: InfrastructureSettings,
) -> calculation.PreparedDataReader:
    """Create calculation execution dependencies."""
    spark = Container.spark
    delta_table_reader = input.TableReader(
        spark,
        settings.calculation_input_path,
        settings.time_series_points_table_name,
        settings.metering_point_periods_table_name,
        settings.grid_loss_metering_points_table_name,
    )
    prepared_data_reader = calculation.PreparedDataReader(delta_table_reader)
    return prepared_data_reader


def raise_if_storage_is_locked(
    is_storage_locked_checker: Callable[..., bool], settings: InfrastructureSettings
) -> None:
    if is_storage_locked_checker(
        settings.data_storage_account_name, settings.data_storage_account_credentials
    ):
        raise Exception(
            "Exiting because storage is locked due to data migrations running."
        )
