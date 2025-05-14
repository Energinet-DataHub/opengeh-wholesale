ALTER TABLE {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.total_monthly_amounts
CLUSTER BY (calculation_id, grid_area_code, energy_supplier_id)