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
from pathlib import Path
from typing import Tuple

from pyspark.sql import SparkSession

from features.utils.dataframes.basis_data_results_dataframe import (
    create_result_dataframe,
)
from features.utils.expected_output import ExpectedOutput
from features.utils.views.pol import InputDataframe
from features.utils.views.view_input_specifications import get_input_specifications
from features.utils.views.view_output_specifications import get_output_specifications
from views.view_reader import ViewReader


class ViewScenarioExecutor:
    view_reader: ViewReader
    dataframes: list = []
    scenario_path: str

    def __init__(self, spark: SparkSession):
        self.spark = spark
        self.view_reader = ViewReader(spark)

    def execute(
        self, scenario_folder_path: str
    ) -> Tuple[list[ExpectedOutput], list[ExpectedOutput]]:
        self.scenario_path = scenario_folder_path

        input_specifications = get_input_specifications()
        output_specifications = get_output_specifications()

        input_dataframes = self._read_files_in_parallel(input_specifications)
        expected = self._read_files_in_parallel(output_specifications)

        self._write_to_tables(input_dataframes)
        actual = self._read_from_views(output_specifications)

        return actual, expected

    @staticmethod
    def _write_to_tables(input_dataframes: list[InputDataframe]) -> None:
        for i in input_dataframes:
            i.df.write.format("delta").mode("overwrite").saveAsTable(
                f"{i.database_name}.{i.name}"
            )

    def _read_file(
        self,
        spark_session: SparkSession,
        csv_file_name: str,
        schema: str,
        database: str,
    ) -> InputDataframe | None:

        path = self.scenario_path + csv_file_name
        if not os.path.exists(path):
            return None

        df = spark_session.read.csv(path, header=True, sep=";", schema=schema)

        # We need to create the dataframe again because nullability and precision
        # are not applied when reading the csv file.
        df = spark_session.createDataFrame(df.rdd, schema)
        return InputDataframe(name=csv_file_name, df=df, database_name=database)

    def _read_files_in_parallel(
        self, correlations: dict[str, tuple]
    ) -> list[InputDataframe]:
        """
        Reads all the csv files in parallel and converts them to dataframes.
        """
        schemas = [t[0] for t in correlations.values()]
        databaser = [t[1] for t in correlations.values()]

        with concurrent.futures.ThreadPoolExecutor() as executor:
            dataframes = list(
                executor.map(
                    self._read_file,
                    [self.spark] * len(correlations.keys()),
                    correlations.keys(),
                    schemas,
                    databaser,
                )
            )
        return dataframes

    def get_output(self, spark: SparkSession) -> list[ExpectedOutput]:
        expected_results = []
        expected_result_file_paths = (
            self._get_paths_to_expected_result_files_in_output_folder()
        )

        if len(expected_result_file_paths) == 0:
            raise Exception("Missing expected result files in output folder.")

        for result_file in expected_result_file_paths:
            raw_df = spark.read.csv(result_file[1], header=True, sep=";")
            df = create_result_dataframe(spark, raw_df, result_file[0])
            expected_results.append(ExpectedOutput(name=result_file[0], df=df))

        return expected_results

    def _get_paths_to_expected_result_files_in_output_folder(
        self,
    ) -> list[Tuple[str, str]]:
        """Returns (file base name without extension, file full path)."""

        output_folder_path = Path(self.output_path)
        csv_files = list(output_folder_path.rglob("*.csv"))
        return [(Path(file).stem, str(file)) for file in csv_files]

    def _read_actual(self, view_reader):
        pass

    def _read_from_views(self, output_specifications):
        pass
