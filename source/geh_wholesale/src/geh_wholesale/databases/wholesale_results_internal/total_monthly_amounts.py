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
from dependency_injector.wiring import Provide, inject
from geh_common.telemetry import logging_configuration, use_span
from pyspark.sql import DataFrame

from geh_wholesale.calculation.calculation_output import WholesaleResultsOutput
from geh_wholesale.container import Container
from geh_wholesale.infrastructure.infrastructure_settings import InfrastructureSettings
from geh_wholesale.infrastructure.paths import (
    WholesaleResultsInternalDatabase,
)


@use_span("calculation.write.wholesale")
def write_total_monthly_amounts(
    wholesale_results_output: WholesaleResultsOutput,
) -> None:
    _write(
        "total_monthly_amounts_per_co_es",
        wholesale_results_output.total_monthly_amounts_per_co_es,
    )
    _write(
        "total_monthly_amounts_per_es",
        wholesale_results_output.total_monthly_amounts_per_es,
    )


@inject
def _write(
    name: str,
    df: DataFrame,
    infrastructure_settings: InfrastructureSettings = Provide[Container.infrastructure_settings],
) -> None:
    with logging_configuration.start_span(name):
        df.write.format("delta").mode("append").option("mergeSchema", "false").insertInto(
            f"{infrastructure_settings.catalog_name}.{WholesaleResultsInternalDatabase.DATABASE_NAME}.{WholesaleResultsInternalDatabase.TOTAL_MONTHLY_AMOUNTS_TABLE_NAME}"
        )
