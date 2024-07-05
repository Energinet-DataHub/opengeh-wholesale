-- Modify calculation_type column in energy_results to snake_case
ALTER TABLE {HIVE_OUTPUT_DATABASE_NAME}.energy_results
    DROP CONSTRAINT IF EXISTS calculation_type_chk
GO

UPDATE {HIVE_OUTPUT_DATABASE_NAME}.energy_results
SET calculation_type =
    CASE
        WHEN calculation_type = 'BalanceFixing' THEN 'balance_fixing'
        WHEN calculation_type = 'Aggregation' THEN 'aggregation'
        WHEN calculation_type = 'WholesaleFixing' THEN 'wholesale_fixing'
        WHEN calculation_type = 'FirstCorrectionSettlement' THEN 'first_correction_settlement'
        WHEN calculation_type = 'SecondCorrectionSettlement' THEN 'second_correction_settlement'
        WHEN calculation_type = 'ThirdCorrectionSettlement' THEN 'third_correction_settlement'
        ELSE calculation_type
    END
GO

ALTER TABLE {HIVE_OUTPUT_DATABASE_NAME}.energy_results
    ADD CONSTRAINT calculation_type_chk CHECK (calculation_type IN ('balance_fixing', 'aggregation', 'wholesale_fixing', 'first_correction_settlement', 'second_correction_settlement', 'third_correction_settlement'))
GO

-- Modify calculation_type column in basis data calculations to snake_case
ALTER TABLE {HIVE_BASIS_DATA_DATABASE_NAME}.calculations
    DROP CONSTRAINT IF EXISTS calculation_type_chk
GO

UPDATE {HIVE_BASIS_DATA_DATABASE_NAME}.calculations
SET calculation_type =
    CASE
        WHEN calculation_type = 'BalanceFixing' THEN 'balance_fixing'
        WHEN calculation_type = 'Aggregation' THEN 'aggregation'
        WHEN calculation_type = 'WholesaleFixing' THEN 'wholesale_fixing'
        WHEN calculation_type = 'FirstCorrectionSettlement' THEN 'first_correction_settlement'
        WHEN calculation_type = 'SecondCorrectionSettlement' THEN 'second_correction_settlement'
        WHEN calculation_type = 'ThirdCorrectionSettlement' THEN 'third_correction_settlement'
        ELSE calculation_type
    END
GO

ALTER TABLE {HIVE_BASIS_DATA_DATABASE_NAME}.calculations
    ADD CONSTRAINT calculation_type_chk CHECK (calculation_type IN ('balance_fixing', 'aggregation', 'wholesale_fixing', 'first_correction_settlement', 'second_correction_settlement', 'third_correction_settlement'))
GO

-- Modify calculation_type column in wholesale_results to snake_case
ALTER TABLE {HIVE_OUTPUT_DATABASE_NAME}.wholesale_results
    DROP CONSTRAINT IF EXISTS calculation_type_chk
GO

UPDATE {HIVE_OUTPUT_DATABASE_NAME}.wholesale_results
SET calculation_type =
    CASE
        WHEN calculation_type = 'WholesaleFixing' THEN 'wholesale_fixing'
        WHEN calculation_type = 'FirstCorrectionSettlement' THEN 'first_correction_settlement'
        WHEN calculation_type = 'SecondCorrectionSettlement' THEN 'second_correction_settlement'
        WHEN calculation_type = 'ThirdCorrectionSettlement' THEN 'third_correction_settlement'
        ELSE calculation_type
    END
GO

ALTER TABLE {HIVE_OUTPUT_DATABASE_NAME}.wholesale_results
    ADD CONSTRAINT calculation_type_chk CHECK (calculation_type IN ('wholesale_fixing', 'first_correction_settlement', 'second_correction_settlement', 'third_correction_settlement'))
GO

-- Modify calculation_type column in monthly_amounts to snake_case
ALTER TABLE {HIVE_OUTPUT_DATABASE_NAME}.monthly_amounts
    DROP CONSTRAINT IF EXISTS calculation_type_chk
GO

UPDATE {HIVE_OUTPUT_DATABASE_NAME}.monthly_amounts
SET calculation_type =
    CASE
        WHEN calculation_type = 'WholesaleFixing' THEN 'wholesale_fixing'
        WHEN calculation_type = 'FirstCorrectionSettlement' THEN 'first_correction_settlement'
        WHEN calculation_type = 'SecondCorrectionSettlement' THEN 'second_correction_settlement'
        WHEN calculation_type = 'ThirdCorrectionSettlement' THEN 'third_correction_settlement'
        ELSE calculation_type
    END
GO

ALTER TABLE {HIVE_OUTPUT_DATABASE_NAME}.monthly_amounts
    ADD CONSTRAINT calculation_type_chk CHECK (calculation_type IN ('wholesale_fixing', 'first_correction_settlement', 'second_correction_settlement', 'third_correction_settlement'))
GO

-- Modify calculation_type column in total_monthly_amounts to snake_case
ALTER TABLE {HIVE_OUTPUT_DATABASE_NAME}.total_monthly_amounts
    DROP CONSTRAINT IF EXISTS calculation_type_chk
GO

UPDATE {HIVE_OUTPUT_DATABASE_NAME}.total_monthly_amounts
SET calculation_type =
    CASE
        WHEN calculation_type = 'WholesaleFixing' THEN 'wholesale_fixing'
        WHEN calculation_type = 'FirstCorrectionSettlement' THEN 'first_correction_settlement'
        WHEN calculation_type = 'SecondCorrectionSettlement' THEN 'second_correction_settlement'
        WHEN calculation_type = 'ThirdCorrectionSettlement' THEN 'third_correction_settlement'
        ELSE calculation_type
    END
GO

ALTER TABLE {HIVE_OUTPUT_DATABASE_NAME}.total_monthly_amounts
    ADD CONSTRAINT calculation_type_chk CHECK (calculation_type IN ('wholesale_fixing', 'first_correction_settlement', 'second_correction_settlement', 'third_correction_settlement'))
GO
