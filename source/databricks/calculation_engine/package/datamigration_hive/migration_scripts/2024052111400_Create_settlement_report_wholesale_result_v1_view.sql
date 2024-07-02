DROP VIEW IF EXISTS {SETTLEMENT_REPORT_DATABASE_NAME}.wholesale_results_v1
GO

-- This view represents the current state of the wholesale_results table with filter by "amount_per_charge".
CREATE VIEW {SETTLEMENT_REPORT_DATABASE_NAME}.wholesale_results_v1 as
SELECT wr.calculation_id,
       wr.calculation_type,
       wr.grid_area_code,
       wr.energy_supplier_id,
       wr.time AS start_date_time,
       wr.resolution,
       wr.quantity_unit,
       "DKK" as currency,
       wr.quantity,
       wr.price,
       wr.amount,
       wr.charge_type,
       wr.charge_code,
       wr.charge_owner_id
FROM {HIVE_OUTPUT_DATABASE_NAME}.wholesale_results AS wr
INNER JOIN {BASIS_DATA_DATABASE_NAME}.calculations AS c ON c.calculation_id = wr.calculation_id
WHERE wr.amount_type = "amount_per_charge"

