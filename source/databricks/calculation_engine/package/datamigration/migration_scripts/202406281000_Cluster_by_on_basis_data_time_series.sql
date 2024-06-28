ALTER TABLE {BASIS_DATA_DATABASE_NAME}.time_series_points
CLUSTER BY (metering_point_id)
