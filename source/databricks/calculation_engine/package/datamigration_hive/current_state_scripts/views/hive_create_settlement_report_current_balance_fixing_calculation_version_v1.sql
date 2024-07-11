CREATE VIEW IF NOT EXISTS {HIVE_SETTLEMENT_REPORT_DATABASE_NAME}.current_balance_fixing_calculation_version_v1 as
SELECT MAX(version) as calculation_version
FROM {HIVE_BASIS_DATA_DATABASE_NAME}.calculations
WHERE calculation_type = 'balance_fixing'
