from spark_sql_migrations import Schema, Table

import package.infrastructure.paths as paths

from package.calculation.output.schemas.energy_results_schema import (
    energy_results_schema,
)

schema_config = [
    Schema(
        name=paths.OutputDatabase.DATABASE_NAME,
        tables=[
            Table(
                name=paths.OutputDatabase.ENERGY_RESULT_TABLE_NAME,
                schema=energy_results_schema,
            ),
        ],
        views=[],
    ),
]
