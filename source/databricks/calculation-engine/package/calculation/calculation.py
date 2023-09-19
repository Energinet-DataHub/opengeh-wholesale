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

from datetime import timedelta
import time
from pyspark.sql import SparkSession
from package.codelists import ProcessType
import package.calculation_input as input
from package.calculation_output.wholesale_calculation_result_writer import (
    WholesaleCalculationResultWriter,
)
from package.infrastructure import log
from .calculator_args import CalculatorArgs
from .energy import energy_calculation
from .wholesale import wholesale_calculation
from . import setup


def execute(args: CalculatorArgs, spark: SparkSession) -> None:
    calculation_input_reader = input.CalculationInputReader(spark)

    # TIMER #
    start_time = time.time()
    # TIMER #

    (
        metering_point_periods_df,
        time_series_points_df,
        grid_loss_responsible_df,
    ) = input.get_calculation_input(
        calculation_input_reader,
        args.batch_period_start_datetime,
        args.batch_period_end_datetime,
        args.batch_grid_areas,
    )

    # TIMER ##
    duration = time.time() - start_time
    log(f"get_calculation_input took: {str(timedelta(seconds=duration))} ")
    start_time = time.time()
    # TIMER ##

    enriched_time_series_point_df = setup.get_enriched_time_series_points_df(
        time_series_points_df,
        metering_point_periods_df,
        args.batch_period_start_datetime,
        args.batch_period_end_datetime,
    )

    # TIMER #
    duration = time.time() - start_time
    log(f"get_enriched_time_series_points_df took: {str(timedelta(seconds=duration))}")
    # TIMER #

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
        wholesale_calculation_result_writer = WholesaleCalculationResultWriter(
            args.batch_id, args.batch_process_type, args.batch_execution_time_start
        )

        wholesale_calculation.execute(
            calculation_input_reader,
            wholesale_calculation_result_writer,
            metering_point_periods_df,
            time_series_points_df,
        )
