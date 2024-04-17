ALTER TABLE {OUTPUT_DATABASE_NAME}.total_monthly_amounts
    ADD CONSTRAINT calculation_id_chk_v2 CHECK (LENGTH(calculation_id) = 36)
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.total_monthly_amounts
    DROP CONSTRAINT IF EXISTS calculation_id_chk_v2
GO
