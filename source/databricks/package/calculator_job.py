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

from datetime import datetime
import sys
from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.functions import col
import ast

# Required when executing in a subprocess from pytest (without using wheel)
sys.path.append(r"/workspaces/opengeh-wholesale/source/databricks")

from package import (
    calculate_balance_fixing_total_production,
    initialize_spark,
    log,
    debug,
    db_logging,
)

import configargparse


def _valid_date(s):
    """See https://stackoverflow.com/questions/25470844/specify-date-format-for-python-argparse-input-arguments"""
    try:
        return datetime.strptime(s, "%Y-%m-%dT%H:%M:%SZ")
    except ValueError:
        msg = "not a valid date: {0!r}".format(s)
        raise configargparse.ArgumentTypeError(msg)


def _valid_list(s):
    """See https://stackoverflow.com/questions/25470844/specify-date-format-for-python-argparse-input-arguments"""
    try:
        return ast.literal_eval(s)
    except ValueError:
        msg = "not a valid grid area list"
        raise configargparse.ArgumentTypeError(msg)


def _valid_log_level(s):
    if s in ["information", "debug"]:
        return str(s)
    else:
        msg = "loglevel is not valid"
        raise configargparse.ArgumentTypeError(msg)


def _get_valid_args_or_throw():
    p = configargparse.ArgParser(
        description="Performs domain calculations for submitted batches",
        formatter_class=configargparse.ArgumentDefaultsHelpFormatter,
    )

    # Infrastructure settings
    p.add("--data-storage-account-name", type=str, required=True)
    p.add("--data-storage-account-key", type=str, required=True)
    p.add("--integration-events-path", type=str, required=True)
    p.add("--time-series-points-path", type=str, required=True)
    p.add("--process-results-path", type=str, required=True)
    p.add("--time-zone", type=str, required=True)

    # Run parameters
    p.add("--batch-id", type=str, required=True)
    p.add("--batch-snapshot-datetime", type=_valid_date, required=True)
    p.add("--batch-grid-areas", type=_valid_list, required=True)
    p.add("--batch-period-start-datetime", type=_valid_date, required=True)
    p.add("--batch-period-end-datetime", type=_valid_date, required=True)
    p.add(
        "--log-level",
        type=_valid_log_level,
        help="debug|information",
    )

    p.add(
        "--only-validate-args",
        type=bool,
        required=False,
        default=False,
        help="Instruct the job to exit after validating input arguments.",
    )

    args, unknown_args = p.parse_known_args()
    if len(unknown_args):
        unknown_args_text = ", ".join(unknown_args)
        raise Exception(f"Unknown args: {unknown_args_text}")

    if type(args.batch_grid_areas) is not list:
        raise Exception("Grid areas must be a list")

    return args


def write_time_series_basis_data_to_csv(
    data_df: DataFrame, process_results_path: str, batch_id: str, resolution_type: str
):
    (
        data_df.withColumnRenamed("GridAreaCode", "grid_area")
        .repartition("grid_area")
        .write.mode("overwrite")
        .partitionBy("grid_area")
        .option("header", True)
        .csv(f"{process_results_path}/basis-data/batch_id={batch_id}/{resolution_type}")
    )


def internal_start(spark: SparkSession, args):
    # Merge schema is expensive according to the Spark documentation.
    # Might be a candidate for future performance optimization initiatives.
    # Only events stored before the snapshot_datetime are needed.
    raw_integration_events_df = spark.read.option("mergeSchema", "true").parquet(
        args.integration_events_path
    )

    # Only points stored before the snapshot_datetime are needed.
    raw_time_series_points_df = spark.read.option("mergeSchema", "true").parquet(
        args.time_series_points_path
    )

    (result_df, timeseries_basis_data_df) = calculate_balance_fixing_total_production(
        raw_integration_events_df,
        raw_time_series_points_df,
        args.batch_id,
        args.batch_grid_areas,
        args.batch_snapshot_datetime,
        args.batch_period_start_datetime,
        args.batch_period_end_datetime,
        args.time_zone,
    )

    debug("raw_timeseries", raw_time_series_points_df)

    (timeseries_quarter_df, timeseries_hour_df) = timeseries_basis_data_df
    debug("timeseries basis data df_hour", timeseries_hour_df)
    debug("timeseries basis data df_quarter", timeseries_quarter_df)

    write_time_series_basis_data_to_csv(
        timeseries_quarter_df,
        args.process_results_path,
        args.batch_id,
        "time-series-quarter",
    )

    write_time_series_basis_data_to_csv(
        timeseries_hour_df,
        args.process_results_path,
        args.batch_id,
        "time-series-hour",
    )

    # First repartition to co-locate all rows for a grid area on a single executor.
    # This ensures that only one file is being written/created for each grid area
    # when writing/creating the files. The partition by creates a folder for each grid area.
    # result/
    (
        result_df.withColumnRenamed("GridAreaCode", "grid_area")
        .withColumn("quantity", col("quantity").cast("string"))
        .repartition("grid_area")
        .write.mode("overwrite")
        .partitionBy("grid_area")
        .json(f"{args.process_results_path}/batch_id={args.batch_id}")
    )


# The start() method should only have its name updated in correspondence with the wheels entry point for it.
# Further the method must remain parameterless because it will be called from the entry point when deployed.
def start():
    args = _get_valid_args_or_throw()
    log(f"Job arguments: {str(args)}")
    db_logging.loglevel = args.log_level
    if args.only_validate_args:
        exit(0)

    spark = initialize_spark(
        args.data_storage_account_name, args.data_storage_account_key
    )

    internal_start(spark, args)


if __name__ == "__main__":
    start()
