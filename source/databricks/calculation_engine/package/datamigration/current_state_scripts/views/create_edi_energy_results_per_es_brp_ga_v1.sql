CREATE VIEW IF NOT EXISTS {EDI_RESULTS_DATABASE_NAME}.energy_result_points_per_es_brp_ga_v1 AS
SELECT calculation_id,
       calculation_type,
       calculation_period_start,
       calculation_period_end,
       calculation_version,
       result_id,
       grid_area_code,
       energy_supplier_id,
       balance_responsible_party_id,
       metering_point_type,
       settlement_method,
       resolution,
       time,
       quantity,
       unit,
       quantity_qualities
FROM {OUTPUT_DATABASE_NAME}.succeeded_energy_results_v1
WHERE
    calculation_type in ('BalanceFixing', 'Aggregation')
    AND aggregation_level = 'es_brp_ga'
    AND time_series_type in ('production', 'non_profiled_consumption', 'flex_consumption')
