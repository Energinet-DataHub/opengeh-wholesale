-- Hack to make sure that the script is not executed when running integration tests
CREATE TABLE if not exists {INPUT_DATABASE_NAME}.metering_point_periods
    USING DELTA
GO

CREATE EXTERNAL
TABLE if not exists {INPUT_DATABASE_NAME}.time_series_points
    USING DELTA
GO

CREATE EXTERNAL
TABLE if not exists {INPUT_DATABASE_NAME}.charge_link_periods
USING DELTA
GO

CREATE EXTERNAL
TABLE if not exists {INPUT_DATABASE_NAME}.charge_masterdata_periods
    USING DELTA
GO

CREATE EXTERNAL
TABLE if not exists {INPUT_DATABASE_NAME}.charge_price_points
    USING DELTA
GO
