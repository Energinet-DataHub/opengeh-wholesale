ALTER TABLE {HIVE_BASIS_DATA_DATABASE_NAME}.calculations
    ADD COLUMN calculation_completed_time TIMESTAMP
GO

UPDATE {HIVE_BASIS_DATA_DATABASE_NAME}.calculations
SET calculation_completed_time = execution_time_start
WHERE calculation_completed_time IS NULL