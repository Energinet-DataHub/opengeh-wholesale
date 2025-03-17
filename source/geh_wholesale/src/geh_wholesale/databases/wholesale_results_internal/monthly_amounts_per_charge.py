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
from dependency_injector.wiring import inject, Provide
from pyspark.sql import DataFrame
from geh_common.telemetry import use_span, logging_configuration

from package.calculation.calculation_output import WholesaleResultsOutput
from package.container import Container
from package.infrastructure.infrastructure_settings import InfrastructureSettings
from package.infrastructure.paths import (
    WholesaleResultsInternalDatabase,
)


@use_span("calculation.write.wholesale")
def write_monthly_amounts_per_charge(
    wholesale_results_output: WholesaleResultsOutput,
) -> None:
    """Write each wholesale result to the output table."""
    _write(
        "monthly_tariff_from_hourly_per_co_es",
        wholesale_results_output.monthly_tariff_from_hourly_per_co_es,
    )
    _write(
        "monthly_tariff_from_daily_per_co_es",
        wholesale_results_output.monthly_tariff_from_daily_per_co_es,
    )
    _write(
        "monthly_subscription_per_co_es",
        wholesale_results_output.monthly_subscription_per_co_es,
    )
    _write(
        "monthly_fee_per_co_es",
        wholesale_results_output.monthly_fee_per_co_es,
    )


@inject
def _write(
    name: str,
    df: DataFrame,
    infrastructure_settings: InfrastructureSettings = Provide[
        Container.infrastructure_settings
    ],
) -> None:

    with logging_configuration.start_span(name):
        df.write.format("delta").mode("append").option(
            "mergeSchema", "false"
        ).insertInto(
            f"{infrastructure_settings.catalog_name}.{WholesaleResultsInternalDatabase.DATABASE_NAME}.{WholesaleResultsInternalDatabase.MONTHLY_AMOUNTS_PER_CHARGE_TABLE_NAME}"
        )
