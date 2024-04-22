DROP TABLE {INPUT_DATABASE_NAME}.time_series_points
GO

CREATE
{TEST} EXTERNAL
TABLE if not exists {INPUT_DATABASE_NAME}.time_series_points
    USING DELTA
{TEST} LOCATION '{CONTAINER_PATH}/{INPUT_FOLDER}/time_series_points_v2'