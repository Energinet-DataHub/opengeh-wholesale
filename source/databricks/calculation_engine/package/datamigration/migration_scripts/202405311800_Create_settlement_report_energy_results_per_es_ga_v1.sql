CREATE VIEW {SETTLEMENT_REPORT_DATABASE_NAME}.energy_result_points_per_es_ga_v1 as
SELECT calculation_id,
       calculation_type,
       calculation_version,
       result_id,
       grid_area_code,
       metering_point_type,
       settlement_method,
       resolution,
       time,
       quantity,
       energy_supplier_id
FROM {OUTPUT_DATABASE_NAME}.succeeded_energy_results_v1
WHERE time_series_type IN ('production', 'non_profiled_consumption', 'flex_consumption', 'net_exchange_per_ga', 'total_consumption')
AND calculation_type IN ('BalanceFixing', 'WholesaleFixing', 'FirstCorrectionSettlement', 'SecondCorrectionSettlement', 'ThirdCorrectionSettlement')
AND aggregation_level = 'es_brp_ga'