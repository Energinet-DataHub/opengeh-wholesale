-- Delete aggregations from the basis data tables

MERGE INTO {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.grid_loss_metering_points AS target
USING (
      SELECT calculation_id
      FROM {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations
      WHERE calculation_type = 'aggregation'
) AS source
ON target.calculation_id = source.calculation_id
WHEN MATCHED THEN DELETE
GO

MERGE INTO {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.metering_point_periods AS target
USING (
      SELECT calculation_id
      FROM {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations
      WHERE calculation_type = 'aggregation'
) AS source
ON target.calculation_id = source.calculation_id
WHEN MATCHED THEN DELETE
GO

MERGE INTO {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.time_series_points AS target
USING (
      SELECT calculation_id
      FROM {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations
      WHERE calculation_type = 'aggregation'
) AS source
ON target.calculation_id = source.calculation_id
WHEN MATCHED THEN DELETE
GO
