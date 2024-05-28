ALTER TABLE {BASIS_DATA_DATABASE_NAME}.calculations
ADD COLUMN execution_time_end timestamp NOT NULL DEFAULT execution_time_start AFTER execution_time_start;
