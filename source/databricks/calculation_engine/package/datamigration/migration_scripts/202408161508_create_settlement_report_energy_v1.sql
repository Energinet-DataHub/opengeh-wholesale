DROP VIEW IF EXISTS {CATALOG_NAME}.{WHOLESALE_SETTLEMENT_REPORTS_DATABASE_NAME}.energy_v1
GO

CREATE VIEW {CATALOG_NAME}.{WHOLESALE_SETTLEMENT_REPORTS_DATABASE_NAME}.energy_v1 as
SELECT c.calculation_id,
       c.calculation_type,
       c.calculation_version,
       result_id,
       grid_area_code,
       metering_point_type,
       settlement_method,
       resolution,
       time,
       quantity
FROM {CATALOG_NAME}.{WHOLESALE_RESULTS_DATABASE_NAME}.energy_v1 AS e
INNER JOIN {CATALOG_NAME}.{WHOLESALE_RESULTS_DATABASE_NAME}.calculations_v1 AS c ON c.calculation_id = e.calculation_id
WHERE calculation_type IN ('balance_fixing', 'wholesale_fixing', 'first_correction_settlement', 'second_correction_settlement', 'third_correction_settlement')
    AND c.is_internal_calculation = FALSE
