-- Create schemas
CREATE DATABASE IF NOT EXISTS {BASIS_DATA_DATABASE_NAME}
COMMENT 'Contains basis data from wholesale subsystem.'
GO

CREATE DATABASE IF NOT EXISTS {INPUT_DATABASE_NAME}
COMMENT 'Contains input data from wholesale domain.'
GO

CREATE DATABASE IF NOT EXISTS {OUTPUT_DATABASE_NAME}
COMMENT 'Contains result data from wholesale domain.'
GO

CREATE DATABASE IF NOT EXISTS {SETTLEMENT_REPORT_DATABASE_NAME}
COMMENT 'Contains settlement report views from wholesale subsystem.'
GO

-- Create tables
DROP TABLE IF EXISTS {BASIS_DATA_DATABASE_NAME}.calculations
GO

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

DROP TABLE IF EXISTS {BASIS_DATA_DATABASE_NAME}.charge_link_periods
GO

CREATE TABLE IF NOT EXISTS {BASIS_DATA_DATABASE_NAME}.charge_link_periods
(
    calculation_id STRING NOT NULL,
    charge_key STRING NOT NULL,
    charge_code STRING NOT NULL,
    charge_type STRING NOT NULL,
    charge_owner_id STRING NOT NULL,
    metering_point_id STRING NOT NULL,
    quantity int NOT NULL,
    from_date TIMESTAMP NOT NULL,
    to_date TIMESTAMP
)
USING DELTA
TBLPROPERTIES (
    delta.deletedFileRetentionDuration = 'interval 30 days',
    delta.constraints.calculation_id_chk = "LENGTH ( calculation_id ) = 36",
    delta.constraints.charge_type_chk = "charge_type IN ( 'subscription' , 'fee' , 'tariff' )",
    delta.constraints.charge_owner_id_chk = "LENGTH ( charge_owner_id ) = 13 OR LENGTH ( charge_owner_id ) = 16",
    delta.constraints.metering_point_id_chk = "LENGTH ( metering_point_id ) = 18"
)
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
{TEST}LOCATION '{CONTAINER_PATH}/{BASIS_DATA_FOLDER}/charge_link_periods'
GO

DROP TABLE IF EXISTS {BASIS_DATA_DATABASE_NAME}.charge_masterdata_periods
GO

CREATE TABLE IF NOT EXISTS {BASIS_DATA_DATABASE_NAME}.charge_masterdata_periods
(
    calculation_id STRING NOT NULL,
    charge_key STRING NOT NULL,
    charge_code STRING NOT NULL,
    charge_type STRING NOT NULL,
    charge_owner_id STRING NOT NULL,
    resolution STRING NOT NULL,
    is_tax BOOLEAN NOT NULL,
    from_date TIMESTAMP NOT NULL,
    to_date TIMESTAMP
)
USING DELTA
TBLPROPERTIES (
    delta.deletedFileRetentionDuration = 'interval 30 days',
    delta.constraints.calculation_id_chk = "LENGTH ( calculation_id ) = 36",
    delta.constraints.charge_type_chk = "charge_type IN ( 'subscription' , 'fee' , 'tariff' )",
    delta.constraints.charge_owner_id_chk = "LENGTH ( charge_owner_id ) = 13 OR LENGTH ( charge_owner_id ) = 16",
    delta.constraints.resolution_chk = "resolution IN ( 'PT1H' , 'P1D' , 'P1M' )"
)
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
{TEST}LOCATION '{CONTAINER_PATH}/{BASIS_DATA_FOLDER}/charge_masterdata_periods'
GO

DROP TABLE IF EXISTS {BASIS_DATA_DATABASE_NAME}.charge_price_points
GO

CREATE TABLE IF NOT EXISTS {BASIS_DATA_DATABASE_NAME}.charge_price_points
(
    calculation_id STRING NOT NULL,
    charge_key STRING NOT NULL,
    charge_code STRING NOT NULL,
    charge_type STRING NOT NULL,
    charge_owner_id STRING NOT NULL,
    charge_price DECIMAL(18, 6) NOT NULL,
    charge_time TIMESTAMP NOT NULL
)
USING DELTA
TBLPROPERTIES (
    delta.deletedFileRetentionDuration = 'interval 30 days',
    delta.constraints.calculation_id_chk = "LENGTH ( calculation_id ) = 36",
    delta.constraints.charge_type_chk = "charge_type IN ( 'subscription' , 'fee' , 'tariff' )",
    delta.constraints.charge_owner_id_chk = "LENGTH ( charge_owner_id ) = 13 OR LENGTH ( charge_owner_id ) = 16"
)
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
{TEST}LOCATION '{CONTAINER_PATH}/{BASIS_DATA_FOLDER}/charge_price_points'
GO

