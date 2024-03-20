CREATE DATABASE IF NOT EXISTS {BASIS_DATA_DATABASE_NAME}
COMMENT 'Contains basis data from wholesale domain.'
GO

CREATE TABLE IF NOT EXISTS {BASIS_DATA_DATABASE_NAME}.metering_point_periods
(
    -- 36 characters UUID
    calculation_id STRING NOT NULL,
    -- Enum
    calculation_type STRING NOT NULL,

    metering_point_id STRING NOT NULL,
    metering_point_type STRING NOT NULL,
    calculation_type STRING NOT NULL,
    settlement_method STRING,
    grid_area_code STRING NOT NULL,
    resolution STRING NOT NULL,
    from_grid_area STRING,
    to_grid_area STRING,
    parent_metering_point_id STRING,
    energy_supplier_id STRING NOT NULL
    balance_responsible_id STRING NOT
    from_date TIMESTAMP NOT NULL,
    to_date TIMESTAMP NOT NULL
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

    metering_point_id STRING NOT NULL,
    observation_time TIMESTAMP NOT NULL,
    quality STRING NOT NULL,
    quantity DECIMAL(18, 3)
)
USING DELTA
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
{TEST}LOCATION '{CONTAINER_PATH}/{BASIS_DATA_FOLDER}/time_series'
GO
