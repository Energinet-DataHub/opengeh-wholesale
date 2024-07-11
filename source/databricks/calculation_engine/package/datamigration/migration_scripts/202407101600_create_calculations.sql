CREATE TABLE IF NOT EXISTS {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations
(
    calculation_id STRING NOT NULL,
    calculation_type STRING NOT NULL,
    calculation_period_start TIMESTAMP NOT NULL,
    calculation_period_end TIMESTAMP NOT NULL,
    calculation_execution_time_start TIMESTAMP NOT NULL,
    created_by_user_id STRING NOT NULL,
    calculation_version BIGINT NOT NULL
)
USING DELTA
TBLPROPERTIES (
    delta.deletedFileRetentionDuration = 'interval 30 days',
    delta.columnMapping.mode = 'name',
    delta.constraints.calculation_id_chk = "LENGTH ( calculation_id ) = 36",
    delta.constraints.calculation_type_chk = "calculation_type IN ( 'balance_fixing' , 'aggregation' , 'wholesale_fixing' , 'first_correction_settlement' , 'second_correction_settlement' , 'third_correction_settlement' )",
    delta.constraints.created_by_user_id_chk = "LENGTH ( created_by_user_id ) = 36",
    delta.constraints.calculation_version_chk = "calculation_version > 0",
    delta.constraints.calculation_period_chk = "calculation_period_end > calculation_period_start"
)
