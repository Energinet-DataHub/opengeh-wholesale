--
-- Replace single-valued quantity_quality column with array of qualities
--
ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    ADD COLUMN quantity_qualities ARRAY<STRING>
GO

UPDATE {OUTPUT_DATABASE_NAME}.energy_results
    SET quantity_qualities = ARRAY(quantity_quality)
GO

-- Enable Databricks column mapping mode in order to drop column
ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results SET TBLPROPERTIES (
   'delta.columnMapping.mode' = 'name',
   'delta.minReaderVersion' = '2',
   'delta.minWriterVersion' = '5'
)
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    DROP CONSTRAINT quantity_quality_chk
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    DROP COLUMN quantity_quality
GO

-- Add new qualities constraint to check that all qualities are valid and at least one is present
ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    ADD CONSTRAINT quantity_qualities_chk
    CHECK (array_size(array_except(quantity_qualities, array('missing', 'calculated', 'measured', 'estimated'))) = 0
           AND array_size(quantity_qualities) > 0)
    GO

-- Put the new column in the same position as the old one to avoid breaking schemas and reading/writing
ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    ALTER COLUMN quantity_qualities AFTER quantity
GO
