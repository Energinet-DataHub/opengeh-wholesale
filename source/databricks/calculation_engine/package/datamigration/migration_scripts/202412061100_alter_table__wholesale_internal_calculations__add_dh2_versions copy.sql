-- Adds new columns for DH2 calculation versions to be filled out my a manual migration script.
-- Requires a follow-up script too.
ALTER TABLE {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations SET TBLPROPERTIES (
    'delta.columnMapping.mode' = 'name'
  )
GO

ALTER TABLE {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations
ADD COLUMNS (calculation_version_dh2 BIGINT, calculation_version_dh3 BIGINT)
