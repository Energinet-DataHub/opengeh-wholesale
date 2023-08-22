ALTER TABLE {OUTPUT_DATABASE_NAME}.result
    DROP CONSTRAINT IF EXISTS batch_process_type_chk
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.result
    DROP CONSTRAINT IF EXISTS time_series_type_chk
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.result
    DROP CONSTRAINT IF EXISTS grid_area_chk
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.result
    DROP CONSTRAINT IF EXISTS out_grid_area_chk
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.result
    DROP CONSTRAINT IF EXISTS quantity_quality_chk
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.result
    DROP CONSTRAINT IF EXISTS aggregation_level_chk
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.result
    ADD CONSTRAINT batch_process_type_chk CHECK (batch_process_type IN ('BalanceFixing', 'Aggregation'))
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.result
    ADD CONSTRAINT time_series_type_chk
        CHECK (time_series_type IN (
            'production',
            'non_profiled_consumption',
            'net_exchange_per_neighboring_ga',
            'net_exchange_per_ga',
            'flex_consumption',
            'grid_loss',
            'negative_grid_loss',
            'positive_grid_loss'))
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.result
    ADD CONSTRAINT grid_area_chk CHECK (LENGTH(grid_area) = 3)
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.result
    ADD CONSTRAINT out_grid_area_chk CHECK (out_grid_area IS NULL OR LENGTH(out_grid_area) = 3)
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.result
    ADD CONSTRAINT quantity_quality_chk CHECK (quantity_quality IN ('missing', 'estimated', 'measured', 'calculated', 'incomplete'))
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.result
    ADD CONSTRAINT aggregation_level_chk CHECK (aggregation_level IN ('total_ga', 'es_brp_ga', 'es_ga', 'brp_ga'))
