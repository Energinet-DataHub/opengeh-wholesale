CREATE TABLE IF NOT EXISTS {OUTPUT_DATABASE_NAME}.wholesale_results
(
    -- 36 characters UUID
    calculation_id STRING NOT NULL,
    -- Enum
    calculation_type STRING NOT NULL,
    calculation_execution_time_start TIMESTAMP NOT NULL,

    -- 36 characters UUID
    calculation_result_id STRING NOT NULL,

    grid_area_code STRING NOT NULL,
    energy_supplier_id STRING NOT NULL,
    -- Energy quantity for the given observation time and duration as defined by `resolution`.
    -- Example: 1234.534
    quantity DECIMAL(18, 3),
    quantity_unit STRING NOT NULL,
    quantity_qualities ARRAY<STRING>,
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
    charge_code STRING,
    charge_type STRING,
    charge_owner_id STRING,
    amount_type STRING NOT NULL
)
USING DELTA
TBLPROPERTIES (
    delta.deletedFileRetentionDuration = 'interval 30 days',
    delta.constraints.calculation_id_chk = LENGTH(calculation_id) = 36,
    delta.constraints.calculation_type_chk = calculation_type IN (
        'WholesaleFixing',
        'FirstCorrectionSettlement',
        'SecondCorrectionSettlement',
        'ThirdCorrectionSettlement'),
    delta.constraints.calculation_result_id_chk = LENGTH(calculation_result_id) = 36,
    delta.constraints.grid_area_chk = LENGTH(grid_area) = 3,
    delta.constraints.energy_supplier_id_chk = LENGTH(energy_supplier_id) = 13 OR LENGTH(energy_supplier_id) = 16,
    delta.constraints.quantity_unit_chk = quantity_unit IN ('kWh', 'pcs'),
    delta.constraints.quantity_qualities_chk = array_size(array_except(quantity_qualities, array('missing', 'calculated', 'measured', 'estimated'))) = 0 AND array_size(quantity_qualities) > 0,
    delta.constraints.resolution_chk = resolution IN ('PT1H', 'P1D', 'P1M'),
    delta.constraints.metering_point_type_chk = metering_point_type IS NULL OR metering_point_type IN (
TBLPROPERTIES (delta.deletedFileRetentionDuration = 'interval 30 days')
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
{TEST}LOCATION '{CONTAINER_PATH}/{OUTPUT_FOLDER}/wholesale_results'
GO
