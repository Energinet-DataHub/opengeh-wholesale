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
from pyspark.sql import SparkSession
from package.calculation import energy_calculation, wholesale_calculation
from package.infrastructure import (
from package import (
    db_logging,
    initialize_spark,
    log,
)
import package.calculation_input as input
from package.infrastructure.calculator_args import CalculatorArgs, get_calculator_args
import package.calculation.setup as setup
from package.infrastructure.storage_account_access import islocked


def _start_calculator(args: CalculatorArgs, spark: SparkSession) -> None:

    calculation_input_reader = input.CalculationInputReader(spark)

    metering_point_periods_df, time_series_points_df, grid_loss_responsible_df = input.get_calculation_input(
        calculation_input_reader,
        args.batch_period_start_datetime,
        args.batch_period_end_datetime,
        args.batch_grid_areas,
    )

    enriched_time_series_point_df = setup.get_enriched_time_series_points_df(
        time_series_points_df,
        metering_point_periods_df,
        args.batch_period_start_datetime,
        args.batch_period_end_datetime,
    )

    energy_calculation.execute(
        args.batch_id,
        args.batch_process_type,
        args.batch_execution_time_start,
        args.wholesale_container_path,
        metering_point_periods_df,
        enriched_time_series_point_df,
        grid_loss_responsible_df,
        args.time_zone,
    )

    if args.batch_process_type == ProcessType.WHOLESALE_FIXING:
        wholesale_calculation.execute(
            calculation_input_reader,
            metering_point_periods_df,
            time_series_points_df,
        )


# The start() method should only have its name updated in correspondence with the wheels entry point for it.
# Further the method must remain parameterless because it will be called from the entry point when deployed.
def start() -> None:

    args = get_calculator_args()

    spark = initialize_spark()

    db_logging.loglevel = "information"
    if islocked(args.data_storage_account_name, args.data_storage_account_credentials):
        log("Exiting because storage is locked due to data migrations running.")
        sys.exit(3)

    _start_calculator(args, spark)
