ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results RENAME COLUMN batch_id TO calculation_id
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results RENAME COLUMN batch_process_type TO calculation_type
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results RENAME COLUMN batch_execution_time_start TO calculation_execution_time_start
GO
