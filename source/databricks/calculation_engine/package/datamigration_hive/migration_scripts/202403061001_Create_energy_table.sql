CREATE TABLE IF NOT EXISTS {HIVE_OUTPUT_DATABASE_NAME}.energy_results
(
    grid_area STRING NOT NULL,
    energy_supplier_id STRING,
    balance_responsible_id STRING,
    -- Energy quantity in kWh for the given observation time.
    -- Example: 1234.534
    quantity DECIMAL(18, 3) NOT NULL,
    quantity_qualities ARRAY<STRING> NOT NULL,
    -- The time when the energy was consumed/produced/exchanged
    time TIMESTAMP NOT NULL,
    aggregation_level STRING NOT NULL,
    time_series_type STRING NOT NULL,
    calculation_id STRING NOT NULL,
    calculation_type STRING NOT NULL,
    calculation_execution_time_start TIMESTAMP NOT NULL,
    out_grid_area STRING,
    calculation_result_id STRING NOT NULL,
    metering_point_id STRING
)
USING DELTA