DROP TABLE IF EXISTS {BASIS_DATA_DATABASE_NAME}.grid_loss_metering_points
GO

CREATE TABLE IF NOT EXISTS {BASIS_DATA_DATABASE_NAME}.grid_loss_metering_points
(
    calculation_id STRING NOT NULL,
    metering_point_id STRING NOT NULL
)
USING DELTA
TBLPROPERTIES (
    delta.deletedFileRetentionDuration = 'interval 30 days',
    delta.constraints.calculation_id_chk = "LENGTH ( calculation_id ) = 36",
    delta.constraints.metering_point_id_chk = "LENGTH ( metering_point_id ) = 18"
)
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
{TEST}LOCATION '{CONTAINER_PATH}/{BASIS_DATA_FOLDER}/grid_loss_metering_points'
GO

DROP TABLE IF EXISTS {BASIS_DATA_DATABASE_NAME}.metering_point_periods
GO

CREATE TABLE IF NOT EXISTS {BASIS_DATA_DATABASE_NAME}.metering_point_periods
(
    calculation_id STRING NOT NULL,
    metering_point_id STRING NOT NULL,
    metering_point_type STRING NOT NULL,
    settlement_method STRING,
    grid_area_code STRING NOT NULL,
    resolution STRING NOT NULL,
    from_grid_area_code STRING,
    to_grid_area_code STRING,
    parent_metering_point_id STRING,
    energy_supplier_id STRING,
    balance_responsible_id STRING,
    from_date TIMESTAMP NOT NULL,
    to_date TIMESTAMP
)
USING DELTA
TBLPROPERTIES (
    delta.deletedFileRetentionDuration = 'interval 30 days',
    delta.constraints.calculation_id_chk = "LENGTH ( calculation_id ) = 36",
    delta.constraints.metering_point_id_chk = "LENGTH ( metering_point_id ) = 18",
    delta.constraints.metering_point_type_chk = "metering_point_type IS NULL OR metering_point_type IN ( 'production' , 'consumption' , 'exchange' , 've_production' , 'net_production' , 'supply_to_grid' , 'consumption_from_grid' , 'wholesale_services_information' , 'own_production' , 'net_from_grid' , 'net_to_grid' , 'total_consumption' , 'electrical_heating' , 'net_consumption' , 'effect_settlement' )",
    delta.constraints.settlement_method_chk = "settlement_method IS NULL OR settlement_method IN ( 'non_profiled' , 'flex' )",
    delta.constraints.grid_area_code_chk = "LENGTH ( grid_area_code ) = 3",
    delta.constraints.resolution_chk = "resolution IN ( 'PT1H' , 'PT15M' )",
    delta.constraints.from_grid_area_code_chk = "from_grid_area_code IS NULL OR LENGTH ( from_grid_area_code ) = 3",
    delta.constraints.to_grid_area_code_chk = "to_grid_area_code IS NULL OR LENGTH ( to_grid_area_code ) = 3",
    delta.constraints.parent_metering_point_id_chk = "parent_metering_point_id IS NULL OR LENGTH ( parent_metering_point_id ) = 18",
    delta.constraints.energy_supplier_id_chk = "energy_supplier_id IS NULL OR LENGTH ( energy_supplier_id ) = 13 OR LENGTH ( energy_supplier_id ) = 16",
    delta.constraints.balance_responsible_id_chk = "balance_responsible_id IS NULL OR LENGTH ( balance_responsible_id ) = 13 OR LENGTH ( balance_responsible_id ) = 16"
)
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
{TEST}LOCATION '{CONTAINER_PATH}/{BASIS_DATA_FOLDER}/metering_point_periods'
GO

DROP TABLE IF EXISTS {BASIS_DATA_DATABASE_NAME}.time_series_points
GO

CREATE TABLE IF NOT EXISTS {BASIS_DATA_DATABASE_NAME}.time_series_points
(
    calculation_id STRING NOT NULL,
    metering_point_id STRING NOT NULL,
    quantity DECIMAL(18, 3) NOT NULL,
    quality STRING NOT NULL,
    observation_time TIMESTAMP NOT NULL
)
USING DELTA
TBLPROPERTIES (
    delta.deletedFileRetentionDuration = 'interval 30 days',
    delta.constraints.calculation_id_chk = "LENGTH ( calculation_id ) = 36",
    delta.constraints.metering_point_id_chk = "LENGTH ( metering_point_id ) = 18",
    delta.constraints.quality_chk = "quality IN ( 'missing' , 'estimated' , 'measured' , 'calculated' )"
)
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
{TEST}LOCATION '{CONTAINER_PATH}/{BASIS_DATA_FOLDER}/time_series'
GO

