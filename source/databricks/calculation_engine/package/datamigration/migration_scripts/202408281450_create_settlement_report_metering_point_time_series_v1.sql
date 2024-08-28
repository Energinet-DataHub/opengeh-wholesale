DROP VIEW IF EXISTS {CATALOG_NAME}.{WHOLESALE_SETTLEMENT_REPORTS_DATABASE_NAME}.metering_point_time_series_v1
GO

CREATE VIEW {CATALOG_NAME}.{WHOLESALE_SETTLEMENT_REPORTS_DATABASE_NAME}.metering_point_time_series_v1 AS
SELECT calculation_id,
       calculation_type,
       calculation_version,
       metering_point_id,
       metering_point_type,
       resolution,
       grid_area_code,
       energy_supplier_id,
       -- There is a row for each 'observation_time' with an associated 'quantity' although the final settlement report
       -- has a row per day with multiple quantity columns for that day. It turns out to that performance increases when
       -- rows to columns are done on the consumer's side outside this view.
       observation_time,
       quantity
FROM {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.time_series_points
WHERE calculation_type IN ('balance_fixing', 'wholesale_fixing', 'first_correction_settlement', 'second_correction_settlement', 'third_correction_settlement')
  AND is_internal_calculation = FALSE
