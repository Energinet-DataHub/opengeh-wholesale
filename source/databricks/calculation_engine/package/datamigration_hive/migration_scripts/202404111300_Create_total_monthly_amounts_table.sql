CREATE TABLE IF NOT EXISTS {HIVE_OUTPUT_DATABASE_NAME}.total_monthly_amounts
(
    -- 36 characters UUID
    calculation_id STRING NOT NULL,
    -- Enum
    calculation_type STRING NOT NULL,
    calculation_execution_time_start TIMESTAMP NOT NULL,
    
    -- 36 characters UUID
    calculation_result_id STRING NOT NULL,

    grid_area STRING NOT NULL,
    energy_supplier_id STRING,
    time TIMESTAMP NOT NULL,
    amount DECIMAL(18, 6),
    charge_owner_id STRING
)
USING DELTA
TBLPROPERTIES (delta.deletedFileRetentionDuration = 'interval 30 days')
