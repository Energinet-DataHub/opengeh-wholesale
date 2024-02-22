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
    energy_supplier_id STRING NOT NULL,
    -- Energy quantity for the given observation time and duration as defined by `resolution`.
    -- Example: 1234.534
    quantity DECIMAL(18, 3) NOT NULL,
    quantity_unit STRING NOT NULL,
    quantity_qualities ARRAY<STRING NOT NULL> NOT NULL,
    -- The time when the energy was consumed/produced/exchanged
    time TIMESTAMP NOT NULL,
    resolution STRING NOT NULL,
    -- Null when monthly sum
    metering_point_type STRING,
    -- Null when metering point type is not consumption or when monthly sum
    settlement_method STRING,
    -- Null when monthly sum or when no price data.
    price DECIMAL(18, 6),
    amount DECIMAL(18, 6),
    -- Applies only to tariff. Null when subscription or fee.
    is_tax BOOLEAN,
    charge_code STRING NOT NULL,
    charge_type STRING NOT NULL,
    charge_owner_id STRING NOT NULL,
    amount_type STRING NOT NULL
)
USING DELTA
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used. 
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.    
{TEST}LOCATION '{CONTAINER_PATH}/{OUTPUT_FOLDER}/wholesale_results'
