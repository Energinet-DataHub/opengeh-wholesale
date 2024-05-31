-- This view represents the current state of the wholesale_results table with filter by "amount_per_charge".
CREATE VIEW {SETTLEMENT_REPORT_DATABASE_NAME}.wholesale_results_v1 as
SELECT c.calculation_id,
       c.calculation_type,
       c.version as calculation_version,
       wr.grid_area_code,
       wr.energy_supplier_id,
       wr.time,
       wr.resolution,
       wr.metering_point_type,
       wr.settlement_method,
       wr.quantity_unit,
       "DKK" as currency,
       wr.quantity,
       wr.price,
       wr.amount,
       wr.charge_type,
       wr.charge_code,
       wr.charge_owner_id
FROM {OUTPUT_DATABASE_NAME}.wholesale_results AS wr
INNER JOIN {BASIS_DATA_DATABASE_NAME}.calculations AS c ON c.calculation_id = wr.calculation_id
WHERE wr.amount_type = "amount_per_charge"
