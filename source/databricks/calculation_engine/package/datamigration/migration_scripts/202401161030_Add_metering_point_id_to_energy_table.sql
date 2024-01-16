ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    ADD COLUMN metering_point_id STRING NULL;
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    DROP CONSTRAINT IF EXISTS metering_point_id_chk
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    ADD CONSTRAINT metering_point_id_chk CHECK (LENGTH(metering_point_id) = 18)
GO
--If time_series_type is 'negative_grid_loss' or 'positive_grid_loss', then metering_point_id must not be null.
--If time_series_type is neither 'negative_grid_loss' nor 'positive_grid_loss', then metering_point_id must be null.
If time_series_type is neither 'negative_grid_loss' nor 'positive_grid_loss', then metering_point_id must be null.
ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    ADD CONSTRAINT metering_point_id_conditional_chk
    CHECK (
        (time_series_type IN ('negative_grid_loss', 'positive_grid_loss') AND metering_point_id IS NOT NULL)
        OR
        (time_series_type NOT IN ('negative_grid_loss', 'positive_grid_loss') AND metering_point_id IS NULL)
    );
GO