CREATE EXTERNAL TABLE if not exists {INPUT_DATABASE_NAME}.charge_link_periods
    USING DELTA LOCATION '{CONTAINER_PATH}/{INPUT_FOLDER}/charge_link_periods'
GO

CREATE EXTERNAL TABLE if not exists {INPUT_DATABASE_NAME}.charge_masterdata_periods
    USING DELTA LOCATION '{CONTAINER_PATH}/{INPUT_FOLDER}/charge_masterdata_periods'
GO

CREATE EXTERNAL TABLE if not exists {INPUT_DATABASE_NAME}.charge_price_points
    USING DELTA LOCATION '{CONTAINER_PATH}/{INPUT_FOLDER}/charge_price_points'
GO

DROP TABLE IF EXISTS {OUTPUT_DATABASE_NAME}.energy_results
GO

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
    delta.constraints.out_grid_area_code_chk = "out_grid_area_code IS NULL OR LENGTH ( out_grid_area_code ) = 3",
    delta.constraints.resolution_chk = "resolution IN ( 'PT15M' , 'PT1H' )"
)
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
{TEST}LOCATION '{CONTAINER_PATH}/{OUTPUT_FOLDER}/result'
GO

DROP TABLE IF EXISTS {INPUT_DATABASE_NAME}.grid_loss_metering_points
GO

CREATE TABLE IF NOT EXISTS {INPUT_DATABASE_NAME}.grid_loss_metering_points
(
    metering_point_id STRING NOT NULL
)
USING DELTA
TBLPROPERTIES (
    delta.deletedFileRetentionDuration = 'interval 30 days',
    delta.constraints.metering_point_id_chk = "LENGTH ( metering_point_id ) = 18"
)
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
{TEST}LOCATION '{CONTAINER_PATH}/{INPUT_FOLDER}/grid_loss_metering_points'
GO

CREATE EXTERNAL TABLE if not exists {INPUT_DATABASE_NAME}.time_series_points
    USING DELTA LOCATION '{CONTAINER_PATH}/{INPUT_FOLDER}/time_series_points_v2'
GO

DROP TABLE IF EXISTS {OUTPUT_DATABASE_NAME}.total_monthly_amounts
GO

CREATE TABLE IF NOT EXISTS {OUTPUT_DATABASE_NAME}.total_monthly_amounts
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
    time TIMESTAMP NOT NULL,
    amount DECIMAL(18, 6),
    charge_owner_id STRING
)
USING DELTA
TBLPROPERTIES (
    delta.deletedFileRetentionDuration = 'interval 30 days',
    delta.constraints.calculation_id_chk = "LENGTH ( calculation_id ) = 36",
    delta.constraints.calculation_type_chk = "calculation_type IN ( 'WholesaleFixing' , 'FirstCorrectionSettlement' , 'SecondCorrectionSettlement' , 'ThirdCorrectionSettlement' )",
    delta.constraints.calculation_result_id_chk = "LENGTH ( calculation_result_id ) = 36",
    delta.constraints.energy_supplier_id_chk = "LENGTH ( energy_supplier_id ) = 13 OR LENGTH ( energy_supplier_id ) = 16",
    delta.constraints.charge_owner_id_chk = "charge_owner_id IS NULL OR LENGTH ( charge_owner_id ) = 13 OR LENGTH ( charge_owner_id ) = 16",
    delta.columnMapping.mode = "name",
    delta.minReaderVersion = "2",
    delta.minWriterVersion = "5",
    delta.constraints.grid_area_code_chk = "LENGTH ( grid_area_code ) = 3"
)
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
{TEST}LOCATION '{CONTAINER_PATH}/{OUTPUT_FOLDER}/total_monthly_amounts'
GO

