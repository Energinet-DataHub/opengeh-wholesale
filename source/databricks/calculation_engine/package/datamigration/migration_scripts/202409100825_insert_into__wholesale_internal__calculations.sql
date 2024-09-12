INSERT INTO {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations
(calculation_id, calculation_type, calculation_period_start, calculation_period_end, calculation_execution_time_start, calculation_succeeded_time, is_internal_calculation)
SELECT
  calculation_id,
  calculation_type,
  calculation_period_start,
  calculation_period_end,
  calculation_execution_time_start,
  calculation_succeeded_time,
  is_internal_calculation
FROM {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations_v1
ORDER BY calculation_execution_time_start;
