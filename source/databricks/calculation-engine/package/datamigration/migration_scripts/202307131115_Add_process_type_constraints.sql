ALTER TABLE wholesale_output.result
    DROP CONSTRAINT IF EXISTS batch_process_type_chk
GO

ALTER TABLE wholesale_output.result
    ADD CONSTRAINT batch_process_type_chk CHECK (batch_process_type IN ('BalanceFixing', 'Aggregation', 'WholesaleFixing', 'FirstCorrectionSettlement', 'SecondCorrectionSettlement', 'ThirdCorrectionSettlement'))
