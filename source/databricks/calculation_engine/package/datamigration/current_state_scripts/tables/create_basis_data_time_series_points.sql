CREATE TABLE IF NOT EXISTS {BASIS_DATA_DATABASE_NAME}.time_series_points
(
    calculation_id STRING NOT NULL,
    metering_point_id STRING NOT NULL,
    quantity DECIMAL(18, 3) NOT NULL,
    quality STRING NOT NULL,
    observation_time TIMESTAMP NOT NULL
)
USING DELTA
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
{TEST}LOCATION '{CONTAINER_PATH}/{BASIS_DATA_FOLDER}/time_series'
GO

-- Constraints --

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
