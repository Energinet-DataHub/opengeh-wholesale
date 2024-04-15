CREATE TABLE IF NOT EXISTS {OUTPUT_DATABASE_NAME}.total_monthly_amounts
(
    -- 36 characters UUID
    calculation_id STRING NOT NULL,
    -- Enum
    calculation_type STRING NOT NULL,
    calculation_execution_time_start TIMESTAMP NOT NULL,

    -- 36 characters UUID
    calculation_result_id STRING NOT NULL,

    grid_area STRING NOT NULL,
    energy_supplier_id STRING,
    time TIMESTAMP NOT NULL,
    amount DECIMAL(18, 6),
    charge_owner_id STRING
)
USING DELTA
TBLPROPERTIES (delta.deletedFileRetentionDuration = 'interval 30 days')
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
{TEST}LOCATION '{CONTAINER_PATH}/{OUTPUT_FOLDER}/total_monthly_amounts'

GO

-- Constraints --
ALTER TABLE {OUTPUT_DATABASE_NAME}.total_monthly_amounts
    ADD CONSTRAINT calculation_id_chk CHECK (LENGTH(calculation_id) = 36)
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.total_monthly_amounts
    ADD CONSTRAINT calculation_type_chk CHECK (calculation_type IN ('WholesaleFixing', 'FirstCorrectionSettlement', 'SecondCorrectionSettlement', 'ThirdCorrectionSettlement'))
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.total_monthly_amounts
    ADD CONSTRAINT calculation_result_id_chk CHECK (LENGTH(calculation_result_id) = 36)
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.total_monthly_amounts
    ADD CONSTRAINT grid_area_chk CHECK (LENGTH(grid_area) = 3)
GO

-- Length is 16 when EIC and 13 when GLN
ALTER TABLE {OUTPUT_DATABASE_NAME}.total_monthly_amounts
    ADD CONSTRAINT energy_supplier_id_chk CHECK (energy_supplier_id IS NULL OR LENGTH(energy_supplier_id) = 13 OR LENGTH(energy_supplier_id) = 16)
GO

-- Length is 16 when EIC and 13 when GLN
ALTER TABLE {OUTPUT_DATABASE_NAME}.total_monthly_amounts
    ADD CONSTRAINT charge_owner_id_chk CHECK (charge_owner_id IS NULL OR LENGTH(charge_owner_id) = 13 OR LENGTH(charge_owner_id) = 16)
GO
