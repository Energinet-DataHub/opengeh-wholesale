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
from dependency_injector.wiring import inject
from pyspark.sql import DataFrame
from pyspark.sql import functions as f

from package.constants import Colname
from package.infrastructure import logging_configuration, paths


@logging_configuration.use_span("calculation.write-succeeded-calculation")
@inject
def write_calculation(calculations: DataFrame) -> None:
    """Writes the succeeded calculation to the calculations table."""

    calculations = calculations.withColumn(
        Colname.calculation_execution_time_end, f.current_timestamp()
    )

    calculations.write.format("delta").mode("append").option(
        "mergeSchema", "false"
    ).insertInto(f"{paths.BASIS_DATA_DATABASE_NAME}.{paths.CALCULATIONS_TABLE_NAME}")
