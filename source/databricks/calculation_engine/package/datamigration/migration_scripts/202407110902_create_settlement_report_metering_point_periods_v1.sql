CREATE VIEW IF NOT EXISTS {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.{WHOLESALE_SETTLEMENT_REPORTS_DATABASE_NAME}.metering_point_periods_v1 AS
SELECT c.calculation_id,
       c.calculation_type,
       c.version as calculation_version,
       m.metering_point_id,
       m.from_date,
       m.to_date,
       m.grid_area_code,
       m.from_grid_area_code,
       m.to_grid_area_code,
       m.metering_point_type,
       m.settlement_method,
       m.energy_supplier_id
FROM {HIVE_BASIS_DATA_DATABASE_NAME}.metering_point_periods as m
INNER JOIN {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations AS c ON c.calculation_id = m.calculation_id
WHERE c.calculation_type IN ('balance_fixing', 'wholesale_fixing', 'first_correction_settlement', 'second_correction_settlement', 'third_correction_settlement')

