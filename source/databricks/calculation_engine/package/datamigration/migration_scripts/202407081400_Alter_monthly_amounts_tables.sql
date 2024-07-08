ALTER TABLE {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.monthly_amounts_per_charge
    DROP COLUMN calculation_type,
    DROP COLUMN calculation_execution_time_start,
    RENAME COLUMN calculation_result_id TO result_id,
GO

ALTER TABLE {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.total_monthly_amounts
    DROP COLUMN calculation_type,
    DROP COLUMN calculation_execution_time_start,
    RENAME COLUMN calculation_result_id TO result_id,
GO


