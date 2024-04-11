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
    energy_supplier_id STRING NULL,
    time TIMESTAMP NOT NULL,
    amount DECIMAL(18, 6),
    charge_owner_id STRING
)
USING DELTA
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used. 
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.    
{TEST}LOCATION '{CONTAINER_PATH}/{OUTPUT_FOLDER}/total_monthly_amounts'
