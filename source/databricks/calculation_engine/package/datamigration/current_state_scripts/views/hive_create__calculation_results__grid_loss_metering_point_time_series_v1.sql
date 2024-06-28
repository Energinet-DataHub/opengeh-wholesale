CREATE VIEW IF NOT EXISTS {CALCULATION_RESULTS_DATABASE_NAME}.grid_loss_metering_point_time_series_v1 AS
SELECT calculation_id,
       calculation_type,
       calculation_period_start,
       calculation_period_end,
       calculation_version,
       metering_point_id,
       metering_point_type,
       resolution,
       unit as quantity_unit,
       time,
       quantity
FROM {HIVE_OUTPUT_DATABASE_NAME}.succeeded_energy_results_v1
WHERE
    time_series_type in ('negative_grid_loss', 'positive_grid_loss')
