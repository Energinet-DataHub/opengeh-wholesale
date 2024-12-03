-- This copy of dh2 calculations is done, such that Wholesale can re-write their input as they please, without affecting Wholesale. 
-- In theory this could be replaced by a view on the Volt input, but then we might accidentally lose data.
CREATE OR REPLACE TABLE {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations_from_dh2 AS (
    SELECT 
        calculation_id, 
        calculation_type,
        calculation_period_start,
        calculation_period_end,
        calculation_execution_time_start,
        calculation_succeeded_time, 
    FROM {CATALOG_NAME}.{SHARED_WHOLESALE_INPUT}.calculations_view_v1
)
