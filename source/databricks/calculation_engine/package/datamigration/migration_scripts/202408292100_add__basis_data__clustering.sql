ALTER TABLE {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations
CLUSTER BY (calculation_id, calculation_type)
GO

ALTER TABLE {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.time_series_points
CLUSTER BY (calculation_id, metering_point_id, grid_area_code, observation_time)
GO

ALTER TABLE {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.metering_point_periods
CLUSTER BY (calculation_id, metering_point_id)
GO
