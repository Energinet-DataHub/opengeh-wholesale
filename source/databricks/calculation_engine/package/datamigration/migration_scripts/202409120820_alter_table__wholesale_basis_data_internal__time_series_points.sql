ALTER TABLE {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.time_series_points
ADD COLUMNS (
    metering_point_type STRING,
    resolution STRING,
    grid_area_code STRING,
    energy_supplier_id STRING
);
