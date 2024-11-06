DROP TABLE {INPUT_DATABASE_NAME}.time_series_points
GO

CREATE TABLE if not exists {INPUT_DATABASE_NAME}.time_series_points
USING DELTA
