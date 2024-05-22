IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{INPUT_DATABASE_NAME}' AND TABLE_NAME = 'charge_masterdata_periods')
BEGIN
    ALTER TABLE {INPUT_DATABASE_NAME}.charge_masterdata_periods RENAME TO charge_price_information_periods
END
GO
ALTER TABLE {INPUT_DATABASE_NAME}.charge_price_information_periods SET LOCATION '{CONTAINER_PATH}/{INPUT_FOLDER}/charge_price_information_periods'
GO
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '{BASIS_DATA_DATABASE_NAME}' AND TABLE_NAME = 'charge_masterdata_periods')
BEGIN
    ALTER TABLE {BASIS_DATA_DATABASE_NAME}.charge_masterdata_periods RENAME TO charge_price_information_periods
END
GO
ALTER TABLE {BASIS_DATA_DATABASE_NAME}.charge_price_information_periods SET LOCATION '{CONTAINER_PATH}/{BASIS_DATA_FOLDER}/charge_price_information_periods'
GO
