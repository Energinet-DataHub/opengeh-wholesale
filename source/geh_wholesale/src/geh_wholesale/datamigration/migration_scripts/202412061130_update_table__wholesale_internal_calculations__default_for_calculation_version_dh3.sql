-- Sets the default values for this new column, so that it is not null always.
UPDATE {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations
SET calculation_version_dh3_temp = CASE WHEN calculation_version_dh2 IS NOT NULL THEN calculation_version_dh2 ELSE calculation_version END
