-- Adds new columns for DH2 calculation versions to be filled out my a manual migration script.
-- Sets the default values for this new column, so that it is not null always.
UPDATE {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations
SET calculation_version_dh3 = CASE WHEN calculation_version_dh2 IS NOT NULL THEN calculation_version_dh2 ELSE calculation_version
