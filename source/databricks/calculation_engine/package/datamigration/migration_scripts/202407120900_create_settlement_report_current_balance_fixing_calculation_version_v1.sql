CREATE VIEW IF NOT EXISTS {CATALOG_NAME}.{WHOLESALE_SETTLEMENT_REPORTS_DATABASE_NAME}.current_balance_fixing_calculation_version_v1 as
SELECT MAX(version) as calculation_version
FROM {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.calculations
WHERE calculation_type = 'balance_fixing'
