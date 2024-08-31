-- Use liquid clustering. Liquid clustering requires Databricks runtime 13.3+, which is not used in the testsuite

{DATABRICKS-ONLY}OPTIMIZE {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations;

-- Avoid problem with empty script while running in test
{TEST-ONLY}SELECT 1;