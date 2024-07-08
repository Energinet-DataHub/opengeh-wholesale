from spark_sql_migrations import Schema, Table

import package.infrastructure.paths as paths

from package.calculation.output.results.schemas import (
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
        ],
        views=[],
    ),
]
