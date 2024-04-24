CREATE TABLE IF NOT EXISTS {BASIS_DATA_DATABASE_NAME}.charge_price_points
(
    calculation_id STRING NOT NULL,
    charge_key STRING NOT NULL,
    charge_code STRING NOT NULL,
    charge_type STRING NOT NULL,
    charge_owner_id STRING NOT NULL,
    charge_price DECIMAL(18, 6) NOT NULL,
    charge_time TIMESTAMP NOT NULL
)
USING DELTA
TBLPROPERTIES (
    delta.deletedFileRetentionDuration = 'interval 30 days',
    delta.constraints.calculation_id_chk = 'LENGTH(calculation_id) = 36',
    delta.constraints.charge_type_chk = "charge_type IN ('subscription', 'fee', 'tariff')"
    delta.constraints.charge_owner_id_chk = 'LENGTH(charge_owner_id) = 13 OR LENGTH(charge_owner_id) = 16'
)
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
{TEST}LOCATION '{CONTAINER_PATH}/{BASIS_DATA_FOLDER}/charge_price_points'
GO
