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
