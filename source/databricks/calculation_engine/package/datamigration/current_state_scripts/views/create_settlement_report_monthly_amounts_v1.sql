CREATE VIEW {SETTLEMENT_REPORT_DATABASE_NAME}.monthly_amounts_v1 as
SELECT c.calculation_id,
       c.calculation_type,
       c.version as calculation_version,
       wr.grid_area_code,
       wr.energy_supplier_id,
       wr.time,
       wr.resolution,
       wr.quantity_unit,
       "DKK" as currency,
       wr.amount,
       wr.charge_type,
       wr.charge_code,
       wr.charge_owner_id
FROM {OUTPUT_DATABASE_NAME}.wholesale_results AS wr
INNER JOIN {BASIS_DATA_DATABASE_NAME}.calculations AS c ON c.calculation_id = wr.calculation_id
WHERE wr.amount_type = "monthly_amount_per_charge"

UNION

SELECT c.calculation_id,
       c.calculation_type,
       c.version as calculation_version
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