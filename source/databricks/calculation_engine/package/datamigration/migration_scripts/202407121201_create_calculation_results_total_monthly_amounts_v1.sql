CREATE VIEW IF NOT EXISTS {CATALOG_NAME}.{WHOLESALE_RESULTS_DATABASE_NAME}.total_monthly_amounts_v1 AS
SELECT c.calculation_id,
       c.calculation_type,
       c.calculation_version,
       tma.result_id,
       tma.grid_area_code,
       tma.energy_supplier_id,
       tma.charge_owner_id,
       "DKK" as currency,
       tma.time,
       tma.amount
FROM {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.total_monthly_amounts as tma
INNER JOIN {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations AS c ON c.calculation_id = tma.calculation_id