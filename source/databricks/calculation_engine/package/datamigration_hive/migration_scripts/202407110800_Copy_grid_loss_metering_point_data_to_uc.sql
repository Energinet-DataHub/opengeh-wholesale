INSERT INTO {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.grid_loss_metering_points
SELECT * FROM {INPUT_DATABASE_NAME}.grid_loss_metering_points
WHERE NOT EXISTS (
    SELECT 1
    FROM {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.grid_loss_metering_points
    WHERE {INPUT_DATABASE_NAME}.grid_loss_metering_points.metering_point_id = {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.grid_loss_metering_points.metering_point_id
)
