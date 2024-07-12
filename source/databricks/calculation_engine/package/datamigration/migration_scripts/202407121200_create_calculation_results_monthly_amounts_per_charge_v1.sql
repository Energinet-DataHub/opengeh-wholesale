CREATE VIEW IF NOT EXISTS {CATALOG_NAME}.{WHOLESALE_RESULTS_DATABASE_NAME}.monthly_amounts_per_charge_v1 AS
SELECT c.calculation_id,
       c.calculation_type,
       c.calculation_version,
       ma.result_id,
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
FROM {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.monthly_amounts_per_charge as ma
INNER JOIN {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations AS c ON c.calculation_id = ma.calculation_id
