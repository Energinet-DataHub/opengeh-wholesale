DROP VIEW IF EXISTS {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_DATABASE_NAME}.grid_loss_metering_points_v1
GO

CREATE VIEW IF NOT EXISTS {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_DATABASE_NAME}.grid_loss_metering_points_v1 AS
SELECT c.calculation_id,
       c.calculation_type,
       c.calculation_version,
       g.metering_point_id
FROM {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.grid_loss_metering_points as g
INNER JOIN {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.succeeded_external_calculations_v1 AS c ON c.calculation_id = g.calculation_id 
