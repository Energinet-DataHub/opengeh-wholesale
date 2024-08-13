CREATE VIEW IF NOT EXISTS {CATALOG_NAME}.{WHOLESALE_RESULTS_DATABASE_NAME}.active_calculations_v1 as
WITH calculations_by_day AS (
  SELECT
    c.calculation_id,
    calculation_type,
    c.calculation_id,
    calculation_type,
    calculation_version,
    cga.grid_area_code,
    explode(sequence(
      DATE(FROM_UTC_TIMESTAMP(calculation_period_start, 'Europe/Copenhagen')),
      DATE_SUB(DATE(FROM_UTC_TIMESTAMP(calculation_period_end, 'Europe/Copenhagen')), 1),
      interval 1 day
    )) AS from_date_local,
    DATE_ADD(from_date_local, 1) AS to_date_local,
    calculation_period_start,
    calculation_period_end,
    calculation_execution_time_start as active_from_time
  FROM {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations c
  INNER JOIN {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculation_grid_areas cga ON c.calculation_id = cga.calculation_id
)
SELECT
  calculation_id,
  calculation_type,
  calculation_version,
  grid_area_code,
  TO_UTC_TIMESTAMP(from_date_local, 'Europe/Copenhagen') AS from_date,
  TO_UTC_TIMESTAMP(to_date_local, 'Europe/Copenhagen') AS to_date,
  active_from_time,
  LEAD(active_from_time) OVER (PARTITION BY calculation_type, grid_area_code, from_date_local ORDER BY calculation_version ASC) AS active_to_time
FROM calculations_by_day

