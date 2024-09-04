-- Use liquid clustering. Liquid clustering requires Databricks runtime 13.3+, which is not used in the testsuite

{DATABRICKS-ONLY}ALTER TABLE {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.metering_point_periods
{DATABRICKS-ONLY}CLUSTER BY (calculation_id, metering_point_id, grid_area_code, energy_supplier_id, from_date, to_date);

-- Avoid problem with empty script while running in test
{TEST-ONLY}SELECT 1;
