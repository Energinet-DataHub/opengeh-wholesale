CREATE TABLE IF NOT EXISTS {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.energy_per_ga
(
    calculation_id STRING NOT NULL,
    result_id STRING NOT NULL,
    grid_area_code STRING NOT NULL,
    time_series_type STRING NOT NULL,
    resolution STRING NOT NULL,
    -- The time when the energy was consumed/produced/exchanged
    time TIMESTAMP NOT NULL,
    -- Energy quantity in kWh for the given observation time.
    -- Example: 1234.534
    quantity DECIMAL(18, 3) NOT NULL,
    quantity_qualities ARRAY<STRING> NOT NULL
)
USING DELTA
TBLPROPERTIES (
    delta.deletedFileRetentionDuration = 'interval 30 days',
    delta.constraints.calculation_id_chk = "LENGTH ( calculation_id ) = 36",
    delta.constraints.result_id_chk = "LENGTH ( result_id ) = 36",
    delta.constraints.grid_area_code_chk = "LENGTH ( grid_area_code ) = 3",
    delta.constraints.time_series_type_chk = "time_series_type IN ( 'production' , 'non_profiled_consumption' , 'net_exchange_per_ga' , 'flex_consumption' , 'grid_loss' , 'total_consumption' , 'temp_flex_consumption' , 'temp_production' )",
    delta.constraints.resolution_chk = "resolution IN ( 'PT15M' , 'PT1H' )",
    delta.constraints.quantity_qualities_chk = "array_size ( array_except ( quantity_qualities , array ( 'missing' , 'calculated' , 'measured' , 'estimated' ) ) ) = 0 AND array_size ( quantity_qualities ) > 0"
)
