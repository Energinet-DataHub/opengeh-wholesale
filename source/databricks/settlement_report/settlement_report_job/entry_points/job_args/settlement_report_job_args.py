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
import json
import sys
import uuid
from argparse import Namespace

import configargparse
from configargparse import argparse

from settlement_report_job.entry_points.job_args.args_helper import (
    valid_date,
    valid_energy_supplier_ids,
)
from settlement_report_job.entry_points.job_args.calculation_type import CalculationType
from settlement_report_job.infrastructure.paths import (
    get_settlement_reports_output_path,
)
from telemetry_logging import Logger, logging_configuration
from settlement_report_job.domain.utils.market_role import MarketRole
from settlement_report_job.entry_points.job_args.settlement_report_args import (
    SettlementReportArgs,
)
import settlement_report_job.entry_points.job_args.environment_variables as env_vars


def parse_command_line_arguments() -> Namespace:
    return _parse_args_or_throw(sys.argv[1:])


def parse_job_arguments(
    job_args: Namespace,
) -> SettlementReportArgs:
    logger = Logger(__name__)
    logger.info(f"Command line arguments: {repr(job_args)}")

    with logging_configuration.start_span("settlement_report.parse_job_arguments"):

        grid_area_codes = (
            _create_grid_area_codes(job_args.grid_area_codes)
            if job_args.calculation_type is CalculationType.BALANCE_FIXING
            else None
        )

        calculation_id_by_grid_area = (
            _create_calculation_ids_by_grid_area_code(
                job_args.calculation_id_by_grid_area
            )
            if job_args.calculation_type is not CalculationType.BALANCE_FIXING
            else None
        )

        settlement_report_args = SettlementReportArgs(
            report_id=job_args.report_id,
            period_start=job_args.period_start,
            period_end=job_args.period_end,
            calculation_type=job_args.calculation_type,
            requesting_actor_market_role=job_args.requesting_actor_market_role,
            requesting_actor_id=job_args.requesting_actor_id,
            calculation_id_by_grid_area=calculation_id_by_grid_area,
            grid_area_codes=grid_area_codes,
            energy_supplier_ids=job_args.energy_supplier_ids,
            split_report_by_grid_area=job_args.split_report_by_grid_area,
            prevent_large_text_files=job_args.prevent_large_text_files,
            time_zone="Europe/Copenhagen",
            catalog_name=env_vars.get_catalog_name(),
            settlement_reports_output_path=get_settlement_reports_output_path(
                env_vars.get_catalog_name()
            ),
            include_basis_data=job_args.include_basis_data,
        )

        return settlement_report_args


def _parse_args_or_throw(command_line_args: list[str]) -> argparse.Namespace:
    p = configargparse.ArgParser(
        description="Create settlement report",
        formatter_class=configargparse.ArgumentDefaultsHelpFormatter,
    )

    # Run parameters
    p.add_argument("--report-id", type=str, required=True)
    p.add_argument("--period-start", type=valid_date, required=True)
    p.add_argument("--period-end", type=valid_date, required=True)
    p.add_argument("--calculation-type", type=CalculationType, required=True)
    p.add_argument("--requesting-actor-market-role", type=MarketRole, required=True)
    p.add_argument("--requesting-actor-id", type=str, required=True)
    p.add_argument("--calculation-id-by-grid-area", type=str, required=False)
    p.add_argument("--grid-area-codes", type=str, required=False)
    p.add_argument(
        "--energy-supplier-ids", type=valid_energy_supplier_ids, required=False
    )
    p.add_argument(
        "--split-report-by-grid-area", action="store_true"
    )  # true if present, false otherwise
    p.add_argument(
        "--prevent-large-text-files", action="store_true"
    )  # true if present, false otherwise
    p.add_argument(
        "--include-basis-data", action="store_true"
    )  # true if present, false otherwise

    args, unknown_args = p.parse_known_args(args=command_line_args)
    if len(unknown_args):
        unknown_args_text = ", ".join(unknown_args)
        raise Exception(f"Unknown args: {unknown_args_text}")

    return args


def _create_grid_area_codes(grid_area_codes: str) -> list[str]:
    if not grid_area_codes.startswith("[") or not grid_area_codes.endswith("]"):
        msg = "Grid area codes must be a list enclosed by an opening '[' and a closing ']'"
        raise configargparse.ArgumentTypeError(msg)

    # 1. Remove enclosing list characters 2. Split each grid area code 3. Remove possibly enclosing spaces.
    tokens = [token.strip() for token in grid_area_codes.strip("[]").split(",")]

    # Grid area codes must always consist of 3 digits
    if any(
        len(token) != 3 or any(c < "0" or c > "9" for c in token) for token in tokens
    ):
        msg = "Grid area codes must consist of 3 digits"
        raise configargparse.ArgumentTypeError(msg)

    return tokens


def _create_calculation_ids_by_grid_area_code(json_str: str) -> dict[str, uuid.UUID]:
    try:
        calculation_id_by_grid_area = json.loads(json_str)
    except json.JSONDecodeError as e:
        raise ValueError(
            f"Failed to parse `calculation_id_by_grid_area` json format as dict[str, uuid]: {e}"
        )

    for grid_area, calculation_id in calculation_id_by_grid_area.items():
        try:
            calculation_id_by_grid_area[grid_area] = uuid.UUID(calculation_id)
        except ValueError:
            raise ValueError(f"Calculation ID for grid area {grid_area} is not a uuid")

    return calculation_id_by_grid_area
