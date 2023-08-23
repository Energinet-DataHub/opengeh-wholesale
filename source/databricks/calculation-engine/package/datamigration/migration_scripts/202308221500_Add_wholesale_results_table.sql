CREATE TABLE IF NOT EXISTS {OUTPUT_DATABASE_NAME}.wholesale_results
(
    batch_id STRING NOT NULL,
    batch_process_type STRING NOT NULL,
    batch_execution_time_start TIMESTAMP NOT NULL,
    
    calculation_result_id STRING NOT NULL,

    grid_area STRING NOT NULL,
    energy_supplier_id STRING,
    -- Energy quantity for the given observation time and duration as defined by `resolution`.
    -- Null when quality is missing.
    -- Null when monthly sum.
    -- Example: 1234.534
    quantity DECIMAL(18, 3),
    quantity_unit STRING NOT NULL,
    quantity_quality STRING NOT NULL,
    -- The time when the energy was consumed/produced/exchanged
    time TIMESTAMP NOT NULL,
    resolution STRING NOT NULL,

    -- Null when monthly sum
    metering_point_type STRING,
    -- Null when metering point type is production
    settlement_method STRING,

    -- Null when monthly sum.
    price DECIMAL(18, 6),
    amount DECIMAL(18, 6) NOT NULL,

    -- Null when monthly sum.
    charge_id STRING,
    -- Null when monthly sum.
    charge_type STRING,
    -- Null when monthly sum.
    charge_owner_id STRING,
    -- Null when monthly sum.
    is_tax BOOLEAN
)
USING DELTA
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used. 
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.    
{TEST}LOCATION '{CONTAINER_PATH}/{OUTPUT_FOLDER}/wholesale_results'
