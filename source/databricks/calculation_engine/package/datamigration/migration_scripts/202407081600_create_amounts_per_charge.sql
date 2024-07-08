CREATE TABLE IF NOT EXISTS {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.monthly_amounts_per_charge
(
    -- 36 characters UUID
    calculation_id STRING NOT NULL,

    -- 36 characters UUID
    result_id STRING NOT NULL,

    grid_area_code STRING NOT NULL,
    energy_supplier_id STRING NOT NULL,
    quantity_unit STRING NOT NULL,
    time TIMESTAMP NOT NULL,
    amount DECIMAL(18, 6),
    is_tax BOOLEAN NOT NULL,
    charge_code STRING NOT NULL,
    charge_type STRING NOT NULL,
    charge_owner_id STRING NOT NULL
)
USING DELTA
TBLPROPERTIES (
    delta.deletedFileRetentionDuration = 'interval 30 days',
    delta.constraints.calculation_id_chk = "LENGTH ( calculation_id ) = 36",
    delta.constraints.result_id_chk = "LENGTH ( result_id ) = 36",
    delta.constraints.grid_area_code_chk = "LENGTH ( grid_area_code ) = 3",
    delta.constraints.energy_supplier_id_chk = "LENGTH ( energy_supplier_id ) = 13 OR LENGTH ( energy_supplier_id ) = 16",
    delta.constraints.charge_owner_id_chk = "LENGTH ( charge_owner_id ) = 13 OR LENGTH ( charge_owner_id ) = 16",
    delta.constraints.charge_type_chk = "charge_type IN ( 'subscription' , 'fee' , 'tariff' )",
    delta.constraints.quantity_unit_chk = "quantity_unit IN ( 'kWh' , 'pcs' )"
)
