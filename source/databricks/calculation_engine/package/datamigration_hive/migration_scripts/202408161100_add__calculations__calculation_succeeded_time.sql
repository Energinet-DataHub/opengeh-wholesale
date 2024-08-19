ALTER TABLE {HIVE_BASIS_DATA_DATABASE_NAME}.calculations
    ADD COLUMN calculation_succeeded_time TIMESTAMP
GO

UPDATE {HIVE_BASIS_DATA_DATABASE_NAME}.calculations
SET calculation_succeeded_time = execution_time_start
WHERE calculation_succeeded_time IS NULL