DROP VIEW IF EXISTS {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_DATABASE_NAME}.charge_link_periods_v1
GO

CREATE VIEW IF NOT EXISTS {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_DATABASE_NAME}.charge_link_periods_v1 AS
SELECT c.calculation_id,
       c.calculation_type,
       c.calculation_version,
       l.charge_key,
       l.charge_code,
       l.charge_type,
       l.charge_owner_id,
       l.metering_point_id,
       l.quantity,
       l.from_date,
       l.to_date
FROM {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.charge_link_periods as l
INNER JOIN {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.succeeded_external_calculations_v1 AS c ON c.calculation_id = l.calculation_id
