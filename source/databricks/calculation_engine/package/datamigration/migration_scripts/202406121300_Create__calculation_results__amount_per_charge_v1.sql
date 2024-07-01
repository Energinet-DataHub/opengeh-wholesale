CREATE VIEW IF NOT EXISTS {CALCULATION_RESULTS_DATABASE_NAME}.amount_per_charge_v1 AS
SELECT c.calculation_id,
       c.calculation_type,
       c.version as calculation_version,
       wr.calculation_result_id as result_id,
       wr.grid_area_code,
       wr.energy_supplier_id,
       wr.charge_code,
       wr.charge_type,
       wr.charge_owner_id,
       wr.resolution,
       wr.quantity_unit,
       wr.metering_point_type,
       wr.settlement_method,
       wr.is_tax,
       "DKK" as currency,
       time,
       quantity,
       quantity_qualities,
       price,
       amount
FROM {HIVE_OUTPUT_DATABASE_NAME}.wholesale_results as wr
INNER JOIN {BASIS_DATA_DATABASE_NAME}.calculations AS c ON c.calculation_id = wr.calculation_id
WHERE wr.amount_type = "amount_per_charge"
