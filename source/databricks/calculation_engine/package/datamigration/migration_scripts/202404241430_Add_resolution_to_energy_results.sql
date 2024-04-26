ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results SET TBLPROPERTIES (
    'delta.columnMapping.mode' = 'name',
    'delta.minReaderVersion' = '2',
    'delta.minWriterVersion' = '5')
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
ADD COLUMN resolution STRING
GO

UPDATE {OUTPUT_DATABASE_NAME}.energy_results
SET resolution = 'PT15M' WHERE resolution IS NULL
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    DROP CONSTRAINT IF EXISTS resolution_chk
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    ADD CONSTRAINT resolution_chk CHECK (resolution IN ('PT15M', 'PT1H'))
GO
