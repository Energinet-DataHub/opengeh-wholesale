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
from argparse import Namespace

import configargparse
from configargparse import argparse

from settlement_report_job import logging_configuration
from settlement_report_job.args_helper import valid_date
from settlement_report_job.calculation_type import CalculationType
from settlement_report_job.logger import Logger
from settlement_report_job.settlement_report_args import SettlementReportArgs
import settlement_report_job.environment_variables as env_vars


def parse_command_line_arguments() -> Namespace:
    return _parse_args_or_throw(sys.argv[1:])


def parse_job_arguments(
    job_args: Namespace,
) -> SettlementReportArgs:
    logger = Logger(__name__)
    logger.info(f"Command line arguments: {repr(job_args)}")

    with logging_configuration.start_span("settlement_report.parse_job_arguments"):

        settlement_report_args = SettlementReportArgs(
            report_id=job_args.report_id,
            period_start=job_args.period_start,
            period_end=job_args.period_end,
            calculation_type=job_args.calculation_type,
            split_report_by_grid_area=job_args.split_report_by_grid_area,
            prevent_large_text_files=job_args.prevent_large_text_files,
            time_zone="Europe/Copenhagen",
            catalog_name=env_vars.get_catalog_name(),
        )

        return settlement_report_args


def _parse_args_or_throw(command_line_args: list[str]) -> argparse.Namespace:
    p = configargparse.ArgParser(
        description="Create settlement report",
        formatter_class=configargparse.ArgumentDefaultsHelpFormatter,
    )

    # Run parameters
    p.add("--report-id", type=str, required=True)
    p.add("--period-start", type=valid_date, required=True)
    p.add("--period-end", type=valid_date, required=True)
    p.add("--calculation-type", type=CalculationType, required=True)
    p.add(
        "--split-report-by-grid-area", action="store_true"
    )  # true if present, false otherwise
    p.add(
        "--prevent-large-text-files", action="store_true"
    )  # true if present, false otherwise

    args, unknown_args = p.parse_known_args(args=command_line_args)
    if len(unknown_args):
        unknown_args_text = ", ".join(unknown_args)
        raise Exception(f"Unknown args: {unknown_args_text}")

    return args
