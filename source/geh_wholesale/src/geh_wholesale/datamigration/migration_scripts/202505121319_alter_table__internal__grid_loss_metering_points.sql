ALTER TABLE {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.grid_loss_metering_points
CLUSTER BY (calculation_id, grid_area_code, time)