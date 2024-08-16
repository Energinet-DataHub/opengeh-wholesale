ALTER TABLE {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations
    ADD COLUMN calculation_completed_time TIMESTAMP
GO

-- The completion time is not known for the existing calculations, so we will set it to the start time
UPDATE {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations
SET calculation_completed_time = calculation_execution_time_start
WHERE calculation_completed_time IS NULL