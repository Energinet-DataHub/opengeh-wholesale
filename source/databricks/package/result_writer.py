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

# import sys
# import configargparse
# from .calculator_args import CalculatorArgs
# from .args_helper import valid_date, valid_list, valid_log_level
# from .datamigration import islocked
# import package.calculation_input as calculation_input
from pyspark.sql import DataFrame, SparkSession

# from pyspark.sql.functions import col
# from pyspark.sql.types import Row
# from configargparse import argparse
# from package.constants import Colname
# from package import (
#     calculate_balance_fixing,
#     db_logging,
#     debug,
#     infrastructure,
#     initialize_spark,
#     log,
# )


class ResultWriter:
    def __init__(
        self,
        batch_id: str,
        results_path: str,
    ):
        self.batch_id = batch_id
        self.result_path = results_path

    def write_basis_data(self, data_df: DataFrame) -> None:
        data_df.withColumnRenamed("GridAreaCode", "grid_area")

        basis_data_directory = f"{self.result_path}/batch_id={self.batch_id}/basis_data"

        self._write_basis_data_to_csv(f"{basis_data_directory}/time_series_quarter")
        self._write_basis_data_to_csv(f"{basis_data_directory}/time_series_hour")
        self._write_basis_data_to_csv(f"{basis_data_directory}/master_basis_data")

    def _write_basis_data_to_csv(self, path: str) -> None:
        data_df.repartition("grid_area").write.mode("overwrite").partitionBy(
            "grid_area", Colname.gln
        ).option("header", True).csv(path)


# def _start_calculator(spark: SparkSession, args: CalculatorArgs) -> None:
#     timeseries_points_df = (
#         spark.read.option("mode", "FAILFAST")
#         .format("delta")
#         .load(
#             f"{args.wholesale_container_path}/calculation-input-v2/time-series-points"
#         )
#     )
#     metering_points_periods_df = (
#         spark.read.option("mode", "FAILFAST")
#         .format("delta")
#         .load(
#             f"{args.wholesale_container_path}/calculation-input-v2/metering-point-periods"
#         )
#     )
#     batch_grid_areas_df = get_batch_grid_areas_df(args.batch_grid_areas, spark)
#     _check_all_grid_areas_have_metering_points(
#         batch_grid_areas_df, metering_points_periods_df
#     )

#     metering_point_periods_df = calculation_input.get_metering_point_periods_df(
#         metering_points_periods_df,
#         batch_grid_areas_df,
#         args.batch_period_start_datetime,
#         args.batch_period_end_datetime,
#     )

#     (
#         non_profiled_consumption_per_ga_and_es,
#         production_per_ga_df,
#         timeseries_basis_data_df,
#         master_basis_data_df,
#     ) = calculate_balance_fixing(
#         metering_point_periods_df,
#         timeseries_points_df,
#         args.batch_period_start_datetime,
#         args.batch_period_end_datetime,
#         args.time_zone,
#     )

#     (timeseries_quarter_df, timeseries_hour_df) = timeseries_basis_data_df
#     debug("timeseries basis data df_hour", timeseries_hour_df)
#     debug("timeseries basis data df_quarter", timeseries_quarter_df)
#     debug("master basis data", master_basis_data_df)

#     write_basis_data_to_csv(
#         timeseries_quarter_df,
#         f"{args.process_results_path}/batch_id={args.batch_id}/basis_data/time_series_quarter",
#     )

#     write_basis_data_to_csv(
#         timeseries_hour_df,
#         f"{args.process_results_path}/batch_id={args.batch_id}/basis_data/time_series_hour",
#     )

#     write_basis_data_to_csv(
#         master_basis_data_df,
#         f"{args.process_results_path}/batch_id={args.batch_id}/basis_data/master_basis_data",
#     )

#     path = f"{args.process_results_path}/batch_id={args.batch_id}/result"

#     # First repartition to co-locate all rows for a grid area on a single executor.
#     # This ensures that only one file is being written/created for each grid area
#     # When writing/creating the files. The partition by creates a folder for each grid area.

#     # Total production
#     (
#         production_per_ga_df.repartition("grid_area")
#         .write.mode("overwrite")
#         .partitionBy("grid_area", Colname.gln, Colname.time_series_type)
#         .json(path)
#     )

#     # Non-profiled consumption
#     (
#         non_profiled_consumption_per_ga_and_es.repartition("grid_area")
#         .write.mode("append")
#         .partitionBy("grid_area", Colname.gln, Colname.time_series_type)
#         .json(path)
#     )

#     (
#         non_profiled_consumption_per_ga_and_es.repartition("grid_area")
#         .write.mode("append")
#         .partitionBy("grid_area", Colname.gln, Colname.time_series_type)
#         .json(path, compression="gzip")
#     )
