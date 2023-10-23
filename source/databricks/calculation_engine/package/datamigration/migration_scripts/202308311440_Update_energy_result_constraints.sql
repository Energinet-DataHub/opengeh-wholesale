ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    DROP CONSTRAINT IF EXISTS energy_supplier_id_chk
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    DROP CONSTRAINT IF EXISTS balance_responsible_id_chk
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    DROP CONSTRAINT IF EXISTS calculation_id_chk
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    DROP CONSTRAINT IF EXISTS calculation_result_id_chk
GO

-- Length is 16 when EIC and 13 when GLN
ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    ADD CONSTRAINT energy_supplier_id_chk CHECK (energy_supplier_id IS NULL OR LENGTH(energy_supplier_id) = 13 OR LENGTH(energy_supplier_id) = 16)
GO
-- Length is 16 when EIC and 13 when GLN
ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    ADD CONSTRAINT balance_responsible_id_chk CHECK (balance_responsible_id IS NULL OR LENGTH(balance_responsible_id) = 13 OR LENGTH(balance_responsible_id) = 16)
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    ADD CONSTRAINT calculation_id_chk CHECK (LENGTH(calculation_id) = 36)
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    ADD CONSTRAINT calculation_result_id_chk CHECK (LENGTH(calculation_result_id) = 36)
GO
