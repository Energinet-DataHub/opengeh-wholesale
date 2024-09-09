INSERT INTO {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations_v1
(calculation_id, calculation_type, calculation_period_start, calculation_period_end, calculation_execution_time_start, calculation_succeeded_time)
SELECT
  calculation_id,
  calculation_type,
  calculation_period_start,
  calculation_period_end,
  calculation_execution_time_start,
  calculation_succeeded_time
FROM {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations
ORDER BY calculation_execution_time_start;
