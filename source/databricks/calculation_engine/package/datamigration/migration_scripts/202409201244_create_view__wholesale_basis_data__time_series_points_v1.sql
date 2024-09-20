DROP VIEW IF EXISTS {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_DATABASE_NAME}.time_series_points_v1
GO

CREATE VIEW {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_DATABASE_NAME}.time_series_points_v1 AS
SELECT c.calculation_id,
       c.calculation_type,
       c.calculation_version,
       t.metering_point_id,
       t.quantity,
       t.quality,
       t.observation_time
FROM {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.time_series_points as t
INNER JOIN {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.succeeded_external_calculations_v1 AS c ON c.calculation_id = t.calculation_id 
