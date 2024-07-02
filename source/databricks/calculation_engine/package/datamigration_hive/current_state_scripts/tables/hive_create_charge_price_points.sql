CREATE EXTERNAL TABLE if not exists {INPUT_DATABASE_NAME}.charge_price_points
    USING DELTA LOCATION '{CONTAINER_PATH}/{INPUT_FOLDER}/charge_price_points'
