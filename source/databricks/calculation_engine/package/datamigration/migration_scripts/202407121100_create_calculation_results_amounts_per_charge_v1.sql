CREATE VIEW IF NOT EXISTS {CATALOG_NAME}.{WHOLESALE_RESULTS_DATABASE_NAME}.amounts_per_charge_v1 AS
SELECT c.calculation_id,
       c.calculation_type,
       c.calculation_version,
       apc.result_id,
       apc.grid_area_code,
       apc.energy_supplier_id,
       apc.charge_code,
       apc.charge_type,
       apc.charge_owner_id,
       apc.resolution,
       apc.quantity_unit,
       apc.metering_point_type,
       apc.settlement_method,
       apc.is_tax,
       "DKK" as currency,
       apc.time,
       apc.quantity,
       apc.quantity_qualities,
       apc.price,
       apc.amount
FROM {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.amounts_per_charge as apc
INNER JOIN {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations AS c ON c.calculation_id = apc.calculation_id
