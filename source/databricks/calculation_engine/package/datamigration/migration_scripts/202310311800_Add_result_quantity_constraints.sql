--
-- Make quantity NOT NULL
--
UPDATE {OUTPUT_DATABASE_NAME}.energy_results
SET quantity = -999999.999
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    ADD CONSTRAINT quantity_chk CHECK (quantity IS NOT NULL)
GO
