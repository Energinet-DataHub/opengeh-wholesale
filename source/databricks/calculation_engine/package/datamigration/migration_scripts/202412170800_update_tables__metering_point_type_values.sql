UPDATE {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.time_series_points
SET metering_point_type = 'capacity_settlement'
WHERE metering_point_type = 'effect_settlement';
GO

UPDATE {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.amounts_per_charge
SET metering_point_type = 'capacity_settlement'
WHERE metering_point_type = 'effect_settlement';
GO

UPDATE {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.metering_point_periods
SET metering_point_type = 'capacity_settlement'
WHERE metering_point_type = 'effect_settlement';
GO
