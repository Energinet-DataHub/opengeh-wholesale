DELETE FROM {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.time_series_points
WHERE metering_point_type IS NULL
  AND resolution IS NULL
  AND grid_area_code IS NULL
  AND energy_supplier_id IS NULL;