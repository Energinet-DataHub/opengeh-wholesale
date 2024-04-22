CREATE TABLE IF NOT EXISTS {BASIS_DATA_DATABASE_NAME}.charge_link_periods
(
    calculation_id STRING NOT NULL,
    charge_key STRING NOT NULL,
    charge_code STRING NOT NULL,
    charge_type STRING NOT NULL,
    charge_owner_id STRING NOT NULL,
    metering_point_id STRING NOT NULL,
    quantity int NOT NULL,
    from_date TIMESTAMP NOT NULL,
    to_date TIMESTAMP
)
USING DELTA
TBLPROPERTIES (delta.deletedFileRetentionDuration = 'interval 30 days')
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
{TEST}LOCATION '{CONTAINER_PATH}/{BASIS_DATA_FOLDER}/charge_link_periods'
GO

-- Constraints --

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.charge_link_periods
    DROP CONSTRAINT IF EXISTS calculation_id_chk
GO
ALTER TABLE {BASIS_DATA_DATABASE_NAME}.charge_link_periods
    ADD CONSTRAINT calculation_id_chk CHECK (LENGTH(calculation_id) = 36)
GO

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.charge_link_periods
    DROP CONSTRAINT IF EXISTS charge_type_chk
GO
ALTER TABLE {BASIS_DATA_DATABASE_NAME}.charge_link_periods
    ADD CONSTRAINT charge_type_chk CHECK (charge_type IN ('subscription', 'fee', 'tariff'))
GO

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.charge_link_periods
    DROP CONSTRAINT IF EXISTS charge_owner_id_chk
GO
-- Length is 16 when EIC and 13 when GLN
ALTER TABLE {BASIS_DATA_DATABASE_NAME}.charge_link_periods
    ADD CONSTRAINT charge_owner_id_chk CHECK (LENGTH(charge_owner_id) = 13 OR LENGTH(charge_owner_id) = 16)
GO

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.charge_link_periods
    DROP CONSTRAINT IF EXISTS metering_point_id_chk
GO
ALTER TABLE {BASIS_DATA_DATABASE_NAME}.charge_link_periods
    ADD CONSTRAINT metering_point_id_chk CHECK  (LENGTH(metering_point_id) = 18)
GO
