DROP VIEW IF EXISTS {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_DATABASE_NAME}.charge_price_information_periods_v1
GO

CREATE VIEW IF NOT EXISTS {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_DATABASE_NAME}.charge_price_information_periods_v1 AS
SELECT c.calculation_id,
       c.calculation_type,
       c.calculation_version, 
       p.charge_key,
       p.charge_code,
       p.charge_type,
       p.charge_owner_id,
       p.resolution,
       p.is_tax,
       p.from_date,
       p.to_date
FROM {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.charge_price_information_periods as p
INNER JOIN {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.succeeded_external_calculations_v1 AS c ON c.calculation_id = p.calculation_id
