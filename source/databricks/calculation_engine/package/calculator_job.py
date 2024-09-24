# calculator_job.py
import os
import sys
from argparse import Namespace
from typing import Callable, Tuple

from pyspark.sql import SparkSession

import package.infrastructure.logging_configuration as config
from package.infrastructure.logging_decorator import log_execution

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


def start() -> None:
    applicationinsights_connection_string = os.getenv(
        "APPLICATIONINSIGHTS_CONNECTION_STRING"
    )

    start_with_deps(
        applicationinsights_connection_string=applicationinsights_connection_string
    )


@log_execution(
    cloud_role_name="dbr-calculation-engine",
    applicationinsights_connection_string=os.getenv(
        "APPLICATIONINSIGHTS_CONNECTION_STRING"
    ),
)
def start_with_deps(
    *,
    parse_command_line_args: Callable[..., Namespace] = parse_command_line_arguments,
    parse_job_args: Callable[
        ..., Tuple[CalculatorArgs, InfrastructureSettings]
    ] = parse_job_arguments,
    calculation_executor: Callable[..., None] = calculation.execute,
) -> None:
    """Start overload with explicit dependencies for easier testing."""

    # The command line arguments are parsed to have necessary information for coming log messages
    command_line_args = parse_command_line_args()

    # Add calculation_id to structured logging data to be included in every log message.
    config.add_extras({"calculation_id": command_line_args.calculation_id})

    args, infrastructure_settings = parse_job_args(command_line_args)

    spark = initialize_spark()
    create_and_configure_container(spark, infrastructure_settings)

    prepared_data_reader = create_prepared_data_reader(infrastructure_settings, spark)

    if not prepared_data_reader.is_calculation_id_unique(args.calculation_id):
        raise Exception(f"Calculation ID '{args.calculation_id}' is already used.")

    calculation_executor(
        args,
        prepared_data_reader,
        CalculationCore(),
        CalculationMetadataService(),
        CalculationOutputService(),
    )


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
            settings.grid_loss_metering_points_table_name,
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
