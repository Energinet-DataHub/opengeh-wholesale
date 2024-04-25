CREATE TABLE IF NOT EXISTS {INPUT_DATABASE_NAME}.grid_loss_metering_points
(
    metering_point_id STRING NOT NULL
)
USING DELTA
TBLPROPERTIES (
    delta.deletedFileRetentionDuration = 'interval 30 days',
    delta.constraints.metering_point_id_chk = "LENGTH ( metering_point_id ) = 18"
)
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
{TEST}LOCATION '{CONTAINER_PATH}/{INPUT_FOLDER}/grid_loss_metering_points'
GO
