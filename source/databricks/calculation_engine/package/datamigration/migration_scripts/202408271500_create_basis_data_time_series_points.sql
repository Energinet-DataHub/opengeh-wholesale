CREATE TABLE IF NOT EXISTS {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.time_series_points
(
    calculation_id STRING NOT NULL,
    calculation_type STRING NOT NULL,
    calculation_version STRING NOT NULL,
    is_internal_calculation BOOLEAN NOT NULL,
    metering_point_id STRING NOT NULL,
    metering_point_type STRING NOT NULL,
    resolution STRING NOT NULL,
    grid_area_code STRING NOT NULL,
    energy_supplier_id STRING NOT NULL,
    observation_time TIMESTAMP NOT NULL,
    quantity DECIMAL(18, 3) NOT NULL,
    quality STRING NOT NULL,
)
USING DELTA
TBLPROPERTIES (
    delta.deletedFileRetentionDuration = 'interval 30 days',
    delta.constraints.calculation_id_chk = "LENGTH ( calculation_id ) = 36",
    delta.constraints.calculation_type_chk = "calculation_type IN ( 'balance_fixing' , 'aggregation' , 'wholesale_fixing' , 'first_correction_settlement' , 'second_correction_settlement' , 'third_correction_settlement' )",
    delta.constraints.calculation_version_chk = "calculation_version > 0",
    delta.constraints.metering_point_id_chk = "LENGTH ( metering_point_id ) = 18",
    delta.constraints.metering_point_type_chk = "metering_point_type IS NULL OR metering_point_type IN ( 'production' , 'consumption' , 'exchange' , 've_production' , 'net_production' , 'supply_to_grid' , 'consumption_from_grid' , 'wholesale_services_information' , 'own_production' , 'net_from_grid' , 'net_to_grid' , 'total_consumption' , 'electrical_heating' , 'net_consumption' , 'effect_settlement' )",
    delta.constraints.grid_area_code_chk = "LENGTH ( grid_area_code ) = 3",
    delta.constraints.resolution_chk = "resolution IN ( 'PT1H' , 'PT15M' )",
    delta.constraints.energy_supplier_id_chk = "energy_supplier_id IS NULL OR LENGTH ( energy_supplier_id ) = 13 OR LENGTH ( energy_supplier_id ) = 16",
    delta.constraints.quality_chk = "quality IN ( 'missing' , 'estimated' , 'measured' , 'calculated' )"
)
