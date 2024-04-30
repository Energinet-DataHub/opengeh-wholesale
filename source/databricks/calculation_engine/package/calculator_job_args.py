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
import datetime
import sys
from argparse import Namespace
from typing import Tuple

import configargparse
from configargparse import argparse

import package.infrastructure.environment_variables as env_vars
from package.calculation.calculator_args import CalculatorArgs
from package.codelists.calculation_type import (
    CalculationType,
    is_wholesale_calculation_type,
)
from package.common.logger import Logger
from package.common.datetime_utils import (
    is_exactly_one_calendar_month,
    is_midnight_in_time_zone,
)
from package.infrastructure import valid_date, valid_list, logging_configuration, paths
from package.infrastructure.infrastructure_settings import InfrastructureSettings


def parse_command_line_arguments() -> Namespace:
    return _parse_args_or_throw(sys.argv[1:])


def parse_job_arguments(
    job_args: Namespace,
) -> Tuple[CalculatorArgs, InfrastructureSettings]:
    logger = Logger(__name__)
    logger.info(f"Command line arguments: {repr(job_args)}")

    with logging_configuration.start_span("calculation.parse_job_arguments"):
        time_zone = env_vars.get_time_zone()
        quarterly_resolution_transition_datetime = (
            env_vars.get_quarterly_resolution_transition_datetime()
        )

        _validate_quarterly_resolution_transition_datetime(
            quarterly_resolution_transition_datetime,
            time_zone,
            job_args.period_start_datetime,
            job_args.period_end_datetime,
        )

        if is_wholesale_calculation_type(job_args.calculation_type):
            _validate_period_for_wholesale_calculation(
                time_zone,
                job_args.period_start_datetime,
                job_args.period_end_datetime,
            )

        calculator_args = CalculatorArgs(
            calculation_id=job_args.calculation_id,
            calculation_grid_areas=job_args.grid_areas,
            calculation_period_start_datetime=job_args.period_start_datetime,
            calculation_period_end_datetime=job_args.period_end_datetime,
            calculation_execution_time_start=datetime.utcnow(),
            calculation_type=job_args.calculation_type,
            created_by_user_id=job_args.created_by_user_id,
            time_zone=time_zone,
            quarterly_resolution_transition_datetime=quarterly_resolution_transition_datetime,
        )

        storage_account_name = env_vars.get_storage_account_name()
        credential = env_vars.get_storage_account_credential()
        infrastructure_settings = InfrastructureSettings(
            data_storage_account_name=storage_account_name,
            data_storage_account_credentials=credential,
            wholesale_container_path=paths.get_container_root_path(
                storage_account_name
            ),
            calculation_input_path=paths.get_calculation_input_path(
                storage_account_name, job_args.calculation_input_folder_name
            ),
            time_series_points_table_name=job_args.time_series_points_table_name,
            metering_point_periods_table_name=job_args.metering_point_periods_table_name,
            grid_loss_metering_points_table_name=job_args.grid_loss_metering_points_table_name,
        )

        return calculator_args, infrastructure_settings


def _parse_args_or_throw(command_line_args: list[str]) -> argparse.Namespace:
    p = configargparse.ArgParser(
        description="Execute a calculation",
        formatter_class=configargparse.ArgumentDefaultsHelpFormatter,
    )

    # Run parameters
    p.add("--calculation-id", type=str, required=True)
    p.add("--grid-areas", type=valid_list, required=True)
    p.add("--period-start-datetime", type=valid_date, required=True)
    p.add("--period-end-datetime", type=valid_date, required=True)
    p.add("--calculation-type", type=CalculationType, required=True)
    p.add("--created-by-user-id", type=str, required=True)
    # Infrastructure settings
    p.add("--calculation_input_folder_name", type=str, required=False)
    p.add("--time_series_points_table_name", type=str, required=False)
    p.add("--metering_point_periods_table_name", type=str, required=False)
    p.add("--grid_loss_metering_points_table_name", type=str, required=False)

    args, unknown_args = p.parse_known_args(args=command_line_args)
    if len(unknown_args):
        unknown_args_text = ", ".join(unknown_args)
        raise Exception(f"Unknown args: {unknown_args_text}")

    if type(args.grid_areas) is not list:
        raise Exception("Grid areas must be a list")

    return args


def _validate_quarterly_resolution_transition_datetime(
    quarterly_resolution_transition_datetime: datetime,
    time_zone: str,
    calculation_period_start_datetime: datetime,
    calculation_period_end_datetime: datetime,
) -> None:
    if (
        is_midnight_in_time_zone(quarterly_resolution_transition_datetime, time_zone)
        is False
    ):
        raise Exception(
            f"The quarterly resolution transition datetime must be at midnight local time ({time_zone})."
        )
    if (
        calculation_period_start_datetime < quarterly_resolution_transition_datetime
        and calculation_period_end_datetime > quarterly_resolution_transition_datetime
    ):
        raise Exception(
            "The calculation period must not cross the quarterly resolution transition datetime."
        )


def _validate_period_for_wholesale_calculation(
    time_zone: str,
    calculation_period_start_datetime: datetime,
    calculation_period_end_datetime: datetime,
) -> None:
    is_valid_period = is_exactly_one_calendar_month(
        calculation_period_start_datetime,
        calculation_period_end_datetime,
        time_zone,
    )

    if not is_valid_period:
        raise Exception(
            f"The calculation period for wholesale calculation types must be a full month starting and ending at midnight local time ({time_zone}))."
        )
