CREATE TABLE IF NOT EXISTS {BASIS_DATA_DATABASE_NAME}.calculations
(
    calculation_id STRING NOT NULL,
    calculation_type STRING NOT NULL,
    period_start TIMESTAMP NOT NULL,
    period_end TIMESTAMP NOT NULL,
    execution_time_start TIMESTAMP NOT NULL,
    created_by_user_id STRING NOT NULL,
    version BIGINT NOT NULL
)
USING DELTA
TBLPROPERTIES (
    delta.deletedFileRetentionDuration = 'interval 30 days',
    delta.constraints.calculation_id_chk = "LENGTH ( calculation_id ) = 36",
    delta.constraints.calculation_type_chk = "calculation_type IN ( 'BalanceFixing' , 'Aggregation' , 'WholesaleFixing' , 'FirstCorrectionSettlement' , 'SecondCorrectionSettlement' , 'ThirdCorrectionSettlement' )",
    delta.constraints.created_by_user_id_chk = "LENGTH ( created_by_user_id ) = 36"
)
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
{TEST}LOCATION '{CONTAINER_PATH}/{BASIS_DATA_FOLDER}/calculations'
GO
