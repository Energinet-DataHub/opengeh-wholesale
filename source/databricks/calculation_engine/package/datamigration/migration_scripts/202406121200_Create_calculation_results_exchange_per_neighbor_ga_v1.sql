CREATE VIEW IF NOT EXISTS {CALCULATION_RESULTS_DATABASE_NAME}.exchange_per_neighbor_ga_v1 as
SELECT calculation_id,
       calculation_type,
       calculation_period_start,
       calculation_period_end,
       calculation_version,
       grid_area_code as in_grid_area_code,
       out_grid_area_code,
       resolution,
       time,
       quantity,
       unit as quantity_unit,
       quantity_qualities
FROM {OUTPUT_DATABASE_NAME}.succeeded_energy_results_v1
WHERE time_series_type in ('net_exchange_per_neighboring_ga')
