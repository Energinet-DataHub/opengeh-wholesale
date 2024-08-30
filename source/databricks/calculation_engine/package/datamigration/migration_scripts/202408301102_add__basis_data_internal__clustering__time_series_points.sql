-- Use liquid clustering. Liquid clustering requires Databricks runtime 13.3+, which is not used in the testsuite

{DATABRICKS-ONLY}ALTER TABLE {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.time_series_points
{DATABRICKS-ONLY}CLUSTER BY (calculation_id, metering_point_id, grid_area_code, observation_time);

-- Avoid problem with empty script while running in test
SELECT 1