ALTER TABLE {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.exchange_per_neighbor_ga
CLUSTER BY (calculation_id, grid_area_code, time)