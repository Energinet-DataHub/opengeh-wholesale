-- Hack to make sure that the script is not executed when running integration tests
DESCRIBE DATABASE {INPUT_DATABASE_NAME};
{TEST}CREATE EXTERNAL TABLE if not exists {INPUT_DATABASE_NAME}.metering_point_periods
{TEST}    USING DELTA LOCATION '{CONTAINER_PATH}/{INPUT_FOLDER}/metering_point_periods'
{TEST}GO
{TEST}
{TEST}CREATE EXTERNAL TABLE if not exists {INPUT_DATABASE_NAME}.time_series_points
{TEST}    USING DELTA LOCATION '{CONTAINER_PATH}/{INPUT_FOLDER}/time_series_points'
{TEST}GO
{TEST}
{TEST}CREATE EXTERNAL TABLE if not exists {INPUT_DATABASE_NAME}.charge_link_periods
{TEST}    USING DELTA LOCATION '{CONTAINER_PATH}/{INPUT_FOLDER}/charge_link_periods'
{TEST}GO
{TEST}
{TEST}CREATE EXTERNAL TABLE if not exists {INPUT_DATABASE_NAME}.charge_masterdata_periods
{TEST}    USING DELTA LOCATION '{CONTAINER_PATH}/{INPUT_FOLDER}/charge_masterdata_periods'
{TEST}GO
{TEST}
{TEST}CREATE EXTERNAL TABLE if not exists {INPUT_DATABASE_NAME}.charge_price_points
{TEST}    USING DELTA LOCATION '{CONTAINER_PATH}/{INPUT_FOLDER}/charge_price_points'
{TEST}GO
