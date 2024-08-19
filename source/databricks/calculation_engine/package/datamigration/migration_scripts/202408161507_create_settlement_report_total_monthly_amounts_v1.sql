DROP VIEW IF EXISTS {CATALOG_NAME}.{WHOLESALE_SETTLEMENT_REPORTS_DATABASE_NAME}.total_monthly_amounts_v1
GO

CREATE VIEW {CATALOG_NAME}.{WHOLESALE_SETTLEMENT_REPORTS_DATABASE_NAME}.total_monthly_amounts_v1 as
SELECT c.calculation_id,
       c.calculation_type,
       c.calculation_version,
       result_id,
       grid_area_code,
       energy_supplier_id,
       time,
       amount,
       charge_owner_id
FROM {CATALOG_NAME}.{WHOLESALE_RESULTS_DATABASE_NAME}.total_monthly_amounts_v1 AS tma
INNER JOIN {CATALOG_NAME}.{WHOLESALE_RESULTS_DATABASE_NAME}.calculations_v1 AS c ON c.calculation_id = tma.calculation_id
WHERE c.is_internal_calculation = FALSE
