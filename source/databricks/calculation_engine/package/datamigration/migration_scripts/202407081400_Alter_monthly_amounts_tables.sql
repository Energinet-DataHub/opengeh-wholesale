ALTER TABLE {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.monthly_amounts_per_charge SET TBLPROPERTIES (
    'delta.columnMapping.mode' = 'name',
    'delta.minReaderVersion' = '2',
    'delta.minWriterVersion' = '5')
GO

ALTER TABLE {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.monthly_amounts_per_charge
    DROP COLUMNS (calculation_type, calculation_execution_time_start)
GO

ALTER TABLE {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.monthly_amounts_per_charge
    RENAME COLUMN calculation_result_id TO result_id
GO

ALTER TABLE {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.total_monthly_amounts SET TBLPROPERTIES (
    'delta.columnMapping.mode' = 'name',
    'delta.minReaderVersion' = '2',
    'delta.minWriterVersion' = '5')
GO

ALTER TABLE {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.total_monthly_amounts
    DROP COLUMNS (calculation_type, calculation_execution_time_start)
GO

ALTER TABLE {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.total_monthly_amounts
    RENAME COLUMN calculation_result_id TO result_id
GO

