ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
SET TBLPROPERTIES(
    delta.deletedFileRetentionDuration = "interval 30 days"
)

GO

ALTER TABLE {INPUT_DATABASE_NAME}.grid_loss_metering_points
SET TBLPROPERTIES(
    delta.deletedFileRetentionDuration = "interval 30 days"
)

GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
SET TBLPROPERTIES(
    delta.deletedFileRetentionDuration = "interval 30 days"
)

GO

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.metering_point_periods
SET TBLPROPERTIES(
    delta.deletedFileRetentionDuration = "interval 30 days"
)

GO

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.time_series_points
SET TBLPROPERTIES(
    delta.deletedFileRetentionDuration = "interval 30 days"
)

GO

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.charge_price_points
SET TBLPROPERTIES(
    delta.deletedFileRetentionDuration = "interval 30 days"
)

GO

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.charge_masterdata_periods
SET TBLPROPERTIES(
    delta.deletedFileRetentionDuration = "interval 30 days"
)

GO

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.charge_link_periods
SET TBLPROPERTIES(
    delta.deletedFileRetentionDuration = "interval 30 days"
)

GO

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.grid_loss_metering_points
SET TBLPROPERTIES(
    delta.deletedFileRetentionDuration = "interval 30 days"
)

GO

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.calculations
SET TBLPROPERTIES(
    delta.deletedFileRetentionDuration = "interval 30 days"
)

GO
