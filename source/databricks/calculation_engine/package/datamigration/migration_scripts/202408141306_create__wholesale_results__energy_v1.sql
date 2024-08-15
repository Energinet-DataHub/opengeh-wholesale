DROP VIEW IF EXISTS {CATALOG_NAME}.{WHOLESALE_RESULTS_DATABASE_NAME}.energy_v1
GO

CREATE VIEW {CATALOG_NAME}.{WHOLESALE_RESULTS_DATABASE_NAME}.energy_v1 AS
SELECT c.calculation_id,
       result_id,
       grid_area_code,
       CASE
           WHEN time_series_type = 'production' THEN 'production'
           WHEN time_series_type = 'non_profiled_consumption' THEN 'consumption'
           WHEN time_series_type = 'flex_consumption' THEN 'consumption'
           WHEN time_series_type = 'net_exchange_per_ga' THEN 'exchange'
           WHEN time_series_type = 'total_consumption' THEN 'consumption'
       END as metering_point_type,
       CASE
           WHEN time_series_type = 'production' THEN NULL
           WHEN time_series_type = 'non_profiled_consumption' THEN 'non_profiled'
           WHEN time_series_type = 'flex_consumption' THEN 'flex'
           WHEN time_series_type = 'net_exchange_per_ga' THEN NULL
           WHEN time_series_type = 'total_consumption' THEN NULL
       END as settlement_method,
       resolution,
       time,
       quantity,
       'kWh' as quantity_unit,
       quantity_qualities
FROM {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.energy AS e
INNER JOIN {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations AS c ON c.calculation_id = e.calculation_id
WHERE
    -- Only include results that must be sent to the actors
    time_series_type in ('production', 'non_profiled_consumption', 'net_exchange_per_ga', 'flex_consumption', 'total_consumption')
