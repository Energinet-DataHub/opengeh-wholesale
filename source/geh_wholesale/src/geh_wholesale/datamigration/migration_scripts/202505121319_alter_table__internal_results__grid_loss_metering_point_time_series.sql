ALTER TABLE {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.grid_loss_metering_points
CLUSTER BY (calculation_id)
GO

ALTER TABLE {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.grid_loss_metering_point_time_series
CLUSTER BY (calculation_id, grid_area_code, time)