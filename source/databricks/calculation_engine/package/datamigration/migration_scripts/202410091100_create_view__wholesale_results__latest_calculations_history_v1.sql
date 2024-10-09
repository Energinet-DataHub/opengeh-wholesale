-- This view breaks down each calculation period into daily intervals for each of the calculation's grid areas.
-- This view only considers external calculations

CREATE OR REPLACE VIEW {CATALOG_NAME}.{WHOLESALE_SAP_DATABASE_NAME}.latest_calculations_v1 as
WITH calculations_by_day_and_grid_area_local_time AS (
    SELECT c.calculation_id, c.calculation_type, c.calculation_version, g.grid_area_code,
           explode(sequence(
               from_utc_timestamp(calculation_period_start, 'Europe/Copenhagen'),
               from_utc_timestamp(calculation_period_end, 'Europe/Copenhagen') - interval 1 day,
               interval 1 day
           )) AS start_of_day_local
    FROM {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.succeeded_external_calculations_v1 c
    JOIN {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculation_grid_areas g
    ON c.calculation_id = g.calculation_id
),
calculations_by_day_and_grid_area_utc AS (
    SELECT *,
           to_utc_timestamp(start_of_day_local, 'Europe/Copenhagen') AS start_of_day
    FROM calculations_by_day_and_grid_area_local_time
),
ranked_calculations AS (
    SELECT *,
           rank() OVER (
               PARTITION BY calculation_type, grid_area_code, start_of_day
               ORDER BY calculation_version DESC
           ) AS rank
    FROM calculations_by_day_and_grid_area_utc
)
SELECT calculation_id, calculation_type, calculation_version, grid_area_code, start_of_day
FROM ranked_calculations
WHERE rank = 1