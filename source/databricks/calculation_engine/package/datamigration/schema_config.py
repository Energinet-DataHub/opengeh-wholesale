from spark_sql_migrations import (
    Schema,
    Table
)
from package.infrastructure.paths import (
    WHOLESALE_RESULT_TABLE_NAME,
    OUTPUT_DATABASE_NAME,
    ENERGY_RESULT_TABLE_NAME,
)

# calculation_output
from package.calculation_output.schemas.wholesale_results_schema import wholesale_results_schema
from package.calculation_output.schemas.energy_results_schema import energy_results_schema

schema_config = [
    Schema(
        name=OUTPUT_DATABASE_NAME,
        tables=[
            Table(
                name=WHOLESALE_RESULT_TABLE_NAME,
                schema=wholesale_results_schema,
            ),
            Table(
                name=ENERGY_RESULT_TABLE_NAME,
                schema=energy_results_schema,
            )
        ]
    )
]