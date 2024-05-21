ALTER TABLE {INPUT_DATABASE_NAME}.charge_masterdata_periods RENAME TO charge_price_information_periods
GO
ALTER TABLE {INPUT_DATABASE_NAME}.charge_price_information_periods SET LOCATION '{CONTAINER_PATH}/{INPUT_FOLDER}/charge_price_information_periods'
GO
ALTER TABLE {BASIS_DATA_DATABASE_NAME}.charge_masterdata_periods RENAME TO charge_price_information_periods
GO
ALTER TABLE {BASIS_DATA_DATABASE_NAME}.charge_price_information_periods SET LOCATION '{CONTAINER_PATH}/{BASIS_DATA_FOLDER}/charge_price_information_periods'
GO
