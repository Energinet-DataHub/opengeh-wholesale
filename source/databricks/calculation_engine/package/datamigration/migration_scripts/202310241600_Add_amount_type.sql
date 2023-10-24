ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
    ADD COLUMN amount_type STRING NOT NULL
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
    ADD CONSTRAINT amount_type_chk
    CHECK (array_size(array_except(amount_type, array('amount_per_charge', 'monthly_amount_per_charge', 'total_monthly_amount'))) = 0
           AND array_size(amount_type) > 0)
GO
