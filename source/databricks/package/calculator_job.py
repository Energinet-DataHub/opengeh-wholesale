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

import configargparse
import package.calculation_input as calculation_input
from configargparse import argparse
from package import (
    calculate_balance_fixing,
    db_logging,
    debug,
    infrastructure,
    initialize_spark,
    log,
)
from package.file_writers.basis_data_writer import BasisDataWriter
from package.file_writers.process_step_result_writer import ProcessStepResultWriter
from pyspark.sql import DataFrame, SparkSession
from pyspark.sql.types import Row

from .args_helper import valid_date, valid_list, valid_log_level
from .calculator_args import CalculatorArgs
from .datamigration import islocked


def _get_valid_args_or_throw(command_line_args: list[str]) -> argparse.Namespace:
    p = configargparse.ArgParser(
        description="Performs domain calculations for submitted batches",
        formatter_class=configargparse.ArgumentDefaultsHelpFormatter,
    )

    # Infrastructure settings
    p.add("--data-storage-account-name", type=str, required=True)
    p.add("--data-storage-account-key", type=str, required=True)
    p.add("--time-zone", type=str, required=True)

    # Run parameters
    p.add("--batch-id", type=str, required=True)
    p.add("--batch-grid-areas", type=valid_list, required=True)
    p.add("--batch-period-start-datetime", type=valid_date, required=True)
    p.add("--batch-period-end-datetime", type=valid_date, required=True)
    p.add("--log-level", type=valid_log_level, help="debug|information", required=True)

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
    batch_grid_areas_df = get_batch_grid_areas_df(args.batch_grid_areas, spark)
    _check_all_grid_areas_have_metering_points(
        batch_grid_areas_df, metering_points_periods_df
    )

    metering_point_periods_df = calculation_input.get_metering_point_periods_df(
        metering_points_periods_df,
        batch_grid_areas_df,
        args.batch_period_start_datetime,
        args.batch_period_end_datetime,
    )

    process_step_result_writer = ProcessStepResultWriter(
        args.wholesale_container_path, args.batch_id
    )
    basis_data_writer = BasisDataWriter(args.wholesale_container_path, args.batch_id)

    calculate_balance_fixing(
        basis_data_writer,
        process_step_result_writer,
        metering_point_periods_df,
        timeseries_points_df,
        args.batch_period_start_datetime,
        args.batch_period_end_datetime,
        args.time_zone,
    )


def get_batch_grid_areas_df(
    batch_grid_areas: list[str], spark: SparkSession
) -> DataFrame:
    return spark.createDataFrame(
        map(lambda x: Row(str(x)), batch_grid_areas), ["GridAreaCode"]
    )


def _check_all_grid_areas_have_metering_points(
    batch_grid_areas_df: DataFrame, master_basis_data_df: DataFrame
) -> None:
    distinct_grid_areas_rows_df = master_basis_data_df.select("GridAreaCode").distinct()
    grid_area_with_no_metering_point_df = batch_grid_areas_df.join(
        distinct_grid_areas_rows_df, "GridAreaCode", "leftanti"
    )

    if grid_area_with_no_metering_point_df.count() > 0:
        grid_areas_to_inform_about = grid_area_with_no_metering_point_df.select(
            "GridAreaCode"
        ).collect()

        grid_area_codes_to_inform_about = map(
            lambda x: x.__getitem__("GridAreaCode"), grid_areas_to_inform_about
        )
        raise Exception(
            f"There are no metering points for the grid areas {list(grid_area_codes_to_inform_about)} in the requested period"
        )


def _start(command_line_args: list[str]) -> None:
    args = _get_valid_args_or_throw(command_line_args)
    log(f"Job arguments: {str(args)}")
    db_logging.loglevel = args.log_level

    if islocked(args.data_storage_account_name, args.data_storage_account_key):
        log("Exiting because storage is locked due to data migrations running.")
        sys.exit(3)

    spark = initialize_spark(
        args.data_storage_account_name, args.data_storage_account_key
    )

    calculator_args = CalculatorArgs(
        data_storage_account_name=args.data_storage_account_name,
        data_storage_account_key=args.data_storage_account_key,
        wholesale_container_path=infrastructure.get_container_root_path(
            args.data_storage_account_name
        ),
        batch_id=args.batch_id,
        batch_grid_areas=args.batch_grid_areas,
        batch_period_start_datetime=args.batch_period_start_datetime,
        batch_period_end_datetime=args.batch_period_end_datetime,
        time_zone=args.time_zone,
    )

    _start_calculator(spark, calculator_args)


# The start() method should only have its name updated in correspondence with the wheels entry point for it.
# Further the method must remain parameterless because it will be called from the entry point when deployed.
def start() -> None:
    _start(sys.argv[1:])
