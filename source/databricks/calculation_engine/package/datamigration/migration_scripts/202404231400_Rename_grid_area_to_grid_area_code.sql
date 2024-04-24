--
-- Rename energy_results.grid_area to grid_area_code - including the constraint
--

ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    DROP CONSTRAINT IF EXISTS grid_area_chk
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results SET TBLPROPERTIES (
    'delta.columnMapping.mode' = 'name',
    'delta.minReaderVersion' = '2',
    'delta.minWriterVersion' = '5')
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
RENAME COLUMN grid_area TO grid_area_code
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    ADD CONSTRAINT grid_area_code_chk CHECK (LENGTH(grid_area_code) = 3)
GO

--
-- Rename energy_results.out_grid_area to out_grid_area_code - including the constraint
--

ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    DROP CONSTRAINT IF EXISTS out_grid_area_chk
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
RENAME COLUMN out_grid_area TO out_grid_area_code
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    ADD CONSTRAINT out_grid_area_code_chk CHECK (out_grid_area_code IS NULL OR LENGTH(out_grid_area_code) = 3)
GO


--
-- Rename wholesale_results.grid_area to grid_area_code - including the constraint
--

ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
    DROP CONSTRAINT IF EXISTS grid_area_chk
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results SET TBLPROPERTIES (
    'delta.columnMapping.mode' = 'name',
    'delta.minReaderVersion' = '2',
    'delta.minWriterVersion' = '5')
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
RENAME COLUMN grid_area TO grid_area_code
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
    ADD CONSTRAINT grid_area_code_chk CHECK (LENGTH(grid_area_code) = 3)
GO

--
-- Rename total_monthly_amounts.grid_area to grid_area_code - including the constraint
--

ALTER TABLE {OUTPUT_DATABASE_NAME}.total_monthly_amounts
    DROP CONSTRAINT IF EXISTS grid_area_chk
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.total_monthly_amounts SET TBLPROPERTIES (
    'delta.columnMapping.mode' = 'name',
    'delta.minReaderVersion' = '2',
    'delta.minWriterVersion' = '5')
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.total_monthly_amounts
RENAME COLUMN grid_area TO grid_area_code
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.total_monthly_amounts
    ADD CONSTRAINT grid_area_code_chk CHECK (LENGTH(grid_area_code) = 3)
GO
