ALTER TABLE {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.monthly_amounts_per_charge
CLUSTER BY (calculation_id, grid_area_code, energy_supplier_id)