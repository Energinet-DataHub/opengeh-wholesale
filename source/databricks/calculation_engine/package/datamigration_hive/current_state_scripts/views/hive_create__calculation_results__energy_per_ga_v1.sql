CREATE VIEW IF NOT EXISTS {CALCULATION_RESULTS_DATABASE_NAME}.energy_per_ga_v1 AS
SELECT calculation_id,
       calculation_type,
       calculation_period_start,
       calculation_period_end,
       calculation_version,
       result_id,
       grid_area_code,
       metering_point_type,
       settlement_method,
       resolution,
       time,
       quantity,
       unit as quantity_unit,
       quantity_qualities
FROM {HIVE_OUTPUT_DATABASE_NAME}.succeeded_energy_results_v1
WHERE
    -- Only include results that must be sent to the actors
    time_series_type in ('production', 'non_profiled_consumption', 'net_exchange_per_ga', 'flex_consumption', 'total_consumption')
    -- Only include results that are aggregated per grid area
    AND aggregation_level = 'grid_area'
