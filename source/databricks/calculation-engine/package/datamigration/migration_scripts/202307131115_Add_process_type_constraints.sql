ALTER TABLE {OUTPUT_DATABASE_NAME}.{ENERGY_RESULT_TABLE_NAME}
    DROP CONSTRAINT IF EXISTS batch_process_type_chk
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.{ENERGY_RESULT_TABLE_NAME}
    ADD CONSTRAINT batch_process_type_chk CHECK (batch_process_type IN ('BalanceFixing', 'Aggregation', 'WholesaleFixing', 'FirstCorrectionSettlement', 'SecondCorrectionSettlement', 'ThirdCorrectionSettlement'))