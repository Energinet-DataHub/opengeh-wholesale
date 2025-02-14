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

import geh_common.telemetry.logging_configuration as config
from geh_common.telemetry.span_recording import span_record_exception
from geh_common.telemetry.logger import Logger
from geh_common.telemetry.decorators import start_trace, use_span
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


# from telemetry_logging.logging_configuration import configure_logging, LoggingSettings

from datetime import datetime

from package.common.datetime_utils import (
    is_exactly_one_calendar_month,
    is_midnight_in_time_zone,
)
from package.codelists.calculation_type import (
    CalculationType,
    is_wholesale_calculation_type,
)


# The start() method should only have its name updated in correspondence with the
# wheels entry point for it. Further the method must remain parameterless because
# it will be called from the entry point when deployed.
def start() -> None:
    # Parse params for LoggingSettings
    logging_settings = config.LoggingSettings(
        cloud_role_name="dbr-calculation-engine",
        subsystem="wholesale-aggregations",  # Will be used as trace_name
    )

    # Parse params for CalculatorArgs and InfrastructureSettings
    args = CalculatorArgs()
    infrastructure_settings = InfrastructureSettings()

    # Extra validation of Params
    _validate_quarterly_resolution_transition_datetime(args=args)

    if is_wholesale_calculation_type(args.calculation_type):
        _validate_period_for_wholesale_calculation(args=args)

    _throw_exception_if_internal_calculation_and_not_aggregation_calculation_type(args)

    config.configure_logging(
        logging_settings=logging_settings,
        extras=dict(calculation_id=args.calculation_id),
    )

    start_with_deps(args=args, infrastructure_settings=infrastructure_settings)


@start_trace()
def start_with_deps(
    *,
    args: CalculatorArgs,
    infrastructure_settings: InfrastructureSettings,
) -> None:
    """Start overload with explicit dependencies for easier testing."""
    logger = Logger(__name__)
    logger.info(f"Calculator arguments: {args}")
    logger.info(f"Infrastructure settings: {infrastructure_settings}")

    spark = initialize_spark()
    create_and_configure_container(spark, infrastructure_settings)

    prepared_data_reader = create_prepared_data_reader(infrastructure_settings, spark)

    if not prepared_data_reader.is_calculation_id_unique(args.calculation_id):
        raise Exception(f"Calculation ID '{args.calculation_id}' is already used.")

    calculation.execute(
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


def _validate_quarterly_resolution_transition_datetime(args: CalculatorArgs) -> None:
    if (
        is_midnight_in_time_zone(
            args.quarterly_resolution_transition_datetime, args.time_zone
        )
        is False
    ):
        raise Exception(
            f"The quarterly resolution transition datetime must be at midnight local time ({args.time_zone})."
        )
    if (
        args.calculation_period_start_datetime
        < args.quarterly_resolution_transition_datetime
        < args.calculation_period_end_datetime
    ):
        raise Exception(
            "The calculation period must not cross the quarterly resolution transition datetime."
        )


def _validate_period_for_wholesale_calculation(args: CalculatorArgs) -> None:
    is_valid_period = is_exactly_one_calendar_month(
        args.calculation_period_start_datetime,
        args.calculation_period_end_datetime,
        args.time_zone,
    )

    if not is_valid_period:
        raise Exception(
            f"The calculation period for wholesale calculation types must be a full month starting and ending at midnight local time ({time_zone}))."
        )


def _throw_exception_if_internal_calculation_and_not_aggregation_calculation_type(
    args: CalculatorArgs,
) -> None:
    if (
        calculator_args.is_internal_calculation
        and calculator_args.calculation_type != CalculationType.AGGREGATION
    ):
        raise Exception("Internal calculations must be of type AGGREGATION. ")
