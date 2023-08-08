CREATE DATABASE IF NOT EXISTS {DATABASE_NAME}
COMMENT 'Contains result data from wholesale domain.'
GO
    
CREATE TABLE IF NOT EXISTS {DATABASE_NAME}.{RESULT_TABLE_NAME}
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
-- In the test environment the TEST keyword is set to "--" (commented out). 
-- In the production environment the TEST keyword should is set to empty and the respective table location is used.
-- This means the production tables won't be deleted if the schema is.    
{TEST}LOCATION '{OUTPUT_FOLDER}.{RESULT_TABLE_NAME}' 
