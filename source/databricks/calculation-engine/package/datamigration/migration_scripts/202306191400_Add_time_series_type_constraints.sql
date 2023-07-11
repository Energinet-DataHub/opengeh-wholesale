ALTER TABLE wholesale_output.result
    DROP CONSTRAINT time_series_type_chk
GO

ALTER TABLE wholesale_output.result
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
        'total_consumption'))
