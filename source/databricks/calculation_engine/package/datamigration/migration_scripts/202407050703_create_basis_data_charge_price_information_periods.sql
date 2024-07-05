CREATE TABLE IF NOT EXISTS {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_DATABASE_NAME}.charge_price_information_periods
(
    calculation_id STRING NOT NULL,
    charge_key STRING NOT NULL,
    charge_code STRING NOT NULL,
    charge_type STRING NOT NULL,
    charge_owner_id STRING NOT NULL,
    resolution STRING NOT NULL,
    is_tax BOOLEAN NOT NULL,
    from_date TIMESTAMP NOT NULL,
    to_date TIMESTAMP NOT NULL
)
USING DELTA
TBLPROPERTIES (
    delta.deletedFileRetentionDuration = 'interval 30 days',
    delta.constraints.calculation_id_chk = "LENGTH ( calculation_id ) = 36",
    delta.constraints.charge_type_chk = "charge_type IN ( 'subscription' , 'fee' , 'tariff' )",
    delta.constraints.charge_owner_id_chk = "LENGTH ( charge_owner_id ) = 13 OR LENGTH ( charge_owner_id ) = 16",
    delta.constraints.resolution_chk = "resolution IN ( 'PT1H' , 'P1D' , 'P1M' )"
)
