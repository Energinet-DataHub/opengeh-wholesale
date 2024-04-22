CREATE VIEW {SETTLEMENT_REPORT_DATABASE_NAME}.energy_results_v1 as
SELECT m.calculation_id,
       m.calculation_type,
       m.grid_area_code,
       m.metering_point_type,
       m.resolution,
       m.time
       m.quantity,
       m.energy_supplier_id,
FROM {OUTPUT_DATABASE_NAME}.energy_results AS r
         JOIN (SELECT * FROM {BASIS_DATA_DATABASE_NAME}.time_series_points order by observation_time) AS t ON m.metering_point_id = t.metering_point_id AND m
             .calculation_id = t.calculation_id
WHERE t.observation_time >= m.from_date
  AND t.observation_time < m.to_date
GROUP BY m.calculation_id, m.metering_point_id, m.metering_point_type, observation_day, m.resolution, m.grid_area_code, m.energy_supplier_id
ORDER BY observation_day