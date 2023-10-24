CREATE TABLE IF NOT EXISTS {OUTPUT_DATABASE_NAME}.wholesale_results
(
    -- 36 characters UUID
    calculation_id STRING NOT NULL,
    -- Enum
    calculation_type STRING NOT NULL,
    calculation_execution_time_start TIMESTAMP NOT NULL,
    
    -- 36 characters UUID
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
    -- Null when metering point type is not consumption
    settlement_method STRING,

    -- Null when monthly sum.
    price DECIMAL(18, 6),
    amount DECIMAL(18, 6) NOT NULL,
    -- Applies only to tariff. Null when subscription or fee.
    is_tax BOOLEAN,

    -- Null when total sum.
    charge_id STRING,
    -- Null when total sum.
    charge_type STRING,
    -- Null when total sum.
    charge_owner_id STRING
)
USING DELTA
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used. 
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.    
{TEST}LOCATION '{CONTAINER_PATH}/{OUTPUT_FOLDER}/wholesale_results'
