from spark_sql_migrations import Schema

import package.infrastructure.paths as paths

schema_config = [
    Schema(
        name=paths.WholesaleInternalDatabase.DATABASE_NAME,
        tables=[],
        views=[],
    ),
    Schema(
        name=paths.WholesaleResultsInternalDatabase.DATABASE_NAME,
        tables=[],
        views=[],
    ),
    Schema(
        name=paths.WholesaleResultsDatabase.DATABASE_NAME,
        tables=[],
        views=[],
    ),
    Schema(
        name=paths.WholesaleBasisDataInternalDatabase.DATABASE_NAME,
        tables=[],
        views=[],
    ),
    Schema(
        name=paths.WholesaleSettlementReportsDatabase.DATABASE_NAME,
        tables=[],
        views=[],
    ),
]
