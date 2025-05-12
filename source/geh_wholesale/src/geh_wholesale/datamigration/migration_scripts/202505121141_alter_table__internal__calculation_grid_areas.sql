-- CREATE TABLE IF NOT EXISTS {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculation_grid_areas
-- (
--     -- 36 characters UUID
--     calculation_id STRING NOT NULL,
--     grid_area_code STRING NOT NULL
-- )
-- USING DELTA
-- TBLPROPERTIES (
--     delta.deletedFileRetentionDuration = 'interval 30 days',
--     delta.constraints.calculation_id_chk = "LENGTH ( calculation_id ) = 36",
--     delta.constraints.grid_area_code_chk = "LENGTH ( grid_area_code ) = 3"
-- )

ALTER TABLE {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculation_grid_areas
CLUSTER BY (calculation_id, grid_area_code)