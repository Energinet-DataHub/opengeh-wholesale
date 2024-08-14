DROP VIEW IF EXISTS {CATALOG_NAME}.{WHOLESALE_SETTLEMENT_REPORTS_DATABASE_NAME}.total_monthly_amounts_v1
GO

CREATE VIEW {CATALOG_NAME}.{WHOLESALE_SETTLEMENT_REPORTS_DATABASE_NAME}.total_monthly_amounts_v1 as
SELECT calculation_id,
       result_id,
       grid_area_code,
       energy_supplier_id,
       time,
       amount,
       charge_owner_id
FROM {CATALOG_NAME}.{WHOLESALE_RESULTS_DATABASE_NAME}.total_monthly_amounts_v1
