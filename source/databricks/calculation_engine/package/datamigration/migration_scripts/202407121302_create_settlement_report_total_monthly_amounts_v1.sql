CREATE VIEW IF NOT EXISTS {CATALOG_NAME}.{WHOLESALE_RESULTS_DATABASE_NAME}.total_monthly_amounts_v1 as
SELECT calculation_id,
       calculation_type,
       calculation_version,
       result_id,
       grid_area_code,
       energy_supplier_id,
       time,
       "P1M" as resolution,
       currency,
       amount,
       charge_owner_id,
FROM {CATALOG_NAME}.{WHOLESALE_RESULTS_DATABASE_NAME}.total_monthly_amounts_v1
