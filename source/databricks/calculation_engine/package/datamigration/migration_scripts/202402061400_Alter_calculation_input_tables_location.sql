ALTER TABLE {INPUT_DATABASE_NAME}.metering_point_periods
SET LOCATION '{CONTAINER_PATH}/{INPUT_FOLDER}/metering_point_periods'
GO

ALTER TABLE {INPUT_DATABASE_NAME}.time_series_points
SET LOCATION '{CONTAINER_PATH}/{INPUT_FOLDER}/time_series_points'
GO

ALTER TABLE {INPUT_DATABASE_NAME}.charge_link_periods
SET LOCATION '{CONTAINER_PATH}/{INPUT_FOLDER}/charge_link_periods'
GO

ALTER TABLE {INPUT_DATABASE_NAME}.charge_masterdata_periods
SET LOCATION '{CONTAINER_PATH}/{INPUT_FOLDER}/charge_masterdata_periods'
GO

ALTER TABLE {INPUT_DATABASE_NAME}.charge_price_points
SET LOCATION '{CONTAINER_PATH}/{INPUT_FOLDER}/charge_price_points'
GO

ALTER TABLE {INPUT_DATABASE_NAME}.grid_loss_metering_points
SET LOCATION '{CONTAINER_PATH}/{INPUT_FOLDER}/grid_loss_metering_points'
GO
