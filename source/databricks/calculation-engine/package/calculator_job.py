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

from azure.identity import ClientSecretCredential
import sys
import configargparse
from configargparse import argparse
from pyspark.sql import SparkSession
import package.environment_variables as env_vars
from package import (
    calculate_balance_fixing,
    db_logging,
    infrastructure,
    initialize_spark,
    log,
)
from package.file_writers.basis_data_writer import BasisDataWriter
from package.file_writers.process_step_result_writer import ProcessStepResultWriter
from package.file_writers.actors_writer import ActorsWriter
import package.calculation_input as input

from .args_helper import valid_date, valid_list, valid_log_level
from .calculator_args import CalculatorArgs
from package.storage_account_access import islocked


def _get_valid_args_or_throw(command_line_args: list[str]) -> argparse.Namespace:
    p = configargparse.ArgParser(
        description="Performs domain calculations for submitted batches",
        formatter_class=configargparse.ArgumentDefaultsHelpFormatter,
    )

    # Infrastructure settings
    p.add("--data-storage-account-name", type=str, required=False)
    p.add("--data-storage-account-key", type=str, required=False)
    p.add("--time-zone", type=str, required=False)
    p.add("--log-level", type=valid_log_level, help="debug|information", required=False)

    # Run parameters
    p.add("--batch-id", type=str, required=True)
    p.add("--batch-grid-areas", type=valid_list, required=True)
    p.add("--batch-period-start-datetime", type=valid_date, required=True)
    p.add("--batch-period-end-datetime", type=valid_date, required=True)
    p.add("--batch-process-type", type=str, required=True)
    p.add("--batch-execution-time-start", type=valid_date, required=True)
   
    args, unknown_args = p.parse_known_args(args=command_line_args)
    if len(unknown_args):
        unknown_args_text = ", ".join(unknown_args)
        raise Exception(f"Unknown args: {unknown_args_text}")

    if type(args.batch_grid_areas) is not list:
        raise Exception("Grid areas must be a list")

    return args


def _start_calculator(spark: SparkSession, args: CalculatorArgs) -> None:
    timeseries_points_df = (
        spark.read.option("mode", "FAILFAST")
        .format("delta")
        .load(
            f"{args.wholesale_container_path}/calculation-input-v2/time-series-points"
        )
    )
    metering_points_periods_df = (
        spark.read.option("mode", "FAILFAST")
        .format("delta")
        .load(
            f"{args.wholesale_container_path}/calculation-input-v2/metering-point-periods"
        )
    )
    batch_grid_areas_df = input.get_batch_grid_areas_df(args.batch_grid_areas, spark)
    input.check_all_grid_areas_have_metering_points(
        batch_grid_areas_df, metering_points_periods_df
    )

    metering_point_periods_df = input.get_metering_point_periods_df(
        metering_points_periods_df,
        batch_grid_areas_df,
        args.batch_period_start_datetime,
        args.batch_period_end_datetime,
    )

    process_step_result_writer = ProcessStepResultWriter(
        spark,
        args.wholesale_container_path,
        args.batch_id,
        args.batch_process_type,
        args.batch_execution_time_start,
    )
    basis_data_writer = BasisDataWriter(args.wholesale_container_path, args.batch_id)
    actors_writer = ActorsWriter(args.wholesale_container_path, args.batch_id)

    calculate_balance_fixing(
        actors_writer,
        basis_data_writer,
        process_step_result_writer,
        metering_point_periods_df,
        timeseries_points_df,
        args.batch_period_start_datetime,
        args.batch_period_end_datetime,
        args.time_zone,
    )


def _start(storage_account_name: str, storage_account_credetial: ClientSecretCredential, time_zone: str, job_args: argparse.Namespace) -> None:
    db_logging.loglevel = job_args.log_level

    if islocked(storage_account_name, storage_account_credetial):
        log("Exiting because storage is locked due to data migrations running.")
        sys.exit(3)

    spark = initialize_spark()

    calculator_args = CalculatorArgs(
        data_storage_account_name=storage_account_name,
        wholesale_container_path=infrastructure.get_container_root_path(
            storage_account_name
        ),
        batch_id=job_args.batch_id,
        batch_grid_areas=job_args.batch_grid_areas,
        batch_period_start_datetime=job_args.batch_period_start_datetime,
        batch_period_end_datetime=job_args.batch_period_end_datetime,
        batch_execution_time_start=job_args.batch_execution_time_start,
        batch_process_type=job_args.batch_process_type,
        time_zone=time_zone,
    )

    _start_calculator(spark, calculator_args)


# The start() method should only have its name updated in correspondence with the wheels entry point for it.
# Further the method must remain parameterless because it will be called from the entry point when deployed.
def start() -> None:
    job_args = _get_valid_args_or_throw(sys.argv[1:])
    log(f"Job arguments: {str(job_args)}")

    time_zone = env_vars.get_time_zone()
    storage_account_name = env_vars.get_storage_account_name()
    credential = env_vars.get_storage_account_credential()
    _start(storage_account_name, credential, time_zone, job_args)
