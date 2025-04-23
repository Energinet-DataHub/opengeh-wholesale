-- Script used to correct UTC times in the calculations table.
-- These were left by the previous migration script, which has also been adjusted to avoid this in the future. 

UPDATE ctl_shres_p_we_001.wholesale_internal.calculations 
SET 
  calculation_period_start = CASE WHEN HOUR(calculation_period_start) = 0 THEN FROM_UTC_TIMESTAMP(calculation_period_start, 'Europe/Copenhagen') ELSE calculation_period_start END,
  calculation_period_end = CASE WHEN HOUR(calculation_period_end) = 0 THEN FROM_UTC_TIMESTAMP(calculation_period_end, 'Europe/Copenhagen') ELSE calculation_period_end END;