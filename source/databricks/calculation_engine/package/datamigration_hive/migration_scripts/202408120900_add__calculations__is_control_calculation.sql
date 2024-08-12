ALTER TABLE {HIVE_BASIS_DATA_DATABASE_NAME}.calculations SET TBLPROPERTIES (
    'delta.columnMapping.mode' = 'name',
    'delta.minReaderVersion' = '2',
    'delta.minWriterVersion' = '5')
GO

ALTER TABLE {HIVE_BASIS_DATA_DATABASE_NAME}.calculations
    ADD COLUMN is_control_calculation BOOLEAN
GO

UPDATE {HIVE_BASIS_DATA_DATABASE_NAME}.calculations
    SET is_control_calculation = FALSE
GO

ALTER TABLE {HIVE_BASIS_DATA_DATABASE_NAME}.calculations
    ADD CONSTRAINT is_control_calculation_chk
    CHECK (is_control_calculation IS NOT NULL)
