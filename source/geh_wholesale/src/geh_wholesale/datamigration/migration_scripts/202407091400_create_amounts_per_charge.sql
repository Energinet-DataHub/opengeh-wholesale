CREATE TABLE IF NOT EXISTS {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.amounts_per_charge
(
    -- 36 characters UUID
    calculation_id STRING NOT NULL,

    -- 36 characters UUID
    result_id STRING NOT NULL,

    grid_area_code STRING NOT NULL,
    energy_supplier_id STRING NOT NULL,
    -- This represents either a) the sum of energy quantities or b) the sum of charge quantities (which is then an integer represented as a Decimal)
    -- Example: 1234.534
    quantity DECIMAL(18, 3) NOT NULL,
    quantity_unit STRING NOT NULL,
    -- Only used for tariffs. NULL for subscriptions and fees.
    quantity_qualities ARRAY<STRING>,
    -- The time and the resolution defines the period that the 'amount' is calculated for.
    time TIMESTAMP NOT NULL,
    resolution STRING NOT NULL,
    metering_point_type STRING NOT NULL,
    -- Null when metering point type is not consumption
    settlement_method STRING,
    price DECIMAL(18, 6),
    -- Null when no price or quantity data.
    amount DECIMAL(18, 6),
    is_tax BOOLEAN NOT NULL,
    charge_code STRING NOT NULL,
    charge_type STRING NOT NULL,
    charge_owner_id STRING NOT NULL
)
USING DELTA
TBLPROPERTIES (
    delta.deletedFileRetentionDuration = 'interval 30 days',
    delta.columnMapping.mode = "name",
    delta.constraints.calculation_id_chk = "LENGTH ( calculation_id ) = 36",
    delta.constraints.result_id_chk = "LENGTH ( result_id ) = 36",
    delta.constraints.grid_area_code_chk = "LENGTH ( grid_area_code ) = 3",
    delta.constraints.energy_supplier_id_chk = "LENGTH ( energy_supplier_id ) = 13 OR LENGTH ( energy_supplier_id ) = 16",
    delta.constraints.quantity_unit_chk = "quantity_unit IN ( 'kWh' , 'pcs' )",
    delta.constraints.quantity_qualities_chk = "( quantity_qualities IS NULL ) OR ( array_size ( array_except ( quantity_qualities , array ( 'missing' , 'calculated' , 'measured' , 'estimated' ) ) ) = 0 AND array_size ( quantity_qualities ) > 0 )",
    delta.constraints.resolution_chk = "resolution IN ( 'PT1H' , 'P1D' , 'P1M' )",
    delta.constraints.metering_point_type_chk = "metering_point_type IN ( 'production' , 'consumption' , 've_production' , 'net_production' , 'supply_to_grid' , 'consumption_from_grid' , 'wholesale_services_information' , 'own_production' , 'net_from_grid' , 'net_to_grid' , 'total_consumption' , 'electrical_heating' , 'net_consumption' , 'effect_settlement' )",
    delta.constraints.settlement_method_chk = "settlement_method IS NULL OR settlement_method IN ( 'non_profiled' , 'flex' )",
    delta.constraints.charge_type_chk = "charge_type IN ( 'subscription' , 'fee' , 'tariff' )",
    delta.constraints.charge_owner_id_chk = "LENGTH ( charge_owner_id ) = 13 OR LENGTH ( charge_owner_id ) = 16"
)
