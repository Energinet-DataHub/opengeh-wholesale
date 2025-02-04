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

from opentelemetry.trace import SpanKind
from pyspark.sql import SparkSession

import telemetry_logging.logging_configuration as config
from telemetry_logging.span_recording import span_record_exception
from package import calculation
from package.calculation import CalculationCore
from package.calculation.calculation_metadata_service import CalculationMetadataService
from package.calculation.calculation_output_service import CalculationOutputService
from package.calculation.calculator_args import CalculatorArgs
from package.calculator_job_args import (
    parse_job_arguments,
    parse_command_line_arguments,
)
from package.container import create_and_configure_container
from package.databases import migrations_wholesale, wholesale_internal
from package.infrastructure import initialize_spark
from package.infrastructure.infrastructure_settings import InfrastructureSettings


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
    cloud_role_name: str = "dbr-calculation-engine", # TODO CHBA NOTE: Sets the name of the top-level request
    applicationinsights_connection_string: str | None = None,
    parse_command_line_args: Callable[..., Namespace] = parse_command_line_arguments,
    parse_job_args: Callable[
        ..., Tuple[CalculatorArgs, InfrastructureSettings]
    ] = parse_job_arguments,
    calculation_executor: Callable[..., None] = calculation.execute,
) -> None:
    """Start overload with explicit dependencies for easier testing."""

    config.configure_logging(
        cloud_role_name=cloud_role_name,
        tracer_name="calculator-job",
        applicationinsights_connection_string=applicationinsights_connection_string,
        extras={"Subsystem": "wholesale-aggregations"},
    )

    with config.get_tracer().start_as_current_span(
        __name__, kind=SpanKind.SERVER
    ) as span: # TODO CHBA NOTE: Sets the name of the top-level request, using initial span
        # Try/except added to enable adding custom fields to the exception as
        # the span attributes do not appear to be included in the exception.
        try:
            # The command line arguments are parsed to have necessary information for coming log messages
            command_line_args = parse_command_line_args() # TODO CHBA NOTE: Calls package.calculator_job_args.parse_command_line_arguments, but it does NOT implement decorator - hence not visible in the log

            # Add calculation_id to structured logging data to be included in every log message.
            config.add_extras({"calculation_id": command_line_args.calculation_id})
            span.set_attributes(config.get_extras())

            args, infrastructure_settings = parse_job_args(command_line_args) # TODO CHBA NOTE: This one implements use_span in its definition, and actually creates a subspan of the same name

            spark = initialize_spark()
            create_and_configure_container(spark, infrastructure_settings)

            prepared_data_reader = create_prepared_data_reader(
                infrastructure_settings, spark
            )

            if not prepared_data_reader.is_calculation_id_unique(args.calculation_id):
                raise Exception(
                    f"Calculation ID '{args.calculation_id}' is already used."
                )

            calculation_executor(
                args,
                prepared_data_reader,
                CalculationCore(),
                CalculationMetadataService(),
                CalculationOutputService(),
            )

        # Added as ConfigArgParse uses sys.exit() rather than raising exceptions
        except SystemExit as e:
            if e.code != 0:
                span_record_exception(e, span)
            sys.exit(e.code)

        except Exception as e:
            span_record_exception(e, span)
            sys.exit(4)


def create_prepared_data_reader(
    settings: InfrastructureSettings,
    spark: SparkSession,
) -> calculation.PreparedDataReader:
    """Create calculation execution dependencies."""
    migrations_wholesale_repository = (
        migrations_wholesale.MigrationsWholesaleRepository(
            spark,
            settings.catalog_name,
            settings.calculation_input_database_name,
            settings.time_series_points_table_name,
            settings.metering_point_periods_table_name,
            settings.grid_loss_metering_point_ids_table_name,
        )
    )

    wholesale_internal_repository = wholesale_internal.WholesaleInternalRepository(
        spark,
        settings.catalog_name,
    )

    prepared_data_reader = calculation.PreparedDataReader(
        migrations_wholesale_repository, wholesale_internal_repository
    )
    return prepared_data_reader
