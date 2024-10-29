-- Delete from grid_loss_metering_points where the calculation is an aggregation
DELETE FROM {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.grid_loss_metering_points
WHERE calculation_id IN (
    SELECT calculation_id
    FROM {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations
    WHERE calculation_type = 'aggregation'
)
GO

-- Delete from metering_point_periods where the calculation is an aggregation
DELETE FROM {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.metering_point_periods
WHERE calculation_id IN (
    SELECT calculation_id
    FROM {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations
    WHERE calculation_type = 'aggregation'
)
GO

-- Delete from time_series_points where the calculation is an aggregation
DELETE FROM {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.time_series_points
WHERE calculation_id IN (
    SELECT calculation_id
    FROM {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations
    WHERE calculation_type = 'aggregation'
)
GO
