CREATE EXTERNAL TABLE if not exists {INPUT_DATABASE_NAME}.charge_link_periods
    USING DELTA LOCATION '{CONTAINER_PATH}/{INPUT_FOLDER}/charge_link_periods'
