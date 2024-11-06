CREATE TABLE IF NOT EXISTS {INPUT_DATABASE_NAME}.grid_loss_metering_points
(
    metering_point_id STRING NOT NULL
)
USING DELTA
TBLPROPERTIES (
    delta.deletedFileRetentionDuration = 'interval 30 days',
    delta.constraints.metering_point_id_chk = "LENGTH ( metering_point_id ) = 18"
)
GO
