CREATE DATABASE IF NOT EXISTS wholesale_output
COMMENT 'Contains result data from wholesale domain.'
GO

CREATE TABLE IF NOT EXISTS wholesale_output.result
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
LOCATION '{CONTAINER_PATH}/{OUTPUT_FOLDER}/{RESULT_TABLE_NAME}'
