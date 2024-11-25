-- This view breaks down each calculation period into daily intervals for each of the calculation's grid areas,
-- and calculates the time period for which the calculation is the latest calculation. The time period is defined as
-- the time between the calculation's succeeded time and the succeeded time of the next calculation (if a
-- newer calculation covers the same day, calculation type and grid area). Note that internal and external calculations
-- are not distinguished in this view - the view returns the latest calculation no matter if it is internal or external.
DROP VIEW IF EXISTS {CATALOG_NAME}.{WHOLESALE_SAP_DATABASE_NAME}.latest_calculations_history_v1
GO

CREATE VIEW IF NOT EXISTS {CATALOG_NAME}.{WHOLESALE_SAP_DATABASE_NAME}.latest_calculations_history_v1 as
WITH calculations_by_day AS (
  SELECT
    c.calculation_id,
    calculation_type,
    calculation_version,
    cga.grid_area_code,
    explode(sequence(
      DATE(FROM_UTC_TIMESTAMP(calculation_period_start, 'Europe/Copenhagen')),
      DATE_SUB(DATE(FROM_UTC_TIMESTAMP(calculation_period_end, 'Europe/Copenhagen')), 1),
      interval 1 day
    )) AS from_date_local,
    DATE_ADD(DATE(FROM_UTC_TIMESTAMP(calculation_period_start, 'Europe/Copenhagen')), 1) AS to_date_local,
    calculation_succeeded_time as latest_from_time,
    ROW_NUMBER() OVER (PARTITION BY calculation_type, grid_area_code, DATE(FROM_UTC_TIMESTAMP(calculation_period_start, 'Europe/Copenhagen')) ORDER BY calculation_version ASC) AS row_num
  FROM {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations c
  INNER JOIN {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculation_grid_areas cga ON c.calculation_id = cga.calculation_id
  WHERE calculation_succeeded_time IS NOT NULL
)
SELECT
  a.calculation_id,
  a.calculation_type,
  a.calculation_version,
  a.grid_area_code,
  TO_UTC_TIMESTAMP(a.from_date_local, 'Europe/Copenhagen') AS from_date,
  TO_UTC_TIMESTAMP(a.to_date_local, 'Europe/Copenhagen') AS to_date,
  a.latest_from_time,
  b.latest_from_time AS latest_to_time
FROM calculations_by_day a
LEFT JOIN calculations_by_day b
ON a.calculation_type = b.calculation_type
AND a.grid_area_code = b.grid_area_code
AND a.from_date_local = b.from_date_local
AND a.row_num = b.row_num - 1
