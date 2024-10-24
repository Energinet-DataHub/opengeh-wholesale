CREATE VIEW IF NOT EXISTS {HIVE_SETTLEMENT_REPORT_DATABASE_NAME}.metering_point_periods as
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
FROM {HIVE_BASIS_DATA_DATABASE_NAME}.metering_point_periods