CREATE EXTERNAL TABLE if not exists {INPUT_DATABASE_NAME}.metering_point_periods
    USING DELTA LOCATION '{CONTAINER_PATH}/{INPUT_FOLDER}/metering_point_periods'