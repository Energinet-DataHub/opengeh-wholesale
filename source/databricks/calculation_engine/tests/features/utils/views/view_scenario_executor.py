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
from typing import Tuple

from pyspark.sql import SparkSession

from features.utils.csv_to_dataframe_parser import CsvToDataframeParser
from features.utils.views.dataframe_wrapper import DataframeWrapper
from features.utils.views.view_input_specifications import get_input_specifications
from features.utils.views.view_output_specifications import get_output_specifications


class ViewScenarioExecutor:
    parser: CsvToDataframeParser

    def __init__(self, spark: SparkSession):
        self.spark = spark
        self.parser = CsvToDataframeParser(spark)

    def execute(
        self, scenario_folder_path: str
    ) -> Tuple[list[DataframeWrapper], list[DataframeWrapper]]:

        input_specifications = get_input_specifications()
        output_specifications = get_output_specifications()

        input_dataframes_wrappers = self.parser.parse_csv_files_concurrently(
            f"{scenario_folder_path}/input", input_specifications
        )

        input_dataframes_wrappers = self.correct_dataframe_types(
            input_dataframes_wrappers, input_specifications
        )
        self._write_to_tables(input_dataframes_wrappers, input_specifications)

        output_dataframe_wrappers = self.parser.parse_csv_files_concurrently(
            f"{scenario_folder_path}/output", output_specifications
        )

        expected = self.correct_dataframe_types(
            output_dataframe_wrappers, output_specifications
        )

        actual = self._read_from_views(output_specifications, output_dataframe_wrappers)
        return actual, expected

    @staticmethod
    def _write_to_tables(
        input_dataframe_wrappers: list[DataframeWrapper],
        specifications: dict[str, tuple],
    ) -> None:
        for wrapper in input_dataframe_wrappers:
            database_name = specifications[wrapper.key][3]
            wrapper.df.write.format("delta").mode("overwrite").saveAsTable(
                f"{database_name}.{wrapper.name}"
            )

    def _read_from_views(
        self,
        output_specifications: dict[str, tuple],
        output_dataframe_wrappers: list[DataframeWrapper],
    ) -> list[DataframeWrapper]:

        wrappers = []
        for wrapper in output_dataframe_wrappers:
            read_df_method = output_specifications[wrapper.key][1]
            df = read_df_method(self.spark)
            dataframe_wrapper = DataframeWrapper(
                key=wrapper.key, name=wrapper.name, df=df
            )
            wrappers.append(dataframe_wrapper)

        return wrappers

    def correct_dataframe_types(
        self,
        dataframe_wrappers: list[DataframeWrapper],
        output_specifications: dict[str, tuple],
    ) -> list[DataframeWrapper]:
        wrappers = []
        for wrapper in dataframe_wrappers:
            if wrapper.df is None:
                continue
            correction_method = output_specifications[wrapper.key][2]
            wrapper.df = correction_method(self.spark, wrapper.df)
            wrappers.append(wrapper)

        return wrappers
