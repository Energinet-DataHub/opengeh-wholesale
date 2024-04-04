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
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
{TEST}LOCATION '{CONTAINER_PATH}/{BASIS_DATA_FOLDER}/metering_point_periods'
GO

-- Constraints --

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.metering_point_periods
    DROP CONSTRAINT IF EXISTS calculation_id_chk
GO
ALTER TABLE {BASIS_DATA_DATABASE_NAME}.metering_point_periods
    ADD CONSTRAINT calculation_id_chk CHECK (LENGTH(calculation_id) = 36)
GO

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.metering_point_periods
    DROP CONSTRAINT IF EXISTS metering_point_id_chk
GO
ALTER TABLE {BASIS_DATA_DATABASE_NAME}.metering_point_periods
    ADD CONSTRAINT metering_point_id_chk CHECK  (LENGTH(metering_point_id) = 18)
GO

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.metering_point_periods
    DROP CONSTRAINT IF EXISTS metering_point_type_chk
GO
ALTER TABLE {BASIS_DATA_DATABASE_NAME}.metering_point_periods
    ADD CONSTRAINT metering_point_type_chk CHECK (metering_point_type IS NULL OR metering_point_type IN (
        'production',
        'consumption',
        'exchange',
        've_production',
        'net_production',
        'supply_to_grid',
        'consumption_from_grid',
        'wholesale_services_information',
        'own_production',
        'net_from_grid',
        'net_to_grid',
        'total_consumption',
        'electrical_heating',
        'net_consumption',
        'effect_settlement'))
GO

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.metering_point_periods
    DROP CONSTRAINT IF EXISTS settlement_method_chk
GO
ALTER TABLE {BASIS_DATA_DATABASE_NAME}.metering_point_periods
    ADD CONSTRAINT settlement_method_chk CHECK (settlement_method IS NULL OR settlement_method IN ('non_profiled', 'flex'))
GO

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.metering_point_periods
    DROP CONSTRAINT IF EXISTS grid_area_code_chk
GO
ALTER TABLE {BASIS_DATA_DATABASE_NAME}.metering_point_periods
    ADD CONSTRAINT grid_area_code_chk CHECK (LENGTH(grid_area_code) = 3)
GO

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.metering_point_periods
    DROP CONSTRAINT IF EXISTS resolution_chk
GO
ALTER TABLE {BASIS_DATA_DATABASE_NAME}.metering_point_periods
    ADD CONSTRAINT resolution_chk CHECK (resolution IN ('PT1H', 'PT15M'))
GO

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.metering_point_periods
    DROP CONSTRAINT IF EXISTS from_grid_area_code_chk
GO
ALTER TABLE {BASIS_DATA_DATABASE_NAME}.metering_point_periods
    ADD CONSTRAINT from_grid_area_code_chk CHECK (from_grid_area_code IS NULL OR LENGTH(from_grid_area_code) = 3)
GO

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.metering_point_periods
    DROP CONSTRAINT IF EXISTS to_grid_area_code_chk
GO
ALTER TABLE {BASIS_DATA_DATABASE_NAME}.metering_point_periods
    ADD CONSTRAINT to_grid_area_code_chk CHECK (to_grid_area_code IS NULL OR LENGTH(to_grid_area_code) = 3)
GO

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.metering_point_periods
    DROP CONSTRAINT IF EXISTS parent_metering_point_id_chk
GO
ALTER TABLE {BASIS_DATA_DATABASE_NAME}.metering_point_periods
    ADD CONSTRAINT parent_metering_point_id_chk CHECK (parent_metering_point_id IS NULL OR LENGTH(parent_metering_point_id) = 18)
GO

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.metering_point_periods
    DROP CONSTRAINT IF EXISTS energy_supplier_id_chk
GO
-- Length is 16 when EIC and 13 when GLN
ALTER TABLE {BASIS_DATA_DATABASE_NAME}.metering_point_periods
    ADD CONSTRAINT energy_supplier_id_chk CHECK (energy_supplier_id IS NULL OR LENGTH(energy_supplier_id) = 13 OR LENGTH(energy_supplier_id) = 16)
GO

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.metering_point_periods
    DROP CONSTRAINT IF EXISTS balance_responsible_id_chk
GO
-- Length is 16 when EIC and 13 when GLN
ALTER TABLE {BASIS_DATA_DATABASE_NAME}.metering_point_periods
    ADD CONSTRAINT balance_responsible_id_chk CHECK (balance_responsible_id IS NULL OR LENGTH(balance_responsible_id) = 13 OR LENGTH(balance_responsible_id) = 16)
GO
