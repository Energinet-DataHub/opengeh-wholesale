DROP VIEW IF EXISTS {HIVE_SETTLEMENT_REPORT_DATABASE_NAME}.metering_point_time_series_v1
GO

CREATE VIEW IF NOT EXISTS {HIVE_SETTLEMENT_REPORT_DATABASE_NAME}.metering_point_time_series_v1 AS
SELECT c.calculation_id,
       c.calculation_type,
       m.metering_point_id,
       m.metering_point_type,
       m.resolution,
       m.grid_area_code,
       m.energy_supplier_id,
       TO_UTC_TIMESTAMP(DATE_TRUNC('day', FROM_UTC_TIMESTAMP(t.observation_time, 'Europe/Copenhagen')),'Europe/Copenhagen') AS start_date_time,
       ARRAY_SORT(ARRAY_AGG(struct(t.observation_time, t.quantity))) AS quantities
FROM {HIVE_BASIS_DATA_DATABASE_NAME}.metering_point_periods AS m
         INNER JOIN {HIVE_BASIS_DATA_DATABASE_NAME}.calculations AS c ON c.calculation_id = m.calculation_id
         INNER JOIN {HIVE_BASIS_DATA_DATABASE_NAME}.time_series_points AS t ON m.metering_point_id = t.metering_point_id AND m.calculation_id = t
             .calculation_id
WHERE t.observation_time >= m.from_date
  AND (m.to_date IS NULL OR t.observation_time < m.to_date)
GROUP BY c.calculation_id, c.calculation_type, m.metering_point_id, m.metering_point_type, DATE_TRUNC('day', FROM_UTC_TIMESTAMP(t.observation_time, 'Europe/Copenhagen')), m.resolution, m.grid_area_code, m.energy_supplier_id

