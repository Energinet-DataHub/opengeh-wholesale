CREATE VIEW {SETTLEMENT_REPORT_DATABASE_NAME}.energy_results_v1 as
SELECT e.calculation_id,
       e.calculation_type,
       e.grid_area_code,
       e.metering_point_type,
       e.settlement_method,
       e.resolution,
       e.time,
       e.quantity,
       e.energy_supplier_id
FROM {OUTPUT_DATABASE_NAME}.energy_results AS e
INNER JOIN (SELECT calculation_id FROM {BASIS_DATA_DATABASE_NAME}.calculations) AS c ON c.calculation_id = e.calculation_id