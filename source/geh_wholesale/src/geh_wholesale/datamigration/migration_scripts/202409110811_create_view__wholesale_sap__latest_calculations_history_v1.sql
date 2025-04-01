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
    -- Explode calculation period into daily intervals.
    -- The end date is exclusive, and the period is therefore subtracted by one day.
    -- The calculation period is in UTC, so we convert it to local time.
    explode(sequence(
      DATE(FROM_UTC_TIMESTAMP(calculation_period_start, 'Europe/Copenhagen')),
      DATE_SUB(DATE(FROM_UTC_TIMESTAMP(calculation_period_end, 'Europe/Copenhagen')), 1),
      interval 1 day
    )) AS from_date_local,
    -- All rows should represent a full day, so the to_date is the day after the from_date
    calculation_succeeded_time as latest_from_time
  FROM {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations c
  INNER JOIN {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculation_grid_areas cga ON c.calculation_id = cga.calculation_id
  WHERE calculation_succeeded_time IS NOT NULL
)
SELECT
  calculation_id,
  calculation_type,
  calculation_version,
  grid_area_code,
  TO_UTC_TIMESTAMP(from_date_local, 'Europe/Copenhagen') AS from_date,
  TO_UTC_TIMESTAMP(DATE_ADD(from_date_local, 1) , 'Europe/Copenhagen') AS to_date,
  latest_from_time,
  -- The latest_to_time is the latest_from_time of the next calculation that has been calculated (with the same, calc. type, grid are and from date).
  LEAD(latest_from_time) OVER (PARTITION BY calculation_type, grid_area_code, from_date_local ORDER BY calculation_version ASC) AS latest_to_time
FROM calculations_by_day

