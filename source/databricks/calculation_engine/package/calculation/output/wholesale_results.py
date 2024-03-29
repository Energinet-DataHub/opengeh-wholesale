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
from dataclasses import fields

from pyspark.sql import DataFrame

from package.calculation.calculation_results import WholesaleResultsContainer
from package.infrastructure import logging_configuration
from package.infrastructure.paths import (
    OUTPUT_DATABASE_NAME,
    WHOLESALE_RESULT_TABLE_NAME,
)


@logging_configuration.use_span("calculation.write.wholesale")
def write_wholesale_results(wholesale_results: WholesaleResultsContainer) -> None:
    """Write each wholesale result to the output table."""
    for field in fields(wholesale_results):
        _write(field.name, getattr(wholesale_results, field.name))


def _write(name: str, df: DataFrame) -> None:
    with logging_configuration.start_span(name):
        df.write.format("delta").mode("append").option(
            "mergeSchema", "false"
        ).insertInto(f"{OUTPUT_DATABASE_NAME}.{WHOLESALE_RESULT_TABLE_NAME}")
