DROP VIEW IF EXISTS {HIVE_OUTPUT_DATABASE_NAME}.succeeded_energy_results_v1
GO

CREATE VIEW IF NOT EXISTS {HIVE_OUTPUT_DATABASE_NAME}.succeeded_energy_results_v1 as
SELECT c.calculation_id,
       c.calculation_type,
       c.period_start AS calculation_period_start,
       c.period_end AS calculation_period_end,
       c.execution_time_start AS calculation_execution_time_start,
       c.created_by_user_id AS calculation_created_by_user_id,
       c.version AS calculation_version,

       e.calculation_result_id AS result_id,
       e.grid_area_code,
       e.neighbor_grid_area_code,
       e.energy_supplier_id,
       e.balance_responsible_id AS balance_responsible_party_id,
       e.quantity,
       'kWh' AS unit,
       e.quantity_qualities,
       e.time,
       e.aggregation_level,
       e.resolution,
       e.time_series_type,
       CASE
           WHEN e.time_series_type = 'production' THEN 'production'
           WHEN e.time_series_type = 'non_profiled_consumption' THEN 'consumption'
           WHEN e.time_series_type = 'flex_consumption' THEN 'consumption'
           WHEN e.time_series_type = 'net_exchange_per_ga' THEN 'exchange'
           WHEN e.time_series_type = 'net_exchange_per_neighboring_ga' THEN 'exchange'
           WHEN e.time_series_type = 'total_consumption' THEN 'consumption'
           WHEN e.time_series_type = 'grid_loss' AND e.quantity >= 0 THEN 'consumption'
           WHEN e.time_series_type = 'grid_loss' AND e.quantity < 0 THEN 'production'
           WHEN e.time_series_type = 'negative_grid_loss' THEN 'production'
           WHEN e.time_series_type = 'positive_grid_loss' THEN 'consumption'
           WHEN e.time_series_type = 'temp_flex_consumption' THEN 'consumption'
           WHEN e.time_series_type = 'temp_production' THEN 'production'
       END as metering_point_type,
       CASE
           WHEN e.time_series_type = 'non_profiled_consumption' THEN 'non_profiled'
           WHEN e.time_series_type = 'flex_consumption' THEN 'flex'
           WHEN e.time_series_type = 'temp_flex_consumption' THEN 'flex'
       END as settlement_method,
       e.metering_point_id
FROM {HIVE_OUTPUT_DATABASE_NAME}.energy_results AS e
INNER JOIN {BASIS_DATA_DATABASE_NAME}.calculations AS c ON c.calculation_id = e.calculation_id
GO
