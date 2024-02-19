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
import concurrent.futures
import os
import uuid
from pathlib import Path
from unittest.mock import Mock

import pandas as pd
import yaml
from pyspark.sql import SparkSession, DataFrame

from business_logic_tests.features.fixtures.scenario_fixture import (
    wholesale_hourly_tariff_per_ga_co_es_results_schema,
    create_expected_result,
)
from package.calculation import PreparedDataReader
from package.calculation.CalculationResults import CalculationResultsContainer
from package.calculation.calculation import _execute_calculation
from package.calculation.calculator_args import CalculatorArgs
from package.calculation_input.schemas import (
    metering_point_period_schema,
    time_series_point_schema,
    grid_loss_metering_points_schema,
    charge_link_periods_schema,
    charge_master_data_periods_schema,
    charge_price_points_schema,
)
from package.codelists import CalculationType
from package.constants import Colname


class ScenarioFactory:

    table_reader: Mock
    calculation_args: CalculatorArgs
    test_path: str
    results: DataFrame

    def __init__(self, spark: SparkSession, file_path: Path):
        self.spark = spark
        self.table_reader = Mock()
        self.test_path = os.path.dirname(file_path) + "/test_data/"

        file_schema_dict = {
            "metering_point_periods.csv": metering_point_period_schema,
            "time_series_points.csv": time_series_point_schema,
            "grid_loss_metering_points.csv": grid_loss_metering_points_schema,
            "charge_master_data_periods.csv": charge_master_data_periods_schema,
            "charge_link_periods.csv": charge_link_periods_schema,
            "charge_price_points.csv": charge_price_points_schema,
            "wholesale_hourly_tariff_per_ga_co_es_results.csv": wholesale_hourly_tariff_per_ga_co_es_results_schema,
        }

        self.calculation_args = self._load_calculation_args()
        frames = self._read_files_in_parallel(
            list(file_schema_dict.keys()), list(file_schema_dict.values())
        )

        self.table_reader.read_metering_point_periods.return_value = frames[0]
        self.table_reader.read_time_series_points.return_value = frames[1]
        self.table_reader.read_grid_loss_metering_points.return_value = frames[2]
        self.table_reader.read_charge_master_data_periods.return_value = frames[3]
        self.table_reader.read_charge_links_periods.return_value = frames[4]
        self.table_reader.read_charge_price_points.return_value = frames[5]
        self.results = create_expected_result(
            self.spark, self.calculation_args, frames[6]
        )

    def execute_scenario(self) -> CalculationResultsContainer:
        return _execute_calculation(
            self.calculation_args, PreparedDataReader(self.table_reader)
        )

    def get_expected_result(self) -> DataFrame:
        return self.results

    def _read_file(
        self, spark_session: SparkSession, file_path: str, schema: str
    ) -> DataFrame:

        if file_path.__contains__("_results.csv"):
            return spark_session.read.csv(
                self.test_path + file_path, header=True, sep=";"
            )
        df = spark_session.read.csv(
            self.test_path + file_path, header=True, schema=schema, sep=";"
        )
        # We need to create the dataframe again, because (nullability) is not being applied when reading the csv
        return spark_session.createDataFrame(df.rdd, schema)

    def _read_files_in_parallel(
        self, file_names: list, schemas: list
    ) -> list[DataFrame]:
        with concurrent.futures.ThreadPoolExecutor() as executor:
            dataframes = list(
                executor.map(
                    self._read_file, [self.spark] * len(file_names), file_names, schemas
                )
            )
        return dataframes

    def _load_calculation_args(self) -> CalculatorArgs:
        with open(self.test_path + "calculation_arguments.yml", "r") as file:
            calculation_args = yaml.safe_load(file)

        date_format = "%Y-%m-%d %H:%M:%S"

        return CalculatorArgs(
            calculation_id=str(uuid.uuid4()),
            calculation_grid_areas=calculation_args[0]["grid_areas"],  # TODO const?
            calculation_period_start_datetime=pd.to_datetime(
                calculation_args[0]["period_start"], format=date_format
            ),
            calculation_period_end_datetime=pd.to_datetime(
                calculation_args[0]["period_end"], format=date_format
            ),
            calculation_type=CalculationType(
                calculation_args[0][Colname.calculation_type]
            ),
            calculation_execution_time_start=pd.to_datetime(
                calculation_args[0][Colname.calculation_execution_time_start],
                format=date_format,
            ),
            time_zone="Europe/Copenhagen",
        )
