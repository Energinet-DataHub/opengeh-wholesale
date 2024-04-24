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
    delta.constraints.calculation_id = "LENGTH ( calculation_id ) = 36",
    delta.constraints.metering_point_id_chk = "LENGTH ( metering_point_id ) = 18",
    delta.constraints.metering_point_type_chk = "metering_point_type IS NULL OR metering_point_type in ( 'production' , 'consumption' , 'exchange' , 've_production' , 'net_production' , 'supply_to_grid' , 'consumption_from_grid' , 'wholesale_services_information' , 'own_production' , 'net_from_grid' , 'net_to_grid' , 'total_consumption' , 'electrical_heating' , 'net_consumption' , 'effect_settlement' )",
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
