-- Swaps the column namings to make the calculation_version the "combined" column rather than just for DH3.
ALTER TABLE {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations 
RENAME COLUMN calculation_version TO calculation_version_dh3
GO

ALTER TABLE {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations 
RENAME COLUMN calculation_version_dh3_temp TO calculation_version