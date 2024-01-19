UPDATE {OUTPUT_DATABASE_NAME}.energy_results
SET metering_point_id = '000000000000000000'
WHERE (time_series_type = 'negative_grid_loss' OR time_series_type = 'positive_grid_loss') AND metering_point_id IS NULL
GO
