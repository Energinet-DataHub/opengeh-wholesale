ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    DROP CONSTRAINT IF EXISTS calculation_type_chk
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    ADD CONSTRAINT calculation_type_chk CHECK (calculation_type IN ('BalanceFixing', 'Aggregation', 'WholesaleFixing', 'FirstCorrectionSettlement', 'SecondCorrectionSettlement', 'ThirdCorrectionSettlement'))
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    DROP CONSTRAINT IF EXISTS time_series_type_chk
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    ADD CONSTRAINT time_series_type_chk
        CHECK (time_series_type IN (
            'production',
            'non_profiled_consumption',
            'net_exchange_per_neighboring_ga',
            'net_exchange_per_ga',
            'flex_consumption',
            'grid_loss',
            'negative_grid_loss',
            'positive_grid_loss',
            'total_consumption',
            'temp_flex_consumption',
            'temp_production'))
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    DROP CONSTRAINT IF EXISTS grid_area_chk
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    ADD CONSTRAINT grid_area_chk CHECK (LENGTH(grid_area) = 3)
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    DROP CONSTRAINT IF EXISTS out_grid_area_chk
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    ADD CONSTRAINT out_grid_area_chk CHECK (out_grid_area IS NULL OR LENGTH(out_grid_area) = 3)
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    DROP CONSTRAINT IF EXISTS quantity_qualities_chk
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    ADD CONSTRAINT quantity_qualities_chk
    CHECK (array_size(array_except(quantity_qualities, array('missing', 'calculated', 'measured', 'estimated'))) = 0
           AND array_size(quantity_qualities) > 0)
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    DROP CONSTRAINT IF EXISTS aggregation_level_chk
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    ADD CONSTRAINT aggregation_level_chk CHECK (aggregation_level IN ('total_ga', 'es_brp_ga', 'es_ga', 'brp_ga'))
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    DROP CONSTRAINT IF EXISTS energy_supplier_id_chk
GO
-- Length is 16 when EIC and 13 when GLN
ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    ADD CONSTRAINT energy_supplier_id_chk CHECK (energy_supplier_id IS NULL OR LENGTH(energy_supplier_id) = 13 OR LENGTH(energy_supplier_id) = 16)
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    DROP CONSTRAINT IF EXISTS balance_responsible_id_chk
GO
-- Length is 16 when EIC and 13 when GLN
ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    ADD CONSTRAINT balance_responsible_id_chk CHECK (balance_responsible_id IS NULL OR LENGTH(balance_responsible_id) = 13 OR LENGTH(balance_responsible_id) = 16)
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    DROP CONSTRAINT IF EXISTS calculation_id_chk
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    ADD CONSTRAINT calculation_id_chk CHECK (LENGTH(calculation_id) = 36)
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    DROP CONSTRAINT IF EXISTS calculation_result_id_chk
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    ADD CONSTRAINT calculation_result_id_chk CHECK (LENGTH(calculation_result_id) = 36)
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    DROP CONSTRAINT IF EXISTS metering_point_id_chk
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    ADD CONSTRAINT metering_point_id_chk CHECK  (metering_point_id IS NULL OR LENGTH(metering_point_id) = 18)
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    DROP CONSTRAINT IF EXISTS metering_point_id_conditional_chk
GO
--If time_series_type is 'negative_grid_loss' or 'positive_grid_loss', then metering_point_id must not be null.
--If time_series_type is neither 'negative_grid_loss' nor 'positive_grid_loss', then metering_point_id must be null.
ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    ADD CONSTRAINT metering_point_id_conditional_chk
    CHECK (
        (time_series_type IN ('negative_grid_loss', 'positive_grid_loss') AND metering_point_id IS NOT NULL)
        OR
        (time_series_type NOT IN ('negative_grid_loss', 'positive_grid_loss') AND metering_point_id IS NULL)
    )
GO
