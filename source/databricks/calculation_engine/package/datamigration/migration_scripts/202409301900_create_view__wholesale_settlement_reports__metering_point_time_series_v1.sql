CREATE OR REPLACE VIEW {CATALOG_NAME}.{WHOLESALE_SETTLEMENT_REPORTS_DATABASE_NAME}.metering_point_time_series_v1 AS
SELECT t.calculation_id,
       c.calculation_type as calculation_type,
       c.calculation_version,
       t.metering_point_id,
       t.metering_point_type,
       t.resolution,
       t.grid_area_code,
       t.energy_supplier_id,
       -- There is a row for each 'observation_time' with an associated 'quantity' although the final settlement report
       -- has a row per day with multiple quantity columns for that day. It turns out to that performance increases when
       -- rows to columns are done on the consumer's side outside this view.
       t.observation_time,
       t.quantity
FROM {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.time_series_points AS t
  INNER JOIN {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.succeeded_external_calculations_v1 AS c ON c.calculation_id = t.calculation_id
WHERE c.calculation_type IN ('balance_fixing', 'wholesale_fixing', 'first_correction_settlement', 'second_correction_settlement', 'third_correction_settlement')
AND t.metering_point_type IS NOT NULL -- TODO JVM: Remove this when prod data is fixed
