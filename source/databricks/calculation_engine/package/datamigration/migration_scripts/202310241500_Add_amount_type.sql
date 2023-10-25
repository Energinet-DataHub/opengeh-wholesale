ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
    ADD COLUMN amount_type STRING
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
    ADD CONSTRAINT amount_type_chk CHECK (amount_type IN ('amount_per_charge', 'monthly_amount_per_charge', 'total_monthly_amount'))
GO
