-- Use liquid clustering. Liquid clustering requires Databricks runtime 13.3+, which is not used in the testsuite

{DATABRICKS-ONLY}ALTER TABLE {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations
{DATABRICKS-ONLY}CLUSTER BY (calculation_id, calculation_type);

-- Avoid problem with empty script while running in test
{TEST-ONLY}SELECT 1;