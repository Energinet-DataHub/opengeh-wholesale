ALTER TABLE {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.metering_point_periods
CLUSTER BY (calculation_id, grid_area_code, metering_point_id, energy_supplier_id);