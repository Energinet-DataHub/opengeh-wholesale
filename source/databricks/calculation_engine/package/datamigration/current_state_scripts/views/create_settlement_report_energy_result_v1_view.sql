CREATE VIEW {SETTLEMENT_REPORT_DATABASE_NAME}.energy_results_v1 as
SELECT e.calculation_id,
       e.calculation_type,
       e.grid_area_code,
       CASE
           WHEN e.time_series_type = 'production' THEN 'production'
           WHEN e.time_series_type = 'non_profiled_consumption' THEN 'consumption'
           WHEN e.time_series_type = 'flex_consumption' THEN 'consumption'
           WHEN e.time_series_type = 'net_exchange_per_ga' THEN 'exchange'
           WHEN e.time_series_type = 'total_consumption' THEN 'consumption'
       END as metering_point_type,
       CASE
           WHEN e.time_series_type = 'production' THEN NULL
           WHEN e.time_series_type = 'non_profiled_consumption' THEN 'non_profiled'
           WHEN e.time_series_type = 'flex_consumption' THEN 'flex'
           WHEN e.time_series_type = 'net_exchange_per_ga' THEN NULL
           WHEN e.time_series_type = 'total_consumption' THEN NULL
       END as settlement_method,
       e.resolution,
       e.time,
       e.quantity,
       e.energy_supplier_id,
       e.aggregation_level
FROM {OUTPUT_DATABASE_NAME}.energy_results AS e
INNER JOIN (SELECT calculation_id FROM {BASIS_DATA_DATABASE_NAME}.calculations) AS c ON c.calculation_id = e.calculation_id
WHERE e.time_series_type IN ('production', 'non_profiled_consumption', 'flex_consumption', 'net_exchange_per_ga', 'total_consumption')