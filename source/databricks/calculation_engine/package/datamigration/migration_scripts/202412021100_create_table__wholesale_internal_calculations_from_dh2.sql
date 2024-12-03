-- This copy of dh2 calculations is done, such that Wholesale can re-write their input as they please, without affecting Wholesale. 
-- In theory this could be replaced by a view on the Volt input, but then we might accidentally lose data.
ALTER TABLE {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations 
RENAME COLUMN calculation_version TO calculation_version_dh3
GO 

ALTER TABLE {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations
ADD COLUMN (calculation_version_dh2 bigint, calculation_version GENERATED ALWAYS AS (CASE WHEN calculation_version_dh2 IS NOT NULL THEN 0 ELSE calculation_version_dh3 END))
