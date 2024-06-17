DROP VIEW IF EXISTS {SETTLEMENT_REPORT_DATABASE_NAME}.monthly_amounts_v1
GO

CREATE VIEW IF NOT EXISTS {SETTLEMENT_REPORT_DATABASE_NAME}.monthly_amounts_v1 as
SELECT c.calculation_id,
       c.calculation_type,
       c.version as calculation_version,
       ma.calculation_result_id as result_id,
       ma.grid_area_code,
       ma.energy_supplier_id,
       ma.time,
       "P1M" as resolution,
       ma.quantity_unit,
       "DKK" as currency,
       ma.amount,
       ma.charge_type,
       ma.charge_code,
       ma.charge_owner_id
FROM {OUTPUT_DATABASE_NAME}.monthly_amounts AS ma
INNER JOIN {BASIS_DATA_DATABASE_NAME}.calculations AS c ON c.calculation_id = ma.calculation_id

UNION

SELECT c.calculation_id,
       c.calculation_type,
       c.version as calculation_version,
       tma.calculation_result_id as result_id,
       tma.grid_area_code,
       tma.energy_supplier_id,
       tma.time,
       "P1M" as resolution,
       "kWh" as quantity_unit,
       "DKK" as currency,
       tma.amount,
       NULL as charge_type,
       NULL as charge_code,
       tma.charge_owner_id
FROM {OUTPUT_DATABASE_NAME}.total_monthly_amounts AS tma
INNER JOIN {BASIS_DATA_DATABASE_NAME}.calculations AS c ON c.calculation_id = tma.calculation_id