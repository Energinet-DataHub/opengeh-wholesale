CREATE TABLE IF NOT EXISTS {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.grid_loss_metering_point_time_series
(
    calculation_id STRING NOT NULL,
    result_id STRING NOT NULL,
    grid_area_code STRING NOT NULL,
    energy_supplier_id STRING,
    balance_responsible_id STRING,
    -- Energy quantity in kWh for the given observation time.
    -- Example: 1234.534
    quantity DECIMAL(18, 3) NOT NULL,
    quantity_qualities ARRAY<STRING> NOT NULL,
    -- The time when the energy was consumed/produced/exchanged
    time TIMESTAMP NOT NULL,
    metering_point_type STRING NOT NULL
    settlement_method STRING
    metering_point_id STRING NOT NULL,
    resolution STRING NOT NULL
)
USING DELTA
TBLPROPERTIES (
    delta.deletedFileRetentionDuration = 'interval 30 days',
    delta.constraints.calculation_id_chk = "LENGTH ( calculation_id ) = 36",
    delta.constraints.result_id_chk = "LENGTH ( result_id ) = 36",
    delta.constraints.grid_area_code_chk = "LENGTH ( grid_area_code ) = 3",
    delta.constraints.energy_supplier_id_chk = "energy_supplier_id IS NULL OR LENGTH ( energy_supplier_id ) = 13 OR LENGTH ( energy_supplier_id ) = 16",
    delta.constraints.balance_responsible_id_chk = "balance_responsible_id IS NULL OR LENGTH ( balance_responsible_id ) = 13 OR LENGTH ( balance_responsible_id ) = 16",
    delta.constraints.quantity_qualities_chk = "array_size ( array_except ( quantity_qualities , array ( 'missing' , 'calculated' , 'measured' , 'estimated' ) ) ) = 0 AND array_size ( quantity_qualities ) > 0",
    delta.constraints.metering_point_id_chk = "metering_point_id IS NULL OR LENGTH ( metering_point_id ) = 18",
    delta.constraints.metering_point_id_conditional_chk = "( time_series_type IN ( 'negative_grid_loss' , 'positive_grid_loss' ) AND metering_point_id IS NOT NULL ) OR ( time_series_type NOT IN ( 'negative_grid_loss' , 'positive_grid_loss' ) AND metering_point_id IS NULL )",
    delta.constraints.resolution_chk = "resolution IN ( 'PT15M' , 'PT1H' )"
)
