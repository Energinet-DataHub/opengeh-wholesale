from spark_sql_migrations import Schema, Table

import package.infrastructure.paths as paths
from package.calculation.output.schemas import (
    energy_per_brp_schema_uc,
    energy_per_es_schema_uc,
    grid_loss_metering_point_time_series_schema_uc,
    exchange_per_neighbor_ga_schema_uc,
)
from package.calculation.output.schemas.energy_per_ga_schema_uc import (
    energy_per_ga_schema_uc,
)

from package.calculation.output.schemas.total_monthly_amounts_schema import (
    total_monthly_amounts_schema,
)

schema_config = [
    Schema(
        name=paths.WholesaleResultsInternalDatabase.DATABASE_NAME,
        tables=[
            Table(
                name=paths.WholesaleResultsInternalDatabase.TOTAL_MONTHLY_AMOUNTS_TABLE_NAME,
                schema=total_monthly_amounts_schema,
            ),
            Table(
                name=paths.WholesaleResultsInternalDatabase.ENERGY_PER_GA_TABLE_NAME,
                schema=energy_per_ga_schema_uc,
            ),
            Table(
                name=paths.WholesaleResultsInternalDatabase.ENERGY_PER_BRP_TABLE_NAME,
                schema=energy_per_brp_schema_uc,
            ),
            Table(
                name=paths.WholesaleResultsInternalDatabase.ENERGY_PER_ES_TABLE_NAME,
                schema=energy_per_es_schema_uc,
            ),
            Table(
                name=paths.WholesaleResultsInternalDatabase.EXCHANGE_PER_NEIGHBOR_GA_TABLE_NAME,
                schema=exchange_per_neighbor_ga_schema_uc,
            ),
            Table(
                name=paths.WholesaleResultsInternalDatabase.GRID_LOSS_METERING_POINT_TIME_SERIES_TABLE_NAME,
                schema=grid_loss_metering_point_time_series_schema_uc,
            ),
        ],
        views=[],
    ),
]
