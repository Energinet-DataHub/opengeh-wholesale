{TEST-ONLY}ALTER TABLE {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations_v1
{TEST-ONLY}ADD COLUMN calculation_version BIGINT
{TEST-ONLY}GO

{TEST-ONLY}ALTER TABLE {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations_v1
{TEST-ONLY}ALTER COLUMN calculation_version AFTER calculation_period_end