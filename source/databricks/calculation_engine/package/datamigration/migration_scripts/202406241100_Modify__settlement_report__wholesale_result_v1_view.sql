-- Make columns NOT NULL

DROP VIEW IF EXISTS {SETTLEMENT_REPORT_DATABASE_NAME}.wholesale_results_v1
GO

-- This view represents the current state of the wholesale_results table with filter by "amount_per_charge".
CREATE VIEW {SETTLEMENT_REPORT_DATABASE_NAME}.wholesale_results_v1 as
SELECT wr.calculation_id,
       wr.calculation_type,
       c.version as calculation_version,
       wr.calculation_result_id as result_id,
       wr.grid_area_code,
       wr.energy_supplier_id,
       wr.time,
       wr.resolution,
       COALESCE(wr.metering_point_type, 'ERROR') as metering_point_type, -- Hack to make column NOT NULL. Defaults to 'ERROR'.
       wr.settlement_method,
       wr.quantity_unit,
       "DKK" as currency,
       wr.quantity,
       wr.price,
       wr.amount,
       COALESCE(wr.charge_type, 'ERROR') as charge_type, -- Hack to make column NOT NULL. Defaults to 'ERROR'.
       COALESCE(wr.charge_code, 'ERROR') as charge_code, -- Hack to make column NOT NULL. Defaults to 'ERROR'.
       COALESCE(wr.charge_owner_id, 'ERROR') as charge_owner_id -- Hack to make column NOT NULL. Defaults to 'ERROR'.
FROM {OUTPUT_DATABASE_NAME}.wholesale_results AS wr
INNER JOIN {BASIS_DATA_DATABASE_NAME}.calculations AS c ON c.calculation_id = wr.calculation_id
WHERE wr.amount_type = "amount_per_charge"
