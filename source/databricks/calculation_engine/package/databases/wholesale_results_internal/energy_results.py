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

from dependency_injector.wiring import inject, Provide
from pyspark.sql import DataFrame
import pyspark.sql.functions as f
from pyspark.sql.types import StructType

import package.databases.wholesale_results_internal.schemas as schemas
from package.calculation.calculation_results import (
    EnergyResultsContainer,
)
from package.databases.wholesale_results_internal.energy_result_column_names import (
    EnergyResultColumnNames,
)
from package.databases.wholesale_results_internal.schemas import (
    hive_energy_results_schema,
)
from package.codelists import MeteringPointType
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

    print("Writing energy results to Unity Catalog")

    # Write exchange per neighbor grid area
    _write(
        "exchange_per_neighbor",
        energy_results.exchange_per_neighbor,
        WholesaleResultsInternalDatabase.EXCHANGE_PER_NEIGHBOR_TABLE_NAME,
        schemas.exchange_per_neighbor_schema_uc,
    )

    # Write energy per grid area
    energy = _union(
        energy_results.total_consumption,
        energy_results.exchange,
        energy_results.production,
        energy_results.flex_consumption,
        energy_results.non_profiled_consumption,
        energy_results.temporary_production,
        energy_results.temporary_flex_consumption,
        energy_results.grid_loss,
    )
    _write(
        "energy",
        energy,
        WholesaleResultsInternalDatabase.ENERGY_TABLE_NAME,
        schemas.energy_schema_uc,
    )

    # Write energy per balance responsible party
    energy_per_brp = _union(
        energy_results.production_per_brp,
        energy_results.flex_consumption_per_brp,
        energy_results.non_profiled_consumption_per_brp,
    )
    _write(
        "energy_per_brp",
        energy_per_brp,
        WholesaleResultsInternalDatabase.ENERGY_PER_BRP_TABLE_NAME,
        schemas.energy_per_brp_schema_uc,
    )

    # Write energy per energy supplier
    energy_per_es = _union(
        energy_results.flex_consumption_per_es,
        energy_results.production_per_es,
        energy_results.non_profiled_consumption_per_es,
    )
    _write(
        "energy_per_es",
        energy_per_es,
        WholesaleResultsInternalDatabase.ENERGY_PER_ES_TABLE_NAME,
        schemas.energy_per_es_schema_uc,
    )

    # Write positive and negative grid loss time series
    positive_grid_loss = energy_results.positive_grid_loss
    if energy_results.positive_grid_loss:
        positive_grid_loss = energy_results.positive_grid_loss.withColumn(
            EnergyResultColumnNames.metering_point_type,
            f.lit(MeteringPointType.CONSUMPTION.value),
        )
    negative_grid_loss = energy_results.negative_grid_loss
    if energy_results.negative_grid_loss:
        negative_grid_loss = energy_results.negative_grid_loss.withColumn(
            EnergyResultColumnNames.metering_point_type,
            f.lit(MeteringPointType.PRODUCTION.value),
        )
    grid_loss_metering_point_time_series = _union(
        positive_grid_loss, negative_grid_loss
    )
    _write(
        "grid_loss_metering_point_time_series",
        grid_loss_metering_point_time_series,
        WholesaleResultsInternalDatabase.GRID_LOSS_METERING_POINT_TIME_SERIES_TABLE_NAME,
        schemas.grid_loss_metering_point_time_series_schema_uc,
    )

    # TODO BJM: Remove when we're on Unity Catalog
    print("Writing energy results to Hive")
    for field in fields(energy_results):
        _write_to_hive(field.name, getattr(energy_results, field.name))


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
    schema: StructType,
    infrastructure_settings: InfrastructureSettings = Provide[
        Container.infrastructure_settings
    ],
) -> None:
    with logging_configuration.start_span(name):

        # Not all energy results have a value - it depends on the type of calculation
        if df is None:
            return None

        # Adjust to match the schema
        df = (
            df.withColumnRenamed(
                EnergyResultColumnNames.balance_responsible_id,
                EnergyResultColumnNames.balance_responsible_party_id,
            )
            .withColumnRenamed(
                EnergyResultColumnNames.calculation_result_id,
                EnergyResultColumnNames.result_id,
            )
            .select(schema.fieldNames())
        )

        df.write.format("delta").mode("append").option(
            "mergeSchema", "false"
        ).insertInto(
            f"{infrastructure_settings.catalog_name}.{WholesaleResultsInternalDatabase.DATABASE_NAME}.{table_name}"
        )


def _write_to_hive(name: str, df: DataFrame) -> None:
    with logging_configuration.start_span(name):
        # Not all energy results have a value - it depends on the type of calculation
        if df is None:
            return None

        df = df.select(hive_energy_results_schema.fieldNames())

        df.write.format("delta").mode("append").option(
            "mergeSchema", "false"
        ).insertInto(
            f"{paths.HiveOutputDatabase.DATABASE_NAME}.{paths.HiveOutputDatabase.ENERGY_RESULT_TABLE_NAME}"
        )