DROP TABLE IF EXISTS {OUTPUT_DATABASE_NAME}.wholesale_results
GO

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
    delta.constraints.calculation_id_chk = "LENGTH ( calculation_id ) = 36",
    delta.constraints.calculation_type_chk = "calculation_type IN ( 'WholesaleFixing' , 'FirstCorrectionSettlement' , 'SecondCorrectionSettlement' , 'ThirdCorrectionSettlement' )",
    delta.constraints.calculation_result_id_chk = "LENGTH ( calculation_result_id ) = 36",
    delta.constraints.energy_supplier_id_chk = "LENGTH ( energy_supplier_id ) = 13 OR LENGTH ( energy_supplier_id ) = 16",
    delta.constraints.quantity_unit_chk = "quantity_unit IN ( 'kWh' , 'pcs' )",
    delta.constraints.quantity_qualities_chk = "( quantity_qualities IS NULL ) OR ( array_size ( array_except ( quantity_qualities , array ( 'missing' , 'calculated' , 'measured' , 'estimated' ) ) ) = 0 AND array_size ( quantity_qualities ) > 0 )",
    delta.constraints.resolution_chk = "resolution IN ( 'PT1H' , 'P1D' , 'P1M' )",
    delta.constraints.metering_point_type_chk = "metering_point_type IS NULL OR metering_point_type IN ( 'production' , 'consumption' , 'exchange' , 've_production' , 'net_production' , 'supply_to_grid' , 'consumption_from_grid' , 'wholesale_services_information' , 'own_production' , 'net_from_grid' , 'net_to_grid' , 'total_consumption' , 'electrical_heating' , 'net_consumption' , 'effect_settlement' )",
    delta.constraints.settlement_method_chk = "settlement_method IS NULL OR settlement_method IN ( 'non_profiled' , 'flex' )",
    delta.constraints.charge_type_chk = "charge_type IN ( 'subscription' , 'fee' , 'tariff' )",
    delta.constraints.charge_owner_id_chk = "LENGTH ( charge_owner_id ) = 13 OR LENGTH ( charge_owner_id ) = 16",
    delta.constraints.amount_type_chk = "amount_type IN ( 'amount_per_charge' , 'monthly_amount_per_charge' , 'total_monthly_amount' )",
    delta.columnMapping.mode = "name",
    delta.minReaderVersion = "2",
    delta.minWriterVersion = "5",
    delta.constraints.grid_area_code_chk = "LENGTH ( grid_area_code ) = 3"
)
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
{TEST}LOCATION '{CONTAINER_PATH}/{OUTPUT_FOLDER}/wholesale_results'
GO

--Create views
CREATE VIEW {SETTLEMENT_REPORT_DATABASE_NAME}.energy_results_v1 as
SELECT e.calculation_id,
       e.calculation_type,
       e.grid_area_code,
       CASE
           WHEN e.time_series_type = 'production' THEN 'production'
           WHEN e.time_series_type = 'non_profiled_consumption' THEN 'consumption'
           WHEN e.time_series_type = 'flex_consumption' THEN 'consumption'
           WHEN e.time_series_type = 'net_exchange_per_ga' THEN 'exchange'
           WHEN e.time_series_type = 'total_consumption' THEN 'consumption'
       END as metering_point_type,
       CASE
           WHEN e.time_series_type = 'production' THEN NULL
           WHEN e.time_series_type = 'non_profiled_consumption' THEN 'non_profiled'
           WHEN e.time_series_type = 'flex_consumption' THEN 'flex'
           WHEN e.time_series_type = 'net_exchange_per_ga' THEN NULL
           WHEN e.time_series_type = 'total_consumption' THEN NULL
       END as settlement_method,
       e.resolution,
       e.time,
       e.quantity,
       e.energy_supplier_id,
       e.aggregation_level
FROM {OUTPUT_DATABASE_NAME}.energy_results AS e
INNER JOIN (SELECT calculation_id FROM {BASIS_DATA_DATABASE_NAME}.calculations) AS c ON c.calculation_id = e.calculation_id
WHERE e.time_series_type IN ('production', 'non_profiled_consumption', 'flex_consumption', 'net_exchange_per_ga', 'total_consumption')
GO

CREATE VIEW IF NOT EXISTS {SETTLEMENT_REPORT_DATABASE_NAME}.metering_point_periods_v1 as
SELECT calculation_id,
       metering_point_id,
       from_date,
       to_date,
       grid_area_code,
       from_grid_area_code,
       to_grid_area_code,
       metering_point_type,
       settlement_method,
       energy_supplier_id
FROM {BASIS_DATA_DATABASE_NAME}.metering_point_periods
GO

CREATE VIEW IF NOT EXISTS {SETTLEMENT_REPORT_DATABASE_NAME}.metering_point_time_series_v1 as
SELECT m.calculation_id,
       m.metering_point_id,
       m.metering_point_type,
       m.resolution,
       m.grid_area_code,
       m.energy_supplier_id,
       DATE_TRUNC('day', FROM_UTC_TIMESTAMP(t.observation_time, 'Europe/Copenhagen')) AS observation_day,
       ARRAY_SORT(ARRAY_AGG(struct(t.observation_time, t.quantity)))                  AS quantities
FROM {BASIS_DATA_DATABASE_NAME}.metering_point_periods AS m
         JOIN (SELECT * FROM {BASIS_DATA_DATABASE_NAME}.time_series_points order by observation_time) AS t ON m.metering_point_id = t.metering_point_id AND m.calculation_id = t
             .calculation_id
WHERE t.observation_time >= m.from_date
  AND t.observation_time < m.to_date
GROUP BY m.calculation_id, m.metering_point_id, m.metering_point_type, observation_day, m.resolution, m.grid_area_code, m.energy_supplier_id
ORDER BY observation_day
GO