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

from pyspark.sql import DataFrame, SparkSession

from calculation_logic.features.energy_calculations.results_dataframe import (
    create_result_dataframe,
)


def get_expected(*args) -> DataFrame:  # type: ignore
    spark: SparkSession = args[0]
    expected_dataframe: DataFrame = args[1]

    return create_result_dataframe(spark, expected_dataframe)
