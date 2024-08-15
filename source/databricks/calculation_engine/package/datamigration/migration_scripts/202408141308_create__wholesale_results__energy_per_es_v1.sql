DROP VIEW IF EXISTS {CATALOG_NAME}.{WHOLESALE_RESULTS_DATABASE_NAME}.energy_per_es_v1
GO

CREATE VIEW {CATALOG_NAME}.{WHOLESALE_RESULTS_DATABASE_NAME}.energy_per_es_v1 AS
SELECT c.calculation_id,
       result_id,
       grid_area_code,
       energy_supplier_id,
       balance_responsible_party_id,
       CASE
           WHEN time_series_type = 'production' THEN 'production'
           WHEN time_series_type = 'non_profiled_consumption' THEN 'consumption'
           WHEN time_series_type = 'flex_consumption' THEN 'consumption'
       END as metering_point_type,
       CASE
           WHEN time_series_type = 'production' THEN NULL
           WHEN time_series_type = 'non_profiled_consumption' THEN 'non_profiled'
           WHEN time_series_type = 'flex_consumption' THEN 'flex'
       END as settlement_method,
       resolution,
       time,
       quantity,
       'kWh' as quantity_unit,
       quantity_qualities
FROM {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.energy_per_es AS e
INNER JOIN {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations AS c ON c.calculation_id = e.calculation_id
WHERE
    -- Only include results that must be sent to the balance responsible parties
    time_series_type in ('production', 'non_profiled_consumption', 'flex_consumption')
