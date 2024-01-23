CREATE TABLE IF NOT EXISTS {OUTPUT_DATABASE_NAME}.time_series_hour
(
    calculation_id STRING NOT NULL,
    calculation_type STRING NOT NULL,
    grid_area STRING NOT NULL,
    energy_supplier_id STRING NOT NULL,
    metering_point_id STRING NOT NULL,
    start_time TIMESTAMP NOT NULL,
    quantity_1 DECIMAL(18, 3) NOT NULL,
    quantity_2 DECIMAL(18, 3) NOT NULL,
    quantity_3 DECIMAL(18, 3) NOT NULL,
    quantity_4 DECIMAL(18, 3) NOT NULL,
    quantity_5 DECIMAL(18, 3) NOT NULL,
    quantity_6 DECIMAL(18, 3) NOT NULL,
    quantity_7 DECIMAL(18, 3) NOT NULL,
    quantity_8 DECIMAL(18, 3) NOT NULL,
    quantity_9 DECIMAL(18, 3) NOT NULL,
    quantity_10 DECIMAL(18, 3) NOT NULL,
    quantity_11 DECIMAL(18, 3) NOT NULL,
    quantity_12 DECIMAL(18, 3) NOT NULL,
    quantity_13 DECIMAL(18, 3) NOT NULL,
    quantity_14 DECIMAL(18, 3) NOT NULL,
    quantity_15 DECIMAL(18, 3) NOT NULL,
    quantity_16 DECIMAL(18, 3) NOT NULL,
    quantity_17 DECIMAL(18, 3) NOT NULL,
    quantity_18 DECIMAL(18, 3) NOT NULL,
    quantity_19 DECIMAL(18, 3) NOT NULL,
    quantity_20 DECIMAL(18, 3) NOT NULL,
    quantity_21 DECIMAL(18, 3) NOT NULL,
    quantity_22 DECIMAL(18, 3) NOT NULL,
    quantity_23 DECIMAL(18, 3) NOT NULL,
    quantity_24 DECIMAL(18, 3) NOT NULL,
    quantity_25 DECIMAL(18, 3), -- Only used at daylight saving time
)
USING DELTA
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
{TEST}LOCATION '{CONTAINER_PATH}/{OUTPUT_FOLDER}/time_series_hour'


