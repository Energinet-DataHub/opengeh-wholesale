{DATABRICKS-ONLY}DELETE FROM {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.time_series_points
{DATABRICKS-ONLY}WHERE (metering_point_type IS NULL
{DATABRICKS-ONLY}OR metering_point_type NOT IN ('production', 'consumption', 'exchange'))
{DATABRICKS-ONLY}AND calculation_id IN (
{DATABRICKS-ONLY}    SELECT calculation_id
{DATABRICKS-ONLY}    FROM {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations
{DATABRICKS-ONLY}    WHERE calculation_type IN ('balance_fixing', 'aggregation')
{DATABRICKS-ONLY})

{TEST-ONLY}SELECT 1;
