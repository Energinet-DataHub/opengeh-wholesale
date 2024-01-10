ALTER TABLE {INPUT_DATABASE_NAME}.grid_loss_metering_points
    DROP CONSTRAINT IF EXISTS metering_point_id_chk
GO
ALTER TABLE {INPUT_DATABASE_NAME}.grid_loss_metering_points
    ADD CONSTRAINT metering_point_id_chk CHECK (LENGTH(metering_point_id) = 18)
GO
