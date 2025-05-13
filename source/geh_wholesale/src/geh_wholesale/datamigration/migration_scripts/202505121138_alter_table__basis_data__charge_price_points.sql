ALTER TABLE {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.charge_price_points
CLUSTER BY (calculation_id, charge_key, charge_time)