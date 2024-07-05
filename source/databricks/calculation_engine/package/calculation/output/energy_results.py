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
import pyspark.sql.functions as f
from dependency_injector.wiring import inject, Provide
from pyspark.sql import DataFrame

from package.calculation.calculation_results import (
    EnergyResultsContainer,
)
from package.codelists import AggregationLevel
from package.constants import EnergyResultColumnNames
from package.container import Container
from package.infrastructure import logging_configuration
from package.infrastructure import paths
from package.infrastructure.infrastructure_settings import InfrastructureSettings
from package.infrastructure.paths import (
    WholesaleResultsInternalDatabase,
)


@logging_configuration.use_span("calculation.write.energy")
def write_energy_results(energy_results: EnergyResultsContainer) -> None:
    """Write each energy result to the output table."""

    _write(
        "net_exchange_per_neighbor_ga",
        energy_results.net_exchange_per_neighbor_ga,
        WholesaleResultsInternalDatabase.EXCHANGE_PER_NEIGHBOR_GA_TABLE_NAME,
    )

    energy_per_ga = _union(
        energy_results.total_consumption,
        energy_results.net_exchange_per_ga,
        energy_results.production_per_ga,
        energy_results.flex_consumption_per_ga,
        energy_results.non_profiled_consumption_per_ga,
        energy_results.temporary_production_per_ga,
        energy_results.temporary_flex_consumption_per_ga,
        energy_results.grid_loss,
    )
    _write(
        "energy_per_ga",
        energy_per_ga,
        WholesaleResultsInternalDatabase.ENERGY_PER_GA_TABLE_NAME,
    )

    energy_per_brp = _union(
        energy_results.production_per_ga_and_brp,
        energy_results.flex_consumption_per_ga_and_brp,
        energy_results.non_profiled_consumption_per_ga_and_brp,
    )
    _write(
        "energy_per_brp",
        energy_per_brp,
        WholesaleResultsInternalDatabase.ENERGY_PER_BRP_TABLE_NAME,
    )

    energy_per_es = _union(
        energy_results.production_per_ga_and_es,
        energy_results.flex_consumption_per_ga_and_es,
        energy_results.flex_consumption_per_ga_and_brp_and_es,
        energy_results.production_per_ga_and_brp_and_es,
        energy_results.non_profiled_consumption_per_ga_and_es,
        energy_results.non_profiled_consumption_per_ga_and_brp_and_es,
    )
    _write(
        "energy_per_es",
        energy_per_es,
        WholesaleResultsInternalDatabase.ENERGY_PER_ES_TABLE_NAME,
    )

    grid_loss_metering_point_time_series = _union(
        energy_results.positive_grid_loss,
        energy_results.negative_grid_loss,
    )
    _write(
        "grid_loss_metering_point_time_series",
        grid_loss_metering_point_time_series,
        WholesaleResultsInternalDatabase.GRID_LOSS_METERING_POINT_TIME_SERIES_TABLE_NAME,
    )


def _union(*dfs: DataFrame) -> DataFrame | None:
    """
    Union multiple dataframes, ignoring None values.
    """
    not_none_dfs = [df for df in dfs if df is not None]

    if len(not_none_dfs) == 0:
        return None

    result = not_none_dfs[0]
    for df in not_none_dfs[1:]:
        result = result.union(df)
    return result


@inject
def _write(
    name: str,
    df: DataFrame,
    table_name: str,
    infrastructure_settings: InfrastructureSettings = Provide[
        Container.infrastructure_settings
    ],
) -> None:
    with logging_configuration.start_span(name):

        # Not all energy results have a value - it depends on the type of calculation
        if df is None:
            return None

        df.write.format("delta").mode("append").option(
            "mergeSchema", "false"
        ).insertInto(
            f"{infrastructure_settings.catalog_name}.{WholesaleResultsInternalDatabase.DATABASE_NAME}.{table_name}"
        )

        # TODO BJM: Remove when we're on Unity Catalog
        df.write.format("delta").mode("append").option(
            "mergeSchema", "false"
        ).insertInto(
            f"{paths.HiveOutputDatabase.DATABASE_NAME}.{paths.HiveOutputDatabase.ENERGY_RESULT_TABLE_NAME}"
        )
