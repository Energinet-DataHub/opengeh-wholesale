CREATE TABLE IF NOT EXISTS {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.grid_loss_metering_points
(
    metering_point_id STRING NOT NULL
)
USING DELTA
TBLPROPERTIES (
    delta.deletedFileRetentionDuration = 'interval 30 days',
    delta.constraints.metering_point_id_chk = "LENGTH ( metering_point_id ) = 18"
)
GO

CREATE TABLE IF NOT EXISTS {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.grid_loss_metering_points
(
    calculation_id STRING NOT NULL,
    metering_point_id STRING NOT NULL
)
USING DELTA
TBLPROPERTIES (
    delta.deletedFileRetentionDuration = 'interval 30 days',
    delta.constraints.calculation_id_chk = "LENGTH ( calculation_id ) = 36",
    delta.constraints.metering_point_id_chk = "LENGTH ( metering_point_id ) = 18"
)
