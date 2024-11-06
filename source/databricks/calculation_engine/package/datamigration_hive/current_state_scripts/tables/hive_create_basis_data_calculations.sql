CREATE TABLE IF NOT EXISTS {HIVE_BASIS_DATA_DATABASE_NAME}.calculations
(
    calculation_id STRING NOT NULL,
    calculation_type STRING NOT NULL,
    period_start TIMESTAMP NOT NULL,
    period_end TIMESTAMP NOT NULL,
    execution_time_start TIMESTAMP NOT NULL,
    created_by_user_id STRING NOT NULL,
    version BIGINT NOT NULL,
    is_internal_calculation BOOLEAN,
    calculation_succeeded_time TIMESTAMP
)
USING DELTA
TBLPROPERTIES (
    delta.deletedFileRetentionDuration = 'interval 30 days',
    delta.constraints.calculation_id_chk = "LENGTH ( calculation_id ) = 36",
    delta.constraints.calculation_type_chk = "calculation_type IN ( 'balance_fixing' , 'aggregation' , 'wholesale_fixing' , 'first_correction_settlement' , 'second_correction_settlement' , 'third_correction_settlement' )",
    delta.constraints.created_by_user_id_chk = "LENGTH ( created_by_user_id ) = 36",
    delta.constraints.is_internal_calculation_chk = "is_internal_calculation IS NOT NULL",
    delta.columnMapping.mode = 'name'
)
GO
