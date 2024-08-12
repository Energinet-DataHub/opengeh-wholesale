CREATE VIEW IF NOT EXISTS {CATALOG_NAME}.{WHOLESALE_RESULTS_DATABASE_NAME}.active_calculations_v1 as
WITH calculations_by_day AS (
  SELECT
    c.calculation_id,
    calculation_type,
    calculation_version,
    explode(sequence(
      calculation_period_start,
      calculation_period_end,
      interval 1 day
    )) AS date,
    calculation_execution_time_start,
    cga.grid_area_code,
    calculation_period_end
  FROM {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations c
  INNER JOIN {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculation_grid_areas cga ON c.calculation_id = cga.calculation_id
)
SELECT
  calculation_id,
  calculation_type,
  calculation_version,
  grid_area_code,
  date,
  calculation_execution_time_start as active_from_date,
  LEAD(calculation_execution_time_start) OVER (PARTITION BY grid_area_code, date ORDER BY calculation_version ASC) AS active_to_date
FROM calculations_by_day
WHERE date < calculation_period_end
