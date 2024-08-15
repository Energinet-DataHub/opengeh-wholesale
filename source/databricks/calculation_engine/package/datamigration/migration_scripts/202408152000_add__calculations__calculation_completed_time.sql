ALTER TABLE {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations
    ADD COLUMN calculation_completed_time TIMESTAMP
GO

UPDATE {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations
SET calculation_completed_time = calculation_execution_time_start
WHERE calculation_completed_time IS NULL