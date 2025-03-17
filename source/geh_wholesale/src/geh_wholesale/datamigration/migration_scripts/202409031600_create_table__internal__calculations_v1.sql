CREATE TABLE IF NOT EXISTS {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations_v1
(
    calculation_id STRING NOT NULL,
    calculation_type STRING NOT NULL,
    calculation_period_start TIMESTAMP NOT NULL,
    calculation_period_end TIMESTAMP NOT NULL,
    -- Use identity column.This is not supported in the test suite
    {DATABRICKS-ONLY}calculation_version BIGINT GENERATED ALWAYS AS IDENTITY,
    {TEST-ONLY}calculation_version BIGINT,
    calculation_execution_time_start TIMESTAMP NOT NULL,
    calculation_succeeded_time TIMESTAMP
)
USING DELTA
TBLPROPERTIES (
    delta.deletedFileRetentionDuration = 'interval 30 days',
    delta.columnMapping.mode = 'name',
    delta.minReaderVersion = '2',
    delta.minWriterVersion = '5',
    delta.constraints.calculation_id_chk = "LENGTH ( calculation_id ) = 36",
    delta.constraints.calculation_type_chk = "calculation_type IN ( 'balance_fixing' , 'aggregation' , 'wholesale_fixing' , 'first_correction_settlement' , 'second_correction_settlement' , 'third_correction_settlement' )",
    delta.constraints.calculation_period_chk = "calculation_period_end > calculation_period_start"
)
