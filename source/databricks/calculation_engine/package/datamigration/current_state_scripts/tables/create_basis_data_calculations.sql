CREATE TABLE IF NOT EXISTS {BASIS_DATA_DATABASE_NAME}.calculations
(
    calculation_id STRING NOT NULL,
    calculation_type STRING NOT NULL,
    period_start TIMESTAMP NOT NULL,
    period_end TIMESTAMP NOT NULL,
    execution_time_start TIMESTAMP NOT NULL,
    created_time TIMESTAMP NOT NULL,
    created_by_user_id STRING NOT NULL,
    version BIGINT NOT NULL
    energy_results_resolution STRING NOT NULL,
)
USING DELTA
TBLPROPERTIES (delta.deletedFileRetentionDuration = 'interval 30 days')
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
{TEST}LOCATION '{CONTAINER_PATH}/{BASIS_DATA_FOLDER}/calculations'
GO

-- Constraints --

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.calculations
    DROP CONSTRAINT IF EXISTS calculation_id_chk
GO
ALTER TABLE {BASIS_DATA_DATABASE_NAME}.calculations
    ADD CONSTRAINT calculation_id_chk CHECK (LENGTH(calculation_id) = 36)
GO

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.calculations
    DROP CONSTRAINT IF EXISTS calculation_type_chk
GO
ALTER TABLE {BASIS_DATA_DATABASE_NAME}.calculations
    ADD CONSTRAINT calculation_type_chk CHECK (calculation_type IN ('BalanceFixing', 'Aggregation', 'WholesaleFixing', 'FirstCorrectionSettlement', 'SecondCorrectionSettlement', 'ThirdCorrectionSettlement'))
GO

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.calculations
    DROP CONSTRAINT IF EXISTS energy_results_resolution_chk
GO
ALTER TABLE {BASIS_DATA_DATABASE_NAME}.calculations
    ADD CONSTRAINT energy_results_resolution_chk CHECK (energy_results_resolution IN ('PT15M', 'PT1H'))
GO
