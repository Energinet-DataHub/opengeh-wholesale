CREATE VIEW IF NOT EXISTS {HIVE_SETTLEMENT_REPORT_DATABASE_NAME}.metering_point_time_series as
SELECT m.calculation_id,
       m.metering_point_id,
       m.metering_point_type,
       m.resolution,
       m.grid_area_code,
       m.energy_supplier_id,
       DATE_TRUNC('day', FROM_UTC_TIMESTAMP(t.observation_time, 'Europe/Copenhagen')) AS observation_day,
       ARRAY_SORT(ARRAY_AGG(struct(t.observation_time, t.quantity)))                  AS quantities
FROM {HIVE_BASIS_DATA_DATABASE_NAME}.metering_point_periods AS m
         JOIN (SELECT * FROM {HIVE_BASIS_DATA_DATABASE_NAME}.time_series_points order by observation_time) AS t ON m.metering_point_id = t.metering_point_id AND m
             .calculation_id = t.calculation_id
WHERE t.observation_time >= m.from_date
  AND t.observation_time < m.to_date
GROUP BY m.calculation_id, m.metering_point_id, m.metering_point_type, observation_day, m.resolution, m.grid_area_code, m.energy_supplier_id
ORDER BY observation_day