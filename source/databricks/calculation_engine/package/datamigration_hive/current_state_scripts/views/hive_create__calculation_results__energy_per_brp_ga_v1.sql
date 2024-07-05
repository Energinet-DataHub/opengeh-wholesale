CREATE VIEW IF NOT EXISTS {CALCULATION_RESULTS_DATABASE_NAME}.energy_per_brp_ga_v1 AS
SELECT calculation_id,
       calculation_type,
       calculation_period_start,
       calculation_period_end,
       calculation_version,
       result_id,
       grid_area_code,
       balance_responsible_party_id,
       metering_point_type,
       settlement_method,
       resolution,
       time,
       quantity,
       unit as quantity_unit,
       quantity_qualities
FROM {HIVE_OUTPUT_DATABASE_NAME}.succeeded_energy_results_v1
WHERE
    calculation_type in ('balance_fixing', 'aggregation')
    AND aggregation_level = 'brp_ga'
    AND time_series_type in ('production', 'non_profiled_consumption', 'flex_consumption')
