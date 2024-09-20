DROP VIEW IF EXISTS {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_DATABASE_NAME}.metering_point_periods_v1
GO

CREATE VIEW {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_DATABASE_NAME}.metering_point_periods_v1 AS
SELECT c.calculation_id,
       c.calculation_type,
       c.calculation_version,
       m.metering_point_id,
       m.from_date,
       m.to_date,
       m.grid_area_code,
       m.from_grid_area_code,
       m.to_grid_area_code,
       m.metering_point_type,
       m.settlement_method,
       m.energy_supplier_id
FROM {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.metering_point_periods as m
INNER JOIN {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.succeeded_external_calculations_v1 AS c ON c.calculation_id = m.calculation_id
