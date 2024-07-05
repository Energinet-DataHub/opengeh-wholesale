from spark_sql_migrations import Schema, Table

import package.infrastructure.paths as paths

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
                name=paths.WholesaleResultsInternalDatabase.MONTHLY_AMOUNTS_PER_CHARGE_TABLE_NAME,
                schema=total_monthly_amounts_schema,
            ),
        ],
        views=[],
    ),
    Schema(
        name=paths.WholesaleBasisDataDatabase.DATABASE_NAME,
        tables=[
            Table(
                name=paths.WholesaleBasisDataDatabase.METERING_POINT_PERIODS_TABLE_NAME,
                schema=basis_data_schemas.metering_point_period_schema,
            ),
            Table(
                name=paths.WholesaleBasisDataDatabase.TIME_SERIES_POINTS_TABLE_NAME,
                schema=basis_data_schemas.time_series_point_schema,
            ),
            Table(
                name=paths.WholesaleBasisDataDatabase.CHARGE_LINK_PERIODS_TABLE_NAME,
                schema=basis_data_schemas.charge_link_periods_schema,
            ),
            Table(
                name=paths.WholesaleBasisDataDatabase.CHARGE_PRICE_INFORMATION_PERIODS_TABLE_NAME,
                schema=basis_data_schemas.charge_price_information_periods_schema,
            ),
            Table(
                name=paths.WholesaleBasisDataDatabase.CHARGE_PRICE_POINTS_TABLE_NAME,
                schema=basis_data_schemas.charge_price_points_schema,
            ),
            Table(
                name=paths.WholesaleBasisDataDatabase.CALCULATIONS_TABLE_NAME,
                schema=basis_data_schemas.calculations_schema,
            ),
        ],
        views=[],
    ),
]
