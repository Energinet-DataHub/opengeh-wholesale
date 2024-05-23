CREATE TABLE IF NOT EXISTS {OUTPUT_DATABASE_NAME}.monthly_amounts
(
    -- 36 characters UUID
    calculation_id STRING NOT NULL,
    -- Enum
    calculation_type STRING NOT NULL,
    calculation_execution_time_start TIMESTAMP NOT NULL,

    -- 36 characters UUID
    calculation_result_id STRING NOT NULL,

    grid_area_code STRING NOT NULL,
    energy_supplier_id STRING,
    time TIMESTAMP NOT NULL,
    amount DECIMAL(18, 6),
    charge_owner_id STRING,
    charge_code STRING,
    charge_type STRING
)
USING DELTA
TBLPROPERTIES (
    delta.deletedFileRetentionDuration = 'interval 30 days',
    delta.constraints.calculation_id_chk = "LENGTH ( calculation_id ) = 36",
    delta.constraints.calculation_type_chk = "calculation_type IN ( 'WholesaleFixing', 'FirstCorrectionSettlement', 'SecondCorrectionSettlement', 'ThirdCorrectionSettlement' )",
    delta.constraints.calculation_result_id_chk = "LENGTH ( calculation_result_id ) = 36",
    delta.constraints.grid_area_code_chk = "LENGTH ( grid_area_code ) = 3",
    delta.constraints.energy_supplier_id_chk = "LENGTH ( energy_supplier_id ) = 13 OR LENGTH ( energy_supplier_id ) = 16",
    delta.constraints.charge_owner_id_chk = "charge_owner_id IS NULL OR LENGTH ( charge_owner_id ) = 13 OR LENGTH ( charge_owner_id ) = 16",
    delta.constraints.charge_type_chk = "charge_type IN ( 'subscription' , 'fee' , 'tariff' )"
)
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
{TEST}LOCATION '{CONTAINER_PATH}/{OUTPUT_FOLDER}/monthly_amounts'
