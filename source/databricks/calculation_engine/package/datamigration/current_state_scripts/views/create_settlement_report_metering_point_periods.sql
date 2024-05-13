CREATE VIEW IF NOT EXISTS {SETTLEMENT_REPORT_DATABASE_NAME}.metering_point_periods_v1 as
SELECT m.calculation_id,
       m.metering_point_id,
       m.from_date,
       m.to_date,
       m.grid_area_code,
       m.metering_point_type,
       m.settlement_method,
       m.energy_supplier_id
FROM {BASIS_DATA_DATABASE_NAME}.metering_point_periods as m
INNER JOIN {BASIS_DATA_DATABASE_NAME}.calculations AS c ON c.calculation_id = m.calculation_id