DROP VIEW IF EXISTS {CATALOG_NAME}.{WHOLESALE_SETTLEMENT_REPORTS_DATABASE_NAME}.charge_link_periods_v1
GO

CREATE VIEW {CATALOG_NAME}.{WHOLESALE_SETTLEMENT_REPORTS_DATABASE_NAME}.charge_link_periods_v1 as
SELECT distinct c.calculation_id,
       c.calculation_type,
       c.calculation_version,
       m.metering_point_id,
       m.metering_point_type,
       l.charge_type,
       l.charge_code,
       l.charge_owner_id,
       l.quantity as charge_link_quantity,
    CASE
        WHEN l.from_date > m.from_date THEN l.from_date
        ELSE m.from_date
    END as from_date,
    CASE
        WHEN l.to_date < m.to_date THEN l.to_date
        ELSE m.to_date
    END as to_date,
       m.grid_area_code,
       m.energy_supplier_id,
       cp.is_tax
FROM {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.charge_link_periods AS l
INNER JOIN {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.metering_point_periods AS m ON m.metering_point_id = l.metering_point_id AND m.calculation_id = l.calculation_id
INNER JOIN {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.succeeded_external_calculations_v1 AS c ON c.calculation_id = l.calculation_id
INNER JOIN {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.charge_price_information_periods AS cp ON cp.calculation_id = l.calculation_id AND cp.charge_key = l.charge_key
WHERE l.from_date < m.to_date AND l.to_date > m.from_date
