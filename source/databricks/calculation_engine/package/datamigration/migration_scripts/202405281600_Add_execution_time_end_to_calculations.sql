ALTER TABLE {BASIS_DATA_DATABASE_NAME}.calculations
ADD COLUMN execution_time_end timestamp AFTER execution_time_start
GO

UPDATE {BASIS_DATA_DATABASE_NAME}.calculations
SET execution_time_end = execution_time_start
GO

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.calculations ALTER COLUMN execution_time_end SET NOT NULL;
