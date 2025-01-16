ALTER TABLE {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.amounts_per_charge
DROP CONSTRAINT IF EXISTS metering_point_type_chk
GO

ALTER TABLE {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.amounts_per_charge
ADD CONSTRAINT metering_point_type_chk
    CHECK (metering_point_type IN ( 'production' , 'consumption' , 'exchange' , 've_production' , 'net_production' , 'supply_to_grid' , 'consumption_from_grid' , 'wholesale_services_information' , 'own_production' , 'net_from_grid' , 'net_to_grid' , 'total_consumption' , 'electrical_heating' , 'net_consumption' , 'capacity_settlement', 'surplus_production'))
GO
