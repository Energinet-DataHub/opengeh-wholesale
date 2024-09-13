-- Use liquid clustering. Liquid clustering requires Databricks runtime 13.3+, which is not used in the testsuite

{DATABRICKS-ONLY}ALTER TABLE {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.metering_point_periods
{DATABRICKS-ONLY}CLUSTER BY (calculation_id, grid_area_code, metering_point_id, energy_supplier_id);

-- Avoid problem with empty script while running in test
{TEST-ONLY}SELECT 1;