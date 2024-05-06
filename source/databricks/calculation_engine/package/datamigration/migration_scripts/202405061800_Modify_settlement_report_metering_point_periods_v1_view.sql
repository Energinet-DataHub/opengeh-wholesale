DROP VIEW IF EXISTS {SETTLEMENT_REPORT_DATABASE_NAME}.metering_point_periods_v1
GO

CREATE VIEW IF NOT EXISTS {SETTLEMENT_REPORT_DATABASE_NAME}.metering_point_periods_v1 as
SELECT calculation_id,
       metering_point_id,
       from_date,
       to_date,
       grid_area_code,
       from_grid_area_code,
       to_grid_area_code,
       metering_point_type,
       settlement_method,
       energy_supplier_id
FROM {BASIS_DATA_DATABASE_NAME}.metering_point_periods
INNER JOIN (SELECT calculation_id FROM {BASIS_DATA_DATABASE_NAME}.calculations) AS c ON c.calculation_id = m.calculation_id
WHERE metering_point_type != 'exchange'
