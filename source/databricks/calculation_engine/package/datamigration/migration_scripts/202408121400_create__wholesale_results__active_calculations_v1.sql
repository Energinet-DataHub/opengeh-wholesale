-- This view breaks down each calculation period into daily intervals for each of the calculation's grid areas,
-- and calculates the active time period for each the rows. The active time period is defined as the time period
-- between the calculation's execution start time and the execution start time of the next calculation (if another
-- calculation covers the same day, calculation type and grid area)

CREATE VIEW IF NOT EXISTS {CATALOG_NAME}.{WHOLESALE_RESULTS_DATABASE_NAME}.active_calculations_v1 as
WITH calculations_by_day AS (
  SELECT
    c.calculation_id,
    calculation_type,
    calculation_version,
    cga.grid_area_code,
    -- Explode calculation period into daily intervals.
    -- The end date is exclusive, and the period is therefore subtracted by one day.
    -- The calculation period is in UTC, so we convert it to local time.
    explode(sequence(
      DATE(FROM_UTC_TIMESTAMP(calculation_period_start, 'Europe/Copenhagen')),
      DATE_SUB(DATE(FROM_UTC_TIMESTAMP(calculation_period_end, 'Europe/Copenhagen')), 1),
      interval 1 day
    )) AS from_date_local,
    -- All rows should represent a full day, so the to_date is the day after the from_date
    DATE_ADD(from_date_local, 1) AS to_date_local,
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
  -- The active_to_time is the active_from_time of the next calculation that has been calculated (with the same, calc. type, grid are and from date).
  LEAD(active_from_time) OVER (PARTITION BY calculation_type, grid_area_code, from_date_local ORDER BY calculation_version ASC) AS active_to_time
FROM calculations_by_day

