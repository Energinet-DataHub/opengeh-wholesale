ALTER TABLE {BASIS_DATA_DATABASE_NAME}.calculations SET TBLPROPERTIES (
    'delta.columnMapping.mode' = 'name',
    'delta.minReaderVersion' = '2',
    'delta.minWriterVersion' = '5')
GO
ALTER TABLE {BASIS_DATA_DATABASE_NAME}.calculations
    DROP COLUMN created_time
GO
ALTER TABLE {BASIS_DATA_DATABASE_NAME}.calculations
    DROP CONSTRAINT IF EXISTS created_by_user_id_chk
GO
ALTER TABLE {BASIS_DATA_DATABASE_NAME}.calculations
    ADD CONSTRAINT created_by_user_id_chk CHECK (LENGTH(created_by_user_id) = 36)
GO
