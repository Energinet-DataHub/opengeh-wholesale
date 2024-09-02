DROP VIEW IF EXISTS {CATALOG_NAME}.{WHOLESALE_SETTLEMENT_REPORTS_DATABASE_NAME}.metering_point_time_series_v1
GO
CREATE VIEW {CATALOG_NAME}.{WHOLESALE_SETTLEMENT_REPORTS_DATABASE_NAME}.metering_point_time_series_v1 AS
SELECT c.calculation_id,
       c.calculation_type as calculation_type,
       c.calculation_version,
       m.metering_point_id,
       m.metering_point_type,
       m.resolution,
       m.grid_area_code,
       m.energy_supplier_id,
       -- There is a row for each 'observation_time' with an associated 'quantity' although the final settlement report
       -- has a row per day with multiple quantity columns for that day. It turns out to that performance increases when
       -- rows to columns are done on the consumer's side outside this view.
       t.observation_time,
       t.quantity
FROM {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.metering_point_periods AS m
  INNER JOIN {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.succeeded_external_calculations_v1 AS c ON c.calculation_id = m.calculation_id
  INNER JOIN {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.temp_time_series_points AS t ON m.metering_point_id = t.metering_point_id AND m.calculation_id = t.calculation_id
WHERE c.calculation_type IN ('balance_fixing', 'wholesale_fixing', 'first_correction_settlement', 'second_correction_settlement', 'third_correction_settlement')
  AND t.observation_time >= m.from_date
  AND (m.to_date IS NULL OR t.observation_time < m.to_date)
