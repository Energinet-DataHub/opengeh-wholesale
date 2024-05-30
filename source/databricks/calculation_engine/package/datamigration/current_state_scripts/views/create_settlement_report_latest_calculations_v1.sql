CREATE VIEW IF NOT EXISTS {SETTLEMENT_REPORT_DATABASE_NAME}.latest_calculations_v1 as
SELECT calculation_type,
       MAX(version) as version
FROM {BASIS_DATA_DATABASE_NAME}.calculations
GROUP BY calculation_type
WHERE calculation_type IN ('BalanceFixing', 'WholesaleFixing', 'FirstCorrectionSettlement', 'SecondCorrectionSettlement', 'ThirdCorrectionSettlement')
