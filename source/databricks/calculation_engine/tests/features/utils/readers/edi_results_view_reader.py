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

from pyspark.sql import SparkSession, DataFrame

from package.infrastructure.paths import EdiResults


class EdiResultsViewReader:
    @staticmethod
    def read_energy_result_points_per_ga_v1(
        spark: SparkSession,
    ) -> DataFrame:
        return spark.read.format("delta").table(
            f"{EdiResults.DATABASE_NAME}.{EdiResults.ENERGY_RESULT_POINTS_PER_GA_V1_VIEW_NAME}"
        )
