DROP VIEW IF EXISTS {SETTLEMENT_REPORT_DATABASE_NAME}.current_calculation_type_versions_v1
GO

CREATE VIEW IF NOT EXISTS {SETTLEMENT_REPORT_DATABASE_NAME}.current_balance_fixing_calculation_version_v1 as
SELECT MAX(version) as calculation_version
FROM {HIVE_BASIS_DATA_DATABASE_NAME}.calculations
WHERE calculation_type = 'BalanceFixing'
