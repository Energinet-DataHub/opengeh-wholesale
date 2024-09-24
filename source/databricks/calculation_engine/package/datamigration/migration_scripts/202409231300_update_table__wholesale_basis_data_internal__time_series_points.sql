MERGE INTO {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.time_series_points AS t
USING {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.metering_point_periods AS m
ON t.calculation_id = m.calculation_id
   AND t.metering_point_id = m.metering_point_id
   AND t.observation_time >= m.from_date
   AND (m.to_date IS NULL OR t.observation_time < m.to_date)
WHEN MATCHED AND t.metering_point_type IS NULL
              AND t.resolution IS NULL
              AND t.grid_area_code IS NULL
              AND t.energy_supplier_id IS NULL THEN
UPDATE SET
    t.metering_point_type = m.metering_point_type,
    t.resolution = m.resolution,
    t.grid_area_code = m.grid_area_code,
    t.energy_supplier_id = m.energy_supplier_id
