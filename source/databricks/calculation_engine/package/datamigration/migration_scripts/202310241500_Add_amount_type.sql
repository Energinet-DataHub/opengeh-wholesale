ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
    ADD COLUMN amount_type STRING
GO

UPDATE {OUTPUT_DATABASE_NAME}.wholesale_results
SET amount_type =
  CASE
    WHEN resolution=='P1M' AND charge_code IS NOT NULL THEN 'monthly_amount_per_charge'
    WHEN resolution=='P1M' AND charge_code IS NULL THEN 'total_monthly_amount'
    ELSE 'amount_per_charge'
END
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
    ADD CONSTRAINT amount_type_chk CHECK (amount_type IN ('amount_per_charge', 'monthly_amount_per_charge', 'total_monthly_amount'))
GO
