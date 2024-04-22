ALTER TABLE {BASIS_DATA_DATABASE_NAME}.time_series_points
    DROP CONSTRAINT IF EXISTS calculation_id_chk
GO
ALTER TABLE {BASIS_DATA_DATABASE_NAME}.time_series_points
    ADD CONSTRAINT calculation_id_chk CHECK (LENGTH(calculation_id) = 36)
GO

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.time_series_points
    DROP CONSTRAINT IF EXISTS metering_point_id_chk
GO
ALTER TABLE {BASIS_DATA_DATABASE_NAME}.time_series_points
    ADD CONSTRAINT metering_point_id_chk CHECK  (LENGTH(metering_point_id) = 18)
GO

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.time_series_points
    DROP CONSTRAINT IF EXISTS quality_chk
GO
ALTER TABLE {BASIS_DATA_DATABASE_NAME}.time_series_points
    ADD CONSTRAINT quality_chk CHECK (quality IN (
        'missing',
        'estimated',
        'measured',
        'calculated'))
GO
