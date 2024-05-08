DROP VIEW IF EXISTS {SETTLEMENT_REPORT_DATABASE_NAME}.metering_point_time_series_v1
GO

CREATE VIEW IF NOT EXISTS {SETTLEMENT_REPORT_DATABASE_NAME}.metering_point_time_series_v1 as
SELECT m.calculation_id,
       m.metering_point_id,
       m.metering_point_type,
       m.resolution,
       m.grid_area_code,
       m.energy_supplier_id,
       DATE_TRUNC('day', FROM_UTC_TIMESTAMP(t.observation_time, 'Europe/Copenhagen')) AS observation_day,
       ARRAY_SORT(ARRAY_AGG(struct(t.observation_time, t.quantity)))                  AS quantities
FROM {BASIS_DATA_DATABASE_NAME}.metering_point_periods AS m
         INNER JOIN {BASIS_DATA_DATABASE_NAME}.calculations AS c ON c.calculation_id = m.calculation_id
         LEFT JOIN {BASIS_DATA_DATABASE_NAME}.time_series_points AS t ON m.metering_point_id = t.metering_point_id AND m.calculation_id = t
             .calculation_id
WHERE t.observation_time >= m.from_date
  AND (m.to_date IS NULL OR t.observation_time < m.to_date)
  AND m.metering_point_type != 'exchange'
GROUP BY m.calculation_id, m.metering_point_id, m.metering_point_type, observation_day, m.resolution, m.grid_area_code, m.energy_supplier_id
ORDER BY observation_day
