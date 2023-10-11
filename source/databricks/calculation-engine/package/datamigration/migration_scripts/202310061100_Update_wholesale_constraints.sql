--
-- Drop constraint energy_supplier_id_chk
--
ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
    DROP CONSTRAINT energy_supplier_id_chk
GO

--
-- Add new constraint to energy_supplier_id
--
ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
    ADD CONSTRAINT energy_supplier_id_chk CHECK (energy_supplier_id IS NOT NULL AND LENGTH(energy_supplier_id) = 13 OR LENGTH(energy_supplier_id) = 16)
GO
