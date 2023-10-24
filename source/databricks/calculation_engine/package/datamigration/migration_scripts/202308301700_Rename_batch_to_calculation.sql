--
-- Enable Databricks column mapping mode in order to rename columns
--
ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results SET TBLPROPERTIES (
   'delta.columnMapping.mode' = 'name',
   'delta.minReaderVersion' = '2',
   'delta.minWriterVersion' = '5'
)
GO

--
-- Rename batch_id to calculation_id
--
ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results RENAME COLUMN batch_id TO calculation_id
GO

--
-- Rename batch_process_type to calculation_type
--
ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    DROP CONSTRAINT IF EXISTS batch_process_type_chk
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results RENAME COLUMN batch_process_type TO calculation_type
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    ADD CONSTRAINT calculation_type_chk CHECK (calculation_type IN ('BalanceFixing', 'Aggregation', 'WholesaleFixing', 'FirstCorrectionSettlement', 'SecondCorrectionSettlement', 'ThirdCorrectionSettlement'))
GO

--
-- Rename batch_execution_time_start to calculation_execution_time_start
--
ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results RENAME COLUMN batch_execution_time_start TO calculation_execution_time_start
GO
