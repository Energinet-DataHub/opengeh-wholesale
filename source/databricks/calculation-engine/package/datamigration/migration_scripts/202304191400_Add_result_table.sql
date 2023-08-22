CREATE DATABASE IF NOT EXISTS {OUTPUT_DATABASE_NAME}
COMMENT 'Contains result data from wholesale domain.'
GO
CREATE TABLE IF NOT EXISTS {OUTPUT_DATABASE_NAME}.result
(
    grid_area STRING NOT NULL,
    energy_supplier_id STRING,
    balance_responsible_id STRING,
    -- Energy quantity in kWh for the given observation time.
    -- Null when quality is missing.
    -- Example: 1234.534
    quantity DECIMAL(18, 3),
    quantity_quality STRING NOT NULL,
    -- The time when the energy was consumed/produced/exchanged
    time TIMESTAMP NOT NULL,
    aggregation_level STRING NOT NULL,
    time_series_type STRING NOT NULL,
    batch_id STRING NOT NULL,
    batch_process_type STRING NOT NULL,
    batch_execution_time_start TIMESTAMP NOT NULL,
    out_grid_area STRING
)
USING DELTA
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used. 
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.    
{TEST}LOCATION '{CONTAINER_PATH}/{OUTPUT_FOLDER}/result' 
