ALTER TABLE {BASIS_DATA_DATABASE_NAME}.calculations
    ADD COLUMN energy_results_resolution STRING NOT NULL DEFAULT 'PT15M'
ADD COLUMN resolution STRING NOT NULL DEFAULT 'PT15M'
GO

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.calculations
    DROP CONSTRAINT IF EXISTS energy_results_resolution_chk
GO
ALTER TABLE {BASIS_DATA_DATABASE_NAME}.calculations
    ADD CONSTRAINT energy_results_resolution_chk CHECK (energy_results_resolution IN ('PT15M', 'PT1H'))
GO
