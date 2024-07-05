CREATE TABLE IF NOT EXISTS {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_DATABASE_NAME}.time_series_points
(
    calculation_id STRING NOT NULL,
    metering_point_id STRING NOT NULL,
    quantity DECIMAL(18, 3) NOT NULL,
    quality STRING NOT NULL,
    observation_time TIMESTAMP NOT NULL
)
USING DELTA
TBLPROPERTIES (
    delta.deletedFileRetentionDuration = 'interval 30 days',
    delta.constraints.calculation_id_chk = "LENGTH ( calculation_id ) = 36",
    delta.constraints.metering_point_id_chk = "LENGTH ( metering_point_id ) = 18",
    delta.constraints.quality_chk = "quality IN ( 'missing' , 'estimated' , 'measured' , 'calculated' )"
)
