-- Make columns NOT NULL

DROP VIEW IF EXISTS {SETTLEMENT_REPORT_DATABASE_NAME}.current_balance_fixing_calculation_version_v1
GO

CREATE VIEW IF NOT EXISTS {SETTLEMENT_REPORT_DATABASE_NAME}.current_balance_fixing_calculation_version_v1 as
SELECT COALESCE(MAX(version), -1) as version -- Hack to make column NOT NULL. Defaults to -1.
FROM {BASIS_DATA_DATABASE_NAME}.calculations
WHERE calculation_type = 'balance_fixing'
