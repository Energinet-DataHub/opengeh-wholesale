-- Hack to make sure that the script is not executed when running integration tests
CREATE
{TEST} EXTERNAL
TABLE if not exists {INPUT_DATABASE_NAME}.metering_point_periods
    USING DELTA
{TEST} LOCATION '{CONTAINER_PATH}/{INPUT_FOLDER}/metering_point_periods'
GO

CREATE
{TEST} EXTERNAL
TABLE if not exists {INPUT_DATABASE_NAME}.time_series_points
    USING DELTA
{TEST} LOCATION '{CONTAINER_PATH}/{INPUT_FOLDER}/time_series_points'
GO

CREATE
{TEST} EXTERNAL
TABLE if not exists {INPUT_DATABASE_NAME}.charge_link_periods
USING DELTA
{TEST} LOCATION '{CONTAINER_PATH}/{INPUT_FOLDER}/charge_link_periods'
GO

CREATE
{TEST} EXTERNAL
TABLE if not exists {INPUT_DATABASE_NAME}.charge_masterdata_periods
    USING DELTA
{TEST} LOCATION '{CONTAINER_PATH}/{INPUT_FOLDER}/charge_masterdata_periods'
GO

CREATE
{TEST} EXTERNAL
TABLE if not exists {INPUT_DATABASE_NAME}.charge_price_points
    USING DELTA
{TEST} LOCATION '{CONTAINER_PATH}/{INPUT_FOLDER}/charge_price_points'
GO
