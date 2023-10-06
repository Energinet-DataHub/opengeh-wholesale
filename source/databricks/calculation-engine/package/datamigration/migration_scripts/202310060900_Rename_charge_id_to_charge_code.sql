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
ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results RENAME COLUMN charge_code TO charge_code
