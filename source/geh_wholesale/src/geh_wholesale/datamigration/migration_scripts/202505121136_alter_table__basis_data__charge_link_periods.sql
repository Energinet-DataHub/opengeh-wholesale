ALTER TABLE {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.charge_link_periods
CLUSTER BY (calculation_id, metering_point_id, from_date, to_date)