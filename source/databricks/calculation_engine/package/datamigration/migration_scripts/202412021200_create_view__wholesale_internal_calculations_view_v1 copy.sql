-- This internal calculations view combines the DH3 calculations from our regular internal database, 
-- with the migrated DH2 calculations from the input database.
CREATE OR REPLACE TABLE {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations_view_v1 AS (
    SELECT 
        calculation_id, 
        calculation_type,
        calculation_period_start,
        calculation_period_end,
        0 AS calculation_version,
        calculation_execution_time_start,
        calculation_succeeded_time,
        FALSE AS is_internal_calculation
    FROM {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations_from_dh2
    UNION
    SELECT 
        calculation_id, 
        calculation_type,
        calculation_period_start,
        calculation_period_end,
        calculation_version,
        calculation_execution_time_start,
        calculation_succeeded_time,
        is_internal_calculation
    FROM {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations
)
