-- Use liquid clustering. Liquid clustering requires Databricks runtime 13.3+, which is not used in the testsuite

{DATABRICKS-ONLY}OPTIMIZE {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.time_series_points;

-- Avoid problem with empty script while running in test
{TEST-ONLY}SELECT 1;