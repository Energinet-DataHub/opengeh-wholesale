CREATE VIEW IF NOT EXISTS {CATALOG_NAME}.{WHOLESALE_SETTLEMENT_REPORTS_DATABASE_NAME}.monthly_amounts_per_charge_v1 as
SELECT calculation_id,
       calculation_type,
       calculation_version,
       result_id,
       grid_area_code,
       energy_supplier_id,
       time,
       quantity_unit,
       amount,
       charge_type,
       charge_code,
       charge_owner_id,
       is_tax
FROM {CATALOG_NAME}.{WHOLESALE_RESULTS_DATABASE_NAME}.monthly_amounts_per_charge_v1