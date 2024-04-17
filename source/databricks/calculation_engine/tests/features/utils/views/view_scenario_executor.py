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
import os
from typing import Tuple

from pyspark.sql import SparkSession

from features.utils.csv_to_dataframe_parser import CsvToDataframeParser
from features.utils.expected_output import ExpectedOutput
from features.utils.readers.settlement_report_view_reader import (
    SettlementReportViewReader,
)
from features.utils.views.view_input_specifications import get_input_specifications
from features.utils.views.view_output_specifications import get_output_specifications
from package.infrastructure.paths import BASIS_DATA_DATABASE_NAME


class ViewScenarioExecutor:
    view_reader: SettlementReportViewReader

    def __init__(self, spark: SparkSession):
        self.spark = spark
        self.view_reader = SettlementReportViewReader(spark)

    def execute(
        self, scenario_folder_path: str
    ) -> Tuple[list[ExpectedOutput], list[ExpectedOutput]]:

        input_specifications = get_input_specifications()
        output_specifications = get_output_specifications()

        parser = CsvToDataframeParser(self.spark)

        input_dataframes = parser.parse_csv_files_concurrently(
            f"{scenario_folder_path}/input", input_specifications
        )
        self._write_to_tables(input_dataframes)

        output_dataframes = parser.parse_csv_files_concurrently(
            f"{scenario_folder_path}/output", output_specifications, ignore_schema=True
        )

        expected = self.correct_dataframe_types(
            output_dataframes, output_specifications
        )
        actual = self._read_from_views(output_specifications)

        return actual, expected

    @staticmethod
    def _write_to_tables(input_dataframes: list[ExpectedOutput]) -> None:
        for i in input_dataframes:
            i.df.write.format("delta").mode("overwrite").saveAsTable(
                f"{BASIS_DATA_DATABASE_NAME}.{i.name}"
            )

    def _read_from_views(
        self, output_specifications: dict[str, tuple]
    ) -> list[ExpectedOutput]:

        outputs = []
        for key in output_specifications:
            value = output_specifications[key]
            read_method = getattr(self.view_reader, value[1])
            df = read_method()
            name, extension = os.path.splitext(key)
            container = ExpectedOutput(name=name, df=df)
            outputs.append(container)

        return outputs

    def correct_dataframe_types(
        self, dataframes: list[ExpectedOutput], output_specifications: dict[str, tuple]
    ) -> list[ExpectedOutput]:
        frames = []
        for key in dataframes:
            value = output_specifications[key.name + ".csv"]
            correction_method = value[2]
            key.df = correction_method(key.df, self.spark)
            frames.append(key)

        return frames
    