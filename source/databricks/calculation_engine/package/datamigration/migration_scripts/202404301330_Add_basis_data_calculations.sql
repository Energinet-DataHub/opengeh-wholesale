ALTER TABLE {BASIS_DATA_DATABASE_NAME}.calculations SET TBLPROPERTIES (
    'delta.columnMapping.mode' = 'name',
    'delta.minReaderVersion' = '2',
    'delta.minWriterVersion' = '5')

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.calculations
DROP COLUMN created_time
GO

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.calculations
    DROP CONSTRAINT IF EXISTS calculation_id_chk
GO

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.calculations
    ADD CONSTRAINT calculation_id_chk CHECK (LENGTH(calculation_id) = 36)
GO

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.calculations
    DROP CONSTRAINT IF EXISTS calculation_type_chk
GO

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.calculations
    ADD CONSTRAINT calculation_type_chk CHECK (calculation_type IN ('BalanceFixing', 'Aggregation', 'WholesaleFixing', 'FirstCorrectionSettlement', 'SecondCorrectionSettlement', 'ThirdCorrectionSettlement'))
GO

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.calculations
    DROP CONSTRAINT IF EXISTS created_by_user_id_chk
GO

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.calculations
    ADD CONSTRAINT created_by_user_id_chk CHECK (LENGTH(created_by_user_id) = 36)
GO
