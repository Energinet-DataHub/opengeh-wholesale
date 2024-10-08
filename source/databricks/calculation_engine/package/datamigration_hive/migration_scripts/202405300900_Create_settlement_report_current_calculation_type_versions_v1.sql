CREATE VIEW IF NOT EXISTS {HIVE_SETTLEMENT_REPORT_DATABASE_NAME}.current_calculation_type_versions_v1 as
SELECT calculation_type,
       MAX(version) as version
FROM {HIVE_BASIS_DATA_DATABASE_NAME}.calculations
WHERE calculation_type IN ('BalanceFixing', 'WholesaleFixing', 'FirstCorrectionSettlement', 'SecondCorrectionSettlement', 'ThirdCorrectionSettlement')
GROUP BY calculation_type
