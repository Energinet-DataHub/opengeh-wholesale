ALTER TABLE {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations
    DROP CONSTRAINT IF EXISTS is_control_calculation_chk
GO

ALTER TABLE {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations
    RENAME COLUMN is_control_calculation TO is_internal_calculation
GO

ALTER TABLE {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations
    ADD CONSTRAINT is_internal_calculation_chk CHECK (is_internal_calculation IS NOT NULL)
GO
