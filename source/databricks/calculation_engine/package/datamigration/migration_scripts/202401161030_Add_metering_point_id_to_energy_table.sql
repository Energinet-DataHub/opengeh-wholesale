ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    ADD COLUMN metering_point_id STRING NULL;
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    DROP CONSTRAINT IF EXISTS metering_point_id_chk
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    ADD CONSTRAINT metering_point_id_chk CHECK (LENGTH(metering_point_id) = 18)
GO
