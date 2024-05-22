CREATE VIEW IF NOT EXISTS {SETTLEMENT_REPORT_DATABASE_NAME}.total_monthly_amounts_v1 as
SELECT tma.calculation_id,
       tma.calculation_type,
       tma.grid_area_code,
       tma.energy_supplier_id,
       tma.time,
       "P1M" as resolution,
       wr.quantity_unit,
       "DDK" as currency,
       tma.amount,
       wr.charge_type,
       wr.charge_code,
       tma.charge_owner_id
FROM {OUTPUT_DATABASE_NAME}.total_monthly_amounts AS tma
INNER JOIN {OUTPUT_DATABASE_NAME}.wholesale_results AS wr ON wr.calculation_id = tma.calculation_id
INNER JOIN {BASIS_DATA_DATABASE_NAME}.calculations AS c ON c.calculation_id = wr.calculation_id
WHERE wr.amount_type = "monthly_amount_per_charge"