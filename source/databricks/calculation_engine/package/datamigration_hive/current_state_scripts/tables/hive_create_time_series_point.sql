CREATE EXTERNAL TABLE if not exists {INPUT_DATABASE_NAME}.time_series_points
    USING DELTA LOCATION '{CONTAINER_PATH}/{INPUT_FOLDER}/time_series_points_v2'
