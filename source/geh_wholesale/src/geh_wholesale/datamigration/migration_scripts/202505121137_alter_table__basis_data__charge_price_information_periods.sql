ALTER TABLE {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.charge_price_information_periods
CLUSTER BY (calculation_id, charge_key, from_date, to_date)