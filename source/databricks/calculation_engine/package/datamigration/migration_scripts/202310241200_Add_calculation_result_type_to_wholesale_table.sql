ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
    ADD COLUMN result_type STRING
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
    ADD CONSTRAINT result_type_chk
    CHECK (array_size(array_except(result_type, array('amount_per_charge', 'monthly_amount_per_charge', 'total_monthly_amount'))) = 0
           AND array_size(result_type) > 0)
GO
