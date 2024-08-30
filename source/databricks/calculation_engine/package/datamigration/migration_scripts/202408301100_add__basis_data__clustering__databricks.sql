-- Use liquid clustering. Liquid clustering requires Databricks runtime 13.3+, which is not used in the testsuite

{DATABRICKS-ONLY}ALTER TABLE {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations;
{DATABRICKS-ONLY}CLUSTER BY (calculation_id, calculation_type);

{DATABRICKS-ONLY}OPTIMIZE {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations;

{DATABRICKS-ONLY}ALTER TABLE {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.time_series_points;
{DATABRICKS-ONLY}CLUSTER BY (calculation_id, metering_point_id, grid_area_code, observation_time);

{DATABRICKS-ONLY}OPTIMIZE {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.time_series_points;

{DATABRICKS-ONLY}ALTER TABLE {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.metering_point_periods;
{DATABRICKS-ONLY}CLUSTER BY (calculation_id, metering_point_id);

{DATABRICKS-ONLY}OPTIMIZE {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.metering_point_periods;

-- Avoid problem with empty script while running in test
SELECT 1;
