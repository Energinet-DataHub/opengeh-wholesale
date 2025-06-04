import pyspark.sql.functions as f
from dependency_injector.wiring import Provide, inject
from geh_common.telemetry import logging_configuration, use_span
from pyspark.sql import DataFrame
from pyspark.sql.types import StructType

import geh_wholesale.databases.wholesale_results_internal.schemas as schemas
from geh_wholesale.calculation.calculation_output import EnergyResultsOutput
from geh_wholesale.codelists import MeteringPointType
from geh_wholesale.container import Container
from geh_wholesale.databases.table_column_names import TableColumnNames
from geh_wholesale.infrastructure.infrastructure_settings import InfrastructureSettings
from geh_wholesale.infrastructure.paths import (
    WholesaleResultsInternalDatabase,
)


@use_span("calculation.write.energy")
def write_energy_results(energy_results_output: EnergyResultsOutput) -> None:
    """Write each energy result to the output table."""
    print("Writing energy results to Unity Catalog")  # noqa: T201
    # Write exchange per neighbor grid area
    _write(
        "exchange_per_neighbor",
        energy_results_output.exchange_per_neighbor,
        WholesaleResultsInternalDatabase.EXCHANGE_PER_NEIGHBOR_TABLE_NAME,
        schemas.exchange_per_neighbor_schema,
    )

    # Write energy per grid area
    energy = _union(
        energy_results_output.total_consumption,
        energy_results_output.exchange,
        energy_results_output.production,
        energy_results_output.flex_consumption,
        energy_results_output.non_profiled_consumption,
        energy_results_output.temporary_production,
        energy_results_output.temporary_flex_consumption,
        energy_results_output.grid_loss,
    )

    _write(
        "energy",
        energy,
        WholesaleResultsInternalDatabase.ENERGY_TABLE_NAME,
        schemas.energy_schema,
    )

    # Write energy per balance responsible party
    energy_per_brp = _union(
        energy_results_output.production_per_brp,
        energy_results_output.flex_consumption_per_brp,
        energy_results_output.non_profiled_consumption_per_brp,
    )
    _write(
        "energy_per_brp",
        energy_per_brp,
        WholesaleResultsInternalDatabase.ENERGY_PER_BRP_TABLE_NAME,
        schemas.energy_per_brp_schema,
    )

    # Write energy per energy supplier
    energy_per_es = _union(
        energy_results_output.flex_consumption_per_es,
        energy_results_output.production_per_es,
        energy_results_output.non_profiled_consumption_per_es,
    )
    _write(
        "energy_per_es",
        energy_per_es,
        WholesaleResultsInternalDatabase.ENERGY_PER_ES_TABLE_NAME,
        schemas.energy_per_es_schema,
    )

    # Write positive and negative grid loss time series
    positive_grid_loss = energy_results_output.positive_grid_loss
    if energy_results_output.positive_grid_loss:
        positive_grid_loss = energy_results_output.positive_grid_loss.withColumn(
            TableColumnNames.metering_point_type,
            f.lit(MeteringPointType.CONSUMPTION.value),
        )
    negative_grid_loss = energy_results_output.negative_grid_loss
    if energy_results_output.negative_grid_loss:
        negative_grid_loss = energy_results_output.negative_grid_loss.withColumn(
            TableColumnNames.metering_point_type,
            f.lit(MeteringPointType.PRODUCTION.value),
        )
    grid_loss_metering_point_time_series = _union(positive_grid_loss, negative_grid_loss)
    _write(
        "grid_loss_metering_point_time_series",
        grid_loss_metering_point_time_series,
        WholesaleResultsInternalDatabase.GRID_LOSS_METERING_POINT_TIME_SERIES_TABLE_NAME,
        schemas.grid_loss_metering_point_time_series_schema,
    )


def _union(*dfs: DataFrame) -> DataFrame | None:
    """Union multiple dataframes, ignoring None values."""
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
    infrastructure_settings: InfrastructureSettings = Provide[Container.infrastructure_settings],
) -> None:
    with logging_configuration.start_span(name):
        # Not all energy results have a value - it depends on the type of calculation
        if df is None:
            return None

        # Adjust to match the schema
        df = df.select(schema.fieldNames())

        df.write.format("delta").mode("append").option("mergeSchema", "false").insertInto(
            f"{infrastructure_settings.catalog_name}.{WholesaleResultsInternalDatabase.DATABASE_NAME}.{table_name}"
        )
