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

from package import calculator, initialize_spark
import configargparse


def start():
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

    # Run parameters
    p.add("--batch-id", type=str, required=True)
    p.add("--snapshot-datetime", type=str, required=True)
    p.add("--grid-areas", type=str, required=True)
    p.add("--period-start-datetime", type=str, required=True)
    p.add("--period-end-datetime", type=str, required=True)

    args, unknown_args = p.parse_known_args()

    spark = initialize_spark(
        args.data_storage_account_name, args.data_storage_account_key
    )

    raw_integration_events_df = spark.read.option("mergeSchema", "true").parquet(
        args.integration_events_path
    )
    raw_time_series_points = spark.read.option("mergeSchema", "true").parquet(
        args.time_series_points_path
    )

    output_df = calculate_balance_fixing_total_production(
        raw_integration_events_df,
        raw_time_series_points,
        args.batch_id,
        args.grid_areas,
        args.snapshot_datetime,
        args.period_start_datetime,
        args.period_end_datetime
    )

    (output_df
        .repartition("grid-area")
        .write
        .mode("overwrite")
        .partitionBy("grid-area")
        .json(f"{args.process_results_path}/batch-id={batch_id}")
    )
