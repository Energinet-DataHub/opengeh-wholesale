ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    ADD COLUMN metering_point_id STRING
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    DROP CONSTRAINT IF EXISTS metering_point_id_chk
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    ADD CONSTRAINT metering_point_id_chk CHECK  (metering_point_id IS NULL OR LENGTH(metering_point_id) = 18)
GO
