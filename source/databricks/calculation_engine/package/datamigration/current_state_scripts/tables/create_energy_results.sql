CREATE TABLE IF NOT EXISTS {OUTPUT_DATABASE_NAME}.energy_results
(
    grid_area_code STRING NOT NULL,
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
    out_grid_area_code STRING,
    calculation_result_id STRING NOT NULL,
    metering_point_id STRING,
    resolution STRING
)
USING DELTA
TBLPROPERTIES (
    delta.deletedFileRetentionDuration = 'interval 30 days',
    delta.constraints.calculation_type_chk = "calculation_type IN ( 'BalanceFixing' , 'Aggregation' , 'WholesaleFixing' , 'FirstCorrectionSettlement' , 'SecondCorrectionSettlement' , 'ThirdCorrectionSettlement' )",
    delta.constraints.time_series_type_chk = "time_series_type IN ( 'production' , 'non_profiled_consumption' , 'net_exchange_per_neighboring_ga' , 'net_exchange_per_ga' , 'flex_consumption' , 'grid_loss' , 'negative_grid_loss' , 'positive_grid_loss' , 'total_consumption' , 'temp_flex_consumption' , 'temp_production' )",
    delta.constraints.quantity_qualities_chk = "array_size ( array_except ( quantity_qualities , array ( 'missing' , 'calculated' , 'measured' , 'estimated' ) ) ) = 0 AND array_size ( quantity_qualities ) > 0",
    delta.constraints.aggregation_level_chk = "aggregation_level IN ( 'total_ga' , 'es_brp_ga' , 'es_ga' , 'brp_ga' )",
    delta.constraints.energy_supplier_id_chk = "energy_supplier_id IS NULL OR LENGTH ( energy_supplier_id ) = 13 OR LENGTH ( energy_supplier_id ) = 16",
    delta.constraints.balance_responsible_id_chk = "balance_responsible_id IS NULL OR LENGTH ( balance_responsible_id ) = 13 OR LENGTH ( balance_responsible_id ) = 16",
    delta.constraints.calculation_id_chk = "LENGTH ( calculation_id ) = 36",
    delta.constraints.calculation_result_id_chk = "LENGTH ( calculation_result_id ) = 36",
    delta.constraints.metering_point_id_chk = "metering_point_id IS NULL OR LENGTH ( metering_point_id ) = 18",
    delta.constraints.metering_point_id_conditional_chk = "( time_series_type IN ( 'negative_grid_loss' , 'positive_grid_loss' ) AND metering_point_id IS NOT NULL ) OR ( time_series_type NOT IN ( 'negative_grid_loss' , 'positive_grid_loss' ) AND metering_point_id IS NULL )",
    delta.columnMapping.mode = "name",
    delta.minReaderVersion = "2",
    delta.minWriterVersion = "5",
    delta.constraints.grid_area_code_chk = "LENGTH ( grid_area_code ) = 3",
    delta.constraints.out_grid_area_code_chk = "out_grid_area_code IS NULL OR LENGTH ( out_grid_area_code ) = 3"
    delta.constraints.resolution_chk = "resolution IN ( 'PT15M' , 'PT1H' )"
)
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
{TEST}LOCATION '{CONTAINER_PATH}/{OUTPUT_FOLDER}/result'
GO
