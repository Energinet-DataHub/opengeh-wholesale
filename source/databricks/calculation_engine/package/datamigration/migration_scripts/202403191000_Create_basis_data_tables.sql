CREATE TABLE IF NOT EXISTS {BASIS_DATA_DATABASE_NAME}.metering_point_periods
(
    -- 36 characters UUID
    calculation_id STRING NOT NULL,
    -- Enum
    calculation_type STRING NOT NULL,
    calculation_execution_time_start TIMESTAMP NOT NULL,

    metering_point_id STRING NOT NULL,
    valid_from TIMESTAMP NOT NULL,
    valid_to TIMESTAMP NOT NULL,
    grid_area STRING NOT NULL,
    to_grid_area STRING,
    from_grid_area STRING,
    metering_point_type STRING NOT NULL,
    settlement_method STRING,
    energy_supplier_id STRING NOT NULL
)
USING DELTA
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used. 
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.    
{TEST}LOCATION '{CONTAINER_PATH}/{BASIS_DATA_FOLDER}/metering_point_periods'
GO

CREATE TABLE IF NOT EXISTS {BASIS_DATA_DATABASE_NAME}.time_series
(
    -- 36 characters UUID
    calculation_id STRING NOT NULL,
    -- Enum
    calculation_type STRING NOT NULL,
    calculation_execution_time_start TIMESTAMP NOT NULL,

    grid_area STRING NOT NULL,
    metering_point_id STRING NOT NULL,
    metering_point_type STRING NOT NULL,
    start_datetime TIMESTAMP NOT NULL,
    energy_supplier_id STRING NOT NULL,
    quantity ARRAY<DECIMAL(18, 3)>,
    resolution STRING NOT NULL
)
USING DELTA
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
{TEST}LOCATION '{CONTAINER_PATH}/{BASIS_DATA_FOLDER}/time_series'
GO
