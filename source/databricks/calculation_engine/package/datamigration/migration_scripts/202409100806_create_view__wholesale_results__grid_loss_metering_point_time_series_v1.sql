DROP VIEW IF EXISTS {CATALOG_NAME}.{WHOLESALE_RESULTS_DATABASE_NAME}.grid_loss_metering_point_time_series_v1
GO

CREATE VIEW IF NOT EXISTS {CATALOG_NAME}.{WHOLESALE_RESULTS_DATABASE_NAME}.grid_loss_metering_point_time_series_v1 AS
SELECT c.calculation_id,
       calculation_type,
       calculation_period_start,
       calculation_period_end,
       calculation_version,
       metering_point_id,
       metering_point_type,
       resolution,
       'kWh' as quantity_unit,
       time,
       quantity
FROM {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.grid_loss_metering_point_time_series AS e
INNER JOIN {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.succeeded_external_calculations_v1 AS c ON c.calculation_id = e.calculation_id
