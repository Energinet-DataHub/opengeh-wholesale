CREATE DATABASE IF NOT EXISTS {BASIS_DATA_DATABASE_NAME}
COMMENT 'Contains basis data from wholesale subsystem.'
GO

CREATE TABLE IF NOT EXISTS {BASIS_DATA_DATABASE_NAME}.metering_point_periods
(
    calculation_id STRING NOT NULL,
    metering_point_id STRING NOT NULL,
    metering_point_type STRING NOT NULL,
    settlement_method STRING,
    grid_area_code STRING NOT NULL,
    resolution STRING NOT NULL,
    from_grid_area_code STRING,
    to_grid_area_code STRING,
    parent_metering_point_id STRING,
    energy_supplier_id STRING,
    balance_responsible_id STRING,
    from_date TIMESTAMP NOT NULL,
    to_date TIMESTAMP
)
USING DELTA
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
{TEST}LOCATION '{CONTAINER_PATH}/{BASIS_DATA_FOLDER}/metering_point_periods'
GO

CREATE TABLE IF NOT EXISTS {BASIS_DATA_DATABASE_NAME}.time_series_points
(
    calculation_id STRING NOT NULL,
    metering_point_id STRING NOT NULL,
    quantity DECIMAL(18, 6),
    quality STRING NOT NULL,
    observation_time TIMESTAMP NOT NULL
)
USING DELTA
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
{TEST}LOCATION '{CONTAINER_PATH}/{BASIS_DATA_FOLDER}/time_series'
GO

CREATE TABLE IF NOT EXISTS {BASIS_DATA_DATABASE_NAME}.charge_price_points
(
    calculation_id STRING NOT NULL,
    charge_code STRING NOT NULL,
    charge_type STRING NOT NULL,
    charge_owner_id STRING NOT NULL,
    charge_price DECIMAL(18, 6) NOT NULL,
    charge_time TIMESTAMP NOT NULL
)
USING DELTA
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
{TEST}LOCATION '{CONTAINER_PATH}/{BASIS_DATA_FOLDER}/charge_price_points'
GO

CREATE TABLE IF NOT EXISTS {BASIS_DATA_DATABASE_NAME}.charge_masterdata_periods
(
    calculation_id STRING NOT NULL,
    charge_code STRING NOT NULL,
    charge_type STRING NOT NULL,
    charge_owner_id STRING NOT NULL,
    resolution STRING NOT NULL,
    is_tax BOOLEAN NOT NULL,
    from_date TIMESTAMP NOT NULL,
    to_date TIMESTAMP
)
USING DELTA
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
{TEST}LOCATION '{CONTAINER_PATH}/{BASIS_DATA_FOLDER}/charge_masterdata_periods'
GO

CREATE TABLE IF NOT EXISTS {BASIS_DATA_DATABASE_NAME}.charge_link_periods
(
    calculation_id STRING NOT NULL,
    charge_code STRING NOT NULL,
    charge_type STRING NOT NULL,
    charge_owner_id STRING NOT NULL,
    metering_point_id STRING NOT NULL,
    quantity int NOT NULL,
    from_date TIMESTAMP NOT NULL,
    to_date TIMESTAMP
)
USING DELTA
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
{TEST}LOCATION '{CONTAINER_PATH}/{BASIS_DATA_FOLDER}/charge_link_periods'
GO

CREATE TABLE IF NOT EXISTS {BASIS_DATA_DATABASE_NAME}.grid_loss_metering_points
(
    calculation_id STRING NOT NULL,
    metering_point_id STRING NOT NULL
)
USING DELTA
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
{TEST}LOCATION '{CONTAINER_PATH}/{BASIS_DATA_FOLDER}/grid_loss_metering_points'
GO

CREATE TABLE IF NOT EXISTS {BASIS_DATA_DATABASE_NAME}.calculations
(
    calculation_id STRING NOT NULL,
    calculation_type STRING NOT NULL,
    period_start TIMESTAMP NOT NULL,
    period_end TIMESTAMP NOT NULL,
    execution_time_start TIMESTAMP NOT NULL,
    created_time TIMESTAMP NOT NULL,
    created_by_user_id STRING NOT NULL,
    version LONG NOT NULL
)
USING DELTA
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
{TEST}LOCATION '{CONTAINER_PATH}/{BASIS_DATA_FOLDER}/calculations'
GO
