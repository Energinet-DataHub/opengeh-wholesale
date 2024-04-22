ALTER TABLE {OUTPUT_DATABASE_NAME}.total_monthly_amounts
    DROP CONSTRAINT IF EXISTS calculation_id_chk
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.total_monthly_amounts
    ADD CONSTRAINT calculation_id_chk CHECK (LENGTH(calculation_id) = 36)
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.total_monthly_amounts
    DROP CONSTRAINT IF EXISTS calculation_type_chk
GO
    
ALTER TABLE {OUTPUT_DATABASE_NAME}.total_monthly_amounts
    ADD CONSTRAINT calculation_type_chk CHECK (calculation_type IN ('WholesaleFixing', 'FirstCorrectionSettlement', 'SecondCorrectionSettlement', 'ThirdCorrectionSettlement'))
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.total_monthly_amounts
    DROP CONSTRAINT IF EXISTS calculation_result_id_chk
GO
    
ALTER TABLE {OUTPUT_DATABASE_NAME}.total_monthly_amounts
    ADD CONSTRAINT calculation_result_id_chk CHECK (LENGTH(calculation_result_id) = 36)
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.total_monthly_amounts
    DROP CONSTRAINT IF EXISTS grid_area_chk
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.total_monthly_amounts
    ADD CONSTRAINT grid_area_chk CHECK (LENGTH(grid_area) = 3)
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.total_monthly_amounts
    DROP CONSTRAINT IF EXISTS energy_supplier_id_chk
GO
    
-- Length is 16 when EIC and 13 when GLN
ALTER TABLE {OUTPUT_DATABASE_NAME}.total_monthly_amounts
    ADD CONSTRAINT energy_supplier_id_chk CHECK (LENGTH(energy_supplier_id) = 13 OR LENGTH(energy_supplier_id) = 16)
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.total_monthly_amounts
    DROP CONSTRAINT IF EXISTS charge_owner_id_chk
GO

-- Length is 16 when EIC and 13 when GLN
ALTER TABLE {OUTPUT_DATABASE_NAME}.total_monthly_amounts
    ADD CONSTRAINT charge_owner_id_chk CHECK (charge_owner_id IS NULL OR LENGTH(charge_owner_id) = 13 OR LENGTH(charge_owner_id) = 16)
GO
