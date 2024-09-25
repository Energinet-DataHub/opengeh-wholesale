ALTER TABLE {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.time_series_points
CLUSTER BY (calculation_id, grid_area_code, energy_supplier_id, observation_time);
