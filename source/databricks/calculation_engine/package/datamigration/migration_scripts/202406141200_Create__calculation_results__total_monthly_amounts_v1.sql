CREATE VIEW IF NOT EXISTS {CALCULATION_RESULTS_DATABASE_NAME}.total_monthly_amounts_v1 AS
SELECT c.calculation_id,
       c.calculation_type,
       c.version as calculation_version,
       ma.calculation_result_id as result_id,
       ma.grid_area_code,
       ma.energy_supplier_id,
       ma.charge_code,
       ma.charge_type,
       ma.charge_owner_id,
       ma.quantity_unit,
       ma.is_tax,
       "DKK" as currency,
       ma.time,
       ma.amount
FROM {OUTPUT_DATABASE_NAME}.monthly_amounts as ma
INNER JOIN {BASIS_DATA_DATABASE_NAME}.calculations AS c ON c.calculation_id = ma.calculation_id
