CREATE TABLE IF NOT EXISTS {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.energy_per_brp
(
    grid_area_code STRING NOT NULL,
    balance_responsible_id STRING NOT NULL,
    -- Energy quantity in kWh for the given observation time.
    -- Example: 1234.534
    quantity DECIMAL(18, 3) NOT NULL,
    quantity_qualities ARRAY<STRING> NOT NULL,
    -- The time when the energy was consumed/produced/exchanged
    time TIMESTAMP NOT NULL,
    metering_point_type STRING NOT NULL
    settlement_method STRING
    calculation_id STRING NOT NULL,
    calculation_result_id STRING NOT NULL,
    resolution STRING NOT NULL
)
USING DELTA
TBLPROPERTIES (
    delta.deletedFileRetentionDuration = 'interval 30 days',
    delta.constraints.quantity_qualities_chk = "array_size ( array_except ( quantity_qualities , array ( 'missing' , 'calculated' , 'measured' , 'estimated' ) ) ) = 0 AND array_size ( quantity_qualities ) > 0",
    delta.constraints.balance_responsible_id_chk = "LENGTH ( balance_responsible_id ) = 13 OR LENGTH ( balance_responsible_id ) = 16",
    delta.constraints.calculation_id_chk = "LENGTH ( calculation_id ) = 36",
    delta.constraints.calculation_result_id_chk = "LENGTH ( calculation_result_id ) = 36",
    delta.constraints.grid_area_code_chk = "LENGTH ( grid_area_code ) = 3",
    delta.constraints.resolution_chk = "resolution IN ( 'PT15M' , 'PT1H' )"
    delta.constraints.metering_point_type_chk = "metering_point_type IN ( 'production' , 'consumption' , 'exchange')",
    delta.constraints.settlement_method_chk = "settlement_method IS NULL OR ( metering_point_type = 'consumption' AND settlement_method IN ( 'non_profiled' , 'flex' ) )",
)
