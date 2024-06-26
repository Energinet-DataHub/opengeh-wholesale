CREATE VIEW IF NOT EXISTS {CALCULATION_RESULTS_DATABASE_NAME}.amounts_per_charge_v1 AS
SELECT c.calculation_id,
       c.calculation_type,
       c.version as calculation_version,
       wr.calculation_result_id as result_id,
       wr.grid_area_code,
       wr.energy_supplier_id,
       CAST(COALESCE(wr.charge_code, 'ERROR') AS STRING) as charge_code, -- Hack to make column NOT NULL. Defaults to 'ERROR'.
       CAST(COALESCE(wr.charge_type, 'ERROR') AS STRING) as charge_type, -- Hack to make column NOT NULL. Defaults to 'ERROR'.
       CAST(COALESCE(wr.charge_owner_id, 'ERROR') AS STRING) as charge_owner_id, -- Hack to make column NOT NULL. Defaults to 'ERROR'. wr.resolution,
       wr.quantity_unit,
       COALESCE(wr.metering_point_type, 'ERROR') as metering_point_type, -- Hack to make column NOT NULL. Defaults to 'ERROR'.
       wr.settlement_method,
       COALESCE(wr.is_tax, false) as is_tax, -- Hack to make column NOT NULL. Defaults to false.
       "DKK" as currency,
       time,
       quantity,
       quantity_qualities,
       price,
       amount
FROM {OUTPUT_DATABASE_NAME}.wholesale_results as wr
INNER JOIN {BASIS_DATA_DATABASE_NAME}.calculations AS c ON c.calculation_id = wr.calculation_id
WHERE wr.amount_type = "amount_per_charge"
