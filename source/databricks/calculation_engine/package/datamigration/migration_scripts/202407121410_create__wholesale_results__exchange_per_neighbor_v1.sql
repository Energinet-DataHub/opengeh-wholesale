CREATE VIEW IF NOT EXISTS {CATALOG_NAME}.{WHOLESALE_RESULTS_DATABASE_NAME}.exchange_per_neighbor_v1 as
SELECT c.calculation_id,
       calculation_type,
       calculation_period_start,
       calculation_period_end,
       grid_area_code,
       neighbor_grid_area_code,
       resolution,
       time,
       quantity,
       'kWh' as quantity_unit,
       quantity_qualities
FROM {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.exchange_per_neighbor_ga AS e
INNER JOIN {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations AS c ON c.calculation_id = e.calculation_id
