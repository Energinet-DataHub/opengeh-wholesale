DROP VIEW IF EXISTS {SETTLEMENT_REPORT_DATABASE_NAME}.metering_point_time_series_v1
GO

CREATE VIEW IF NOT EXISTS {SETTLEMENT_REPORT_DATABASE_NAME}.metering_point_time_series_v1 AS
SELECT c.calculation_id,
       c.calculation_type as calculation_type,
       c.version as calculation_version,
       m.metering_point_id,
       m.metering_point_type,
       m.resolution,
       m.grid_area_code,
       m.energy_supplier_id,
       -- There is a row for each 'observation_time' with an associated 'quantity' although the final settlement report
       -- has a row per day with multiple quantity columns for that day. It turns out that performance increases when
       -- the rows-to-columns transformation is done outside this view on the consumer's side .
       t.observation_time,
       t.quantity
FROM {HIVE_BASIS_DATA_DATABASE_NAME}.metering_point_periods AS m
  INNER JOIN {HIVE_BASIS_DATA_DATABASE_NAME}.calculations AS c ON c.calculation_id = m.calculation_id
  INNER JOIN {HIVE_BASIS_DATA_DATABASE_NAME}.time_series_points AS t ON m.metering_point_id = t.metering_point_id AND m.calculation_id = t.calculation_id
WHERE c.calculation_type IN ('balance_fixing', 'wholesale_fixing', 'first_correction_settlement', 'second_correction_settlement', 'third_correction_settlement')
  AND t.observation_time >= m.from_date
  AND (m.to_date IS NULL OR t.observation_time < m.to_date)
GO
