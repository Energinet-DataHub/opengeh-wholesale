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

import configargparse
from configargparse import argparse
import sys
from package.calculation.calculator_args import CalculatorArgs
from package.infrastructure import valid_date, valid_list, log, paths
import package.infrastructure.environment_variables as env_vars
from package.codelists.process_type import ProcessType


def get_calculator_args() -> CalculatorArgs:
    job_args = _get_valid_args_or_throw(sys.argv[1:])
    log(f"Job arguments: {str(job_args)}")

    time_zone = env_vars.get_time_zone()
    storage_account_name = env_vars.get_storage_account_name()
    credential = env_vars.get_storage_account_credential()

    calculator_args = CalculatorArgs(
        data_storage_account_name=storage_account_name,
        data_storage_account_credentials=credential,
        wholesale_container_path=paths.get_container_root_path(storage_account_name),
        calculation_input_path=paths.get_calculation_input_path(storage_account_name),
        time_series_periods_table_name=job_args.time_series_periods_table_name,
        batch_id=job_args.batch_id,
        batch_grid_areas=job_args.batch_grid_areas,
        batch_period_start_datetime=job_args.batch_period_start_datetime,
        batch_period_end_datetime=job_args.batch_period_end_datetime,
        batch_execution_time_start=job_args.batch_execution_time_start,
        batch_process_type=job_args.batch_process_type,
        time_zone=time_zone,
    )

    return calculator_args


def _get_valid_args_or_throw(command_line_args: list[str]) -> argparse.Namespace:
    p = configargparse.ArgParser(
        description="Performs domain calculations for submitted batches",
        formatter_class=configargparse.ArgumentDefaultsHelpFormatter,
    )

    # Run parameters
    p.add("--batch-id", type=str, required=True)
    p.add("--batch-grid-areas", type=valid_list, required=True)
    p.add("--batch-period-start-datetime", type=valid_date, required=True)
    p.add("--batch-period-end-datetime", type=valid_date, required=True)
    p.add("--batch-process-type", type=ProcessType, required=True)
    p.add("--batch-execution-time-start", type=valid_date, required=True)
    p.add("--time_series_periods_table_name", type=str, required=False)

    args, unknown_args = p.parse_known_args(args=command_line_args)
    if len(unknown_args):
        unknown_args_text = ", ".join(unknown_args)
        raise Exception(f"Unknown args: {unknown_args_text}")

    if type(args.batch_grid_areas) is not list:
        raise Exception("Grid areas must be a list")

    return args
