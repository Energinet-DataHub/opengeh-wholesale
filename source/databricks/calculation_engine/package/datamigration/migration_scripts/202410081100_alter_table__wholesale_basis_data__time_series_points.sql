ALTER TABLE {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.time_series_points
ADD CONSTRAINT metering_point_type_chk
    CHECK (metering_point_type IN ( 'production' , 'consumption' , 'exchange' , 've_production' , 'net_production' , 'supply_to_grid' , 'consumption_from_grid' , 'wholesale_services_information' , 'own_production' , 'net_from_grid' , 'net_to_grid' , 'total_consumption' , 'electrical_heating' , 'net_consumption' , 'effect_settlement' ))
GO

ALTER TABLE {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.time_series_points
ADD CONSTRAINT grid_area_code_chk
    CHECK (LENGTH ( grid_area_code ) = 3)
GO

ALTER TABLE {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.time_series_points
ADD CONSTRAINT resolution_chk
    CHECK (resolution IN ( 'PT1H' , 'PT15M' ))
GO

ALTER TABLE {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.time_series_points
ADD CONSTRAINT energy_supplier_id_chk
    CHECK (energy_supplier_id IS NULL OR LENGTH ( energy_supplier_id ) = 13 OR LENGTH ( energy_supplier_id ) = 16)
