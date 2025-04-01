DROP VIEW IF EXISTS {CATALOG_NAME}.{WHOLESALE_RESULTS_DATABASE_NAME}.energy_per_brp_v1
GO

CREATE VIEW IF NOT EXISTS {CATALOG_NAME}.{WHOLESALE_RESULTS_DATABASE_NAME}.energy_per_brp_v1 AS
SELECT c.calculation_id,
       calculation_type,
       calculation_period_start,
       calculation_period_end,
       calculation_version,
       result_id,
       grid_area_code,
       balance_responsible_party_id,
       CASE
           WHEN e.time_series_type = 'production' THEN 'production'
           WHEN e.time_series_type = 'non_profiled_consumption' THEN 'consumption'
           WHEN e.time_series_type = 'flex_consumption' THEN 'consumption'
       END as metering_point_type,
       CASE
           WHEN e.time_series_type = 'production' THEN NULL
           WHEN e.time_series_type = 'non_profiled_consumption' THEN 'non_profiled'
           WHEN e.time_series_type = 'flex_consumption' THEN 'flex'
       END as settlement_method,
       resolution,
       time,
       quantity,
       'kWh' as quantity_unit,
       quantity_qualities
FROM {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.energy_per_brp AS e
INNER JOIN {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.succeeded_external_calculations_v1 AS c ON c.calculation_id = e.calculation_id
WHERE
    -- Only include results that must be sent to the balance responsible parties
    calculation_type in ('balance_fixing', 'aggregation')
    AND time_series_type in ('production', 'non_profiled_consumption', 'flex_consumption')
