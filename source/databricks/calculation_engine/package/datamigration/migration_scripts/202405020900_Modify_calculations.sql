ALTER TABLE {BASIS_DATA_DATABASE_NAME}.calculations SET TBLPROPERTIES (
    'delta.columnMapping.mode' = 'name',
    'delta.minReaderVersion' = '2',
    'delta.minWriterVersion' = '5')
GO

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.calculations
    DROP COLUMN created_time
GO
