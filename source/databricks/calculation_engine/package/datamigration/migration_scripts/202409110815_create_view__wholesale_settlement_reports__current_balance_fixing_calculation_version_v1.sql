DROP VIEW IF EXISTS {CATALOG_NAME}.{WHOLESALE_SETTLEMENT_REPORTS_DATABASE_NAME}.current_balance_fixing_calculation_version_v1
GO

CREATE VIEW {CATALOG_NAME}.{WHOLESALE_SETTLEMENT_REPORTS_DATABASE_NAME}.current_balance_fixing_calculation_version_v1 as
SELECT MAX(calculation_version) as calculation_version
FROM {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.succeeded_external_calculations_v1
WHERE calculation_type = 'balance_fixing'
