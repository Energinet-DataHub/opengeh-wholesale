ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
DROP COLUMN calculation_execution_time_start
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
DROP COLUMN calculation_execution_time_start
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.total_monthly_amounts
DROP COLUMN calculation_execution_time_start
GO
