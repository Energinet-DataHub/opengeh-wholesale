--
-- Rename energy_results.out_grid_area_code to neighbour_grid_area_code - including the constraint
--

ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    DROP CONSTRAINT IF EXISTS out_grid_area_code_chk
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
RENAME COLUMN out_grid_area_code TO neighbor_grid_area_code
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    ADD CONSTRAINT neighbor_grid_area_code_chk CHECK (neighbor_grid_area_code IS NULL OR LENGTH(neighbor_grid_area_code) = 3)
GO
