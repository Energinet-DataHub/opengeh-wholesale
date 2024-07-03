DROP VIEW IF EXISTS {SETTLEMENT_REPORT_DATABASE_NAME}.current_balance_fixing_calculation_version_v1
GO

CREATE VIEW IF NOT EXISTS {SETTLEMENT_REPORT_DATABASE_NAME}.current_balance_fixing_calculation_version_v1 as
SELECT MAX(version) as calculation_version
FROM {BASIS_DATA_DATABASE_NAME}.calculations
WHERE calculation_type = 'balance_fixing'
GO

DROP VIEW IF EXISTS {SETTLEMENT_REPORT_DATABASE_NAME}.energy_result_points_per_ga_v1
GO

CREATE VIEW {SETTLEMENT_REPORT_DATABASE_NAME}.energy_result_points_per_ga_v1 as
SELECT calculation_id,
       calculation_type,
       calculation_version,
       result_id,
       grid_area_code,
       metering_point_type,
       settlement_method,
       resolution,
       time,
       quantity
FROM {HIVE_OUTPUT_DATABASE_NAME}.succeeded_energy_results_v1
WHERE time_series_type IN ('production', 'non_profiled_consumption', 'flex_consumption', 'net_exchange_per_ga', 'total_consumption')
AND calculation_type IN ('balance_fixing', 'wholesale_fixing', 'first_correction_settlement', 'second_correction_settlement', 'third_correction_settlement')
AND aggregation_level = 'grid_area'
GO

DROP VIEW IF EXISTS {SETTLEMENT_REPORT_DATABASE_NAME}.energy_result_points_per_es_ga_v1
GO

CREATE VIEW {SETTLEMENT_REPORT_DATABASE_NAME}.energy_result_points_per_es_ga_v1 as
SELECT calculation_id,
       calculation_type,
       calculation_version,
       result_id,
       grid_area_code,
       metering_point_type,
       settlement_method,
       resolution,
       time,
       quantity,
       energy_supplier_id
FROM {HIVE_OUTPUT_DATABASE_NAME}.succeeded_energy_results_v1
WHERE time_series_type IN ('production', 'non_profiled_consumption', 'flex_consumption')
AND calculation_type IN ('balance_fixing', 'wholesale_fixing', 'first_correction_settlement', 'second_correction_settlement', 'third_correction_settlement')
AND aggregation_level = 'energy_supplier'
GO

DROP VIEW IF EXISTS {CALCULATION_RESULTS_DATABASE_NAME}.energy_per_brp_ga_v1
GO

CREATE VIEW IF NOT EXISTS {CALCULATION_RESULTS_DATABASE_NAME}.energy_per_brp_ga_v1 AS
SELECT calculation_id,
       calculation_type,
       calculation_period_start,
       calculation_period_end,
       calculation_version,
       result_id,
       grid_area_code,
       balance_responsible_party_id,
       metering_point_type,
       settlement_method,
       resolution,
       time,
       quantity,
       unit as quantity_unit,
       quantity_qualities
FROM {HIVE_OUTPUT_DATABASE_NAME}.succeeded_energy_results_v1
WHERE
    calculation_type in ('balance_fixing', 'aggregation')
    AND aggregation_level = 'balance_responsible_party'
    AND time_series_type in ('production', 'non_profiled_consumption', 'flex_consumption')
GO

DROP VIEW IF EXISTS {CALCULATION_RESULTS_DATABASE_NAME}.energy_per_es_brp_ga_v1
GO

CREATE VIEW IF NOT EXISTS {CALCULATION_RESULTS_DATABASE_NAME}.energy_per_es_brp_ga_v1 AS
SELECT calculation_id,
       calculation_type,
       calculation_period_start,
       calculation_period_end,
       calculation_version,
       result_id,
       grid_area_code,
       energy_supplier_id,
       balance_responsible_party_id,
       metering_point_type,
       settlement_method,
       resolution,
       time,
       quantity,
       unit as quantity_unit,
       quantity_qualities
FROM {HIVE_OUTPUT_DATABASE_NAME}.succeeded_energy_results_v1
WHERE
    aggregation_level = 'energy_supplier'
    AND time_series_type in ('production', 'non_profiled_consumption', 'flex_consumption')
GO

DROP VIEW IF EXISTS {SETTLEMENT_REPORT_DATABASE_NAME}.metering_point_periods_v1
GO

CREATE VIEW IF NOT EXISTS {SETTLEMENT_REPORT_DATABASE_NAME}.metering_point_periods_v1 AS
SELECT c.calculation_id,
       c.calculation_type,
       c.version as calculation_version,
       m.metering_point_id,
       m.from_date,
       m.to_date,
       m.grid_area_code,
       m.from_grid_area_code,
       m.to_grid_area_code,
       m.metering_point_type,
       m.settlement_method,
       m.energy_supplier_id
FROM {BASIS_DATA_DATABASE_NAME}.metering_point_periods as m
INNER JOIN {BASIS_DATA_DATABASE_NAME}.calculations AS c ON c.calculation_id = m.calculation_id
WHERE c.calculation_type IN ('balance_fixing', 'wholesale_fixing', 'first_correction_settlement', 'second_correction_settlement', 'third_correction_settlement')
GO

DROP VIEW IF EXISTS {SETTLEMENT_REPORT_DATABASE_NAME}.metering_point_time_series_v1
GO

CREATE VIEW IF NOT EXISTS {SETTLEMENT_REPORT_DATABASE_NAME}.metering_point_time_series_v1 AS
SELECT c.calculation_id,
       FIRST(c.calculation_type) as calculation_type,
       FIRST(c.version) as calculation_version,
       m.metering_point_id,
       m.metering_point_type,
       m.resolution,
       m.grid_area_code,
       m.energy_supplier_id,
       TO_UTC_TIMESTAMP(DATE_TRUNC('day', FROM_UTC_TIMESTAMP(t.observation_time, 'Europe/Copenhagen')),'Europe/Copenhagen') AS start_date_time,
       ARRAY_SORT(ARRAY_AGG(struct(t.observation_time, t.quantity)))                  AS quantities
FROM {BASIS_DATA_DATABASE_NAME}.metering_point_periods AS m
  INNER JOIN {BASIS_DATA_DATABASE_NAME}.calculations AS c ON c.calculation_id = m.calculation_id
  INNER JOIN {BASIS_DATA_DATABASE_NAME}.time_series_points AS t ON m.metering_point_id = t.metering_point_id AND m.calculation_id = t.calculation_id
WHERE c.calculation_type IN ('balance_fixing', 'wholesale_fixing', 'first_correction_settlement', 'second_correction_settlement', 'third_correction_settlement')
  AND t.observation_time >= m.from_date
  AND (m.to_date IS NULL OR t.observation_time < m.to_date)
GROUP BY
    c.calculation_id, m.metering_point_id, m.metering_point_type, DATE_TRUNC('day', FROM_UTC_TIMESTAMP(t.observation_time, 'Europe/Copenhagen')), m.resolution, m.grid_area_code, m.energy_supplier_id
