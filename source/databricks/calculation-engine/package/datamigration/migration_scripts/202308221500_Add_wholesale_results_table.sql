CREATE TABLE IF NOT EXISTS {OUTPUT_DATABASE_NAME}.wholesale_results
(
    calculation_id STRING NOT NULL,
    calculation_type STRING NOT NULL,
    calculation_execution_time_start TIMESTAMP NOT NULL,
    
    result_id STRING NOT NULL,

    grid_area STRING NOT NULL,
    energy_supplier_id STRING,
    -- Energy quantity in for the given observation time and duration as defined by `resolution`.
    -- Null when quality is missing.
    -- Null for monthly sums.
    -- Example: 1234.534
    quantity DECIMAL(18, 3),
    quantity_unit STRING NOT NULL,
    quantity_quality STRING NOT NULL,
    -- The time when the energy was consumed/produced/exchanged
    time TIMESTAMP NOT NULL,
    resolution STRING NOT NULL,

    -- Null when monthly sum
    metering_point_type STRING,
    settlement_method STRING NOT NULL,

    energy_currency STRING NOT NULL,
    price DECIMAL(18, 3),
    amount DECIMAL(18, 3) NOT NULL,

    charge_id STRING,
    charge_type STRING,
    charge_owner_id STRING,
    is_tax BOOLEAN,
)
USING DELTA
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used. 
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.    
{TEST}LOCATION '{CONTAINER_PATH}/{OUTPUT_FOLDER}/wholesale_results'
