CREATE VIEW IF NOT EXISTS {CATALOG_NAME}.{WHOLESALE_SETTLEMENT_REPORTS_DATABASE_NAME}.energy_v1 as
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
FROM {CATALOG_NAME}.{WHOLESALE_RESULTS_DATABASE_NAME}.energy_v1
WHERE calculation_type IN ('balance_fixing', 'wholesale_fixing', 'first_correction_settlement', 'second_correction_settlement', 'third_correction_settlement')