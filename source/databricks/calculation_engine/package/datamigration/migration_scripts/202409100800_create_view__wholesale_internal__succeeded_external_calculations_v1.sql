DROP VIEW IF EXISTS {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.succeeded_external_calculations_v1
GO

-- Create view exposing succeeded external calculations
CREATE VIEW {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.succeeded_external_calculations_v1 AS
SELECT calculation_id,
       calculation_type,
       calculation_period_start,
       calculation_period_end,
       calculation_version,
       is_internal_calculation
FROM {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations_v1
WHERE is_internal_calculation = false AND calculation_succeeded_time IS NOT NULL
