CREATE VIEW IF NOT EXISTS {CALCULATION_RESULTS_DATABASE_NAME}.total_monthly_amounts_v1 AS
SELECT c.calculation_id,
       c.calculation_type,
       c.version as calculation_version,
       tma.calculation_result_id as result_id,
       tma.grid_area_code,
       tma.energy_supplier_id,
       tma.charge_owner_id,
       "DKK" as currency,
       tma.time,
       tma.amount
FROM {HIVE_OUTPUT_DATABASE_NAME}.total_monthly_amounts as tma
INNER JOIN {BASIS_DATA_DATABASE_NAME}.calculations AS c ON c.calculation_id = tma.calculation_id