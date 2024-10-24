DROP VIEW IF EXISTS {HIVE_SETTLEMENT_REPORT_DATABASE_NAME}.charge_link_periods_v1;
GO

CREATE VIEW IF NOT EXISTS {HIVE_SETTLEMENT_REPORT_DATABASE_NAME}.charge_link_periods_v1 as
SELECT c.calculation_id,
       c.calculation_type,
       c.version as calculation_version,
       m.metering_point_id,
       m.metering_point_type,
       l.charge_type,
       l.charge_code,
       l.charge_owner_id,
       l.quantity as charge_link_quantity,
       l.from_date,
       l.to_date,
       m.grid_area_code,
       m.energy_supplier_id,
       cp.is_tax
FROM {HIVE_BASIS_DATA_DATABASE_NAME}.charge_link_periods AS l
INNER JOIN {HIVE_BASIS_DATA_DATABASE_NAME}.metering_point_periods AS m ON m.metering_point_id = l.metering_point_id AND m.calculation_id = l.calculation_id
INNER JOIN {HIVE_BASIS_DATA_DATABASE_NAME}.calculations AS c ON c.calculation_id = l.calculation_id
INNER JOIN {HIVE_BASIS_DATA_DATABASE_NAME}.charge_price_information_periods AS cp ON cp.calculation_id = l.calculation_id AND cp.charge_key = l.charge_key
