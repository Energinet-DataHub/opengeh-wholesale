ALTER TABLE {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.energy_per_es
CLUSTER BY (calculation_id, grid_area_code, time)