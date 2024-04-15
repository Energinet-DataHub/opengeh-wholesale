ALTER TABLE {OUTPUT_DATABASE_NAME}.total_monthly_amounts
    DROP CONSTRAINT IF EXISTS energy_supplier_id_chk
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.total_monthly_amounts
    ADD CONSTRAINT energy_supplier_id_chk CHECK (LENGTH(energy_supplier_id) = 13 OR LENGTH(energy_supplier_id) = 16)
GO


