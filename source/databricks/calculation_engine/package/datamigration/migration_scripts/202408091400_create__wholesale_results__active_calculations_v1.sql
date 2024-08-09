CREATE VIEW IF NOT EXISTS {CATALOG_NAME}.{WHOLESALE_RESULTS_DATABASE_NAME}.active_calculations_v1 as
SELECT calculation_id,
       calculation_type,
       calculation_version,
       grid_area_code,
       date,
       active_from_date,
       active_to_date
FROM {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.calculations as c
INNER JOIN {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.calculation_grid_areas as cga ON c.calculation_id = cga.calculation_id
