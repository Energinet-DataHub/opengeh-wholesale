CREATE TABLE IF NOT EXISTS {OUTPUT_DATABASE_NAME}.wholesale_results
(
    -- 36 characters UUID
    calculation_id STRING NOT NULL,
    -- Enum
    calculation_type STRING NOT NULL,
    calculation_execution_time_start TIMESTAMP NOT NULL,

    -- 36 characters UUID
    calculation_result_id STRING NOT NULL,

    grid_area STRING NOT NULL,
    energy_supplier_id STRING NOT NULL,
    -- Energy quantity for the given observation time and duration as defined by `resolution`.
    -- Example: 1234.534
    quantity DECIMAL(18, 3) NOT NULL,
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
    charge_code STRING NOT NULL,
    charge_type STRING NOT NULL,
    charge_owner_id STRING NOT NULL,
    amount_type STRING NOT NULL
)
USING DELTA
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
{TEST}LOCATION '{CONTAINER_PATH}/{OUTPUT_FOLDER}/wholesale_results'

GO

-- Constraints --

ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
    DROP CONSTRAINT IF EXISTS calculation_id_chk
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
    ADD CONSTRAINT calculation_id_chk CHECK (LENGTH(calculation_id) = 36)
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
    DROP CONSTRAINT IF EXISTS calculation_type_chk
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
    ADD CONSTRAINT calculation_type_chk CHECK (calculation_type IN ('WholesaleFixing', 'FirstCorrectionSettlement', 'SecondCorrectionSettlement', 'ThirdCorrectionSettlement'))
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
    DROP CONSTRAINT IF EXISTS calculation_result_id_chk
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
    ADD CONSTRAINT calculation_result_id_chk CHECK (LENGTH(calculation_result_id) = 36)
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
    DROP CONSTRAINT IF EXISTS grid_area_chk
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
    ADD CONSTRAINT grid_area_chk CHECK (LENGTH(grid_area) = 3)
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
    DROP CONSTRAINT IF EXISTS energy_supplier_id_chk
GO
-- Length is 16 when EIC and 13 when GLN
ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
    ADD CONSTRAINT energy_supplier_id_chk CHECK (LENGTH(energy_supplier_id) = 13 OR LENGTH(energy_supplier_id) = 16)
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
    DROP CONSTRAINT IF EXISTS quantity_unit_chk
GO
-- Unit is kWh when tariff, and pcs (number of pieces) when subscription or fee
ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
    ADD CONSTRAINT quantity_unit_chk CHECK (quantity_unit IN ('kWh', 'pcs'))
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
    DROP CONSTRAINT IF EXISTS quantity_qualities_chk
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
    ADD CONSTRAINT quantity_qualities_chk
    CHECK ((quantity_qualities IS NULL) OR (array_size(array_except(quantity_qualities, array('missing', 'calculated', 'measured', 'estimated'))) = 0
           AND array_size(quantity_qualities) > 0))
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
    DROP CONSTRAINT IF EXISTS resolution_chk
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
    ADD CONSTRAINT resolution_chk CHECK (resolution IN ('PT1H', 'P1D', 'P1M'))
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
    DROP CONSTRAINT IF EXISTS metering_point_type_chk
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
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

ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
    DROP CONSTRAINT IF EXISTS settlement_method_chk
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
    ADD CONSTRAINT settlement_method_chk CHECK (settlement_method IS NULL OR settlement_method IN ('non_profiled', 'flex'))
GO

-- TODO: Any constraints for charge_id?
ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
    DROP CONSTRAINT IF EXISTS charge_type_chk
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
    ADD CONSTRAINT charge_type_chk CHECK (charge_type IN ('subscription', 'fee', 'tariff'))
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
    DROP CONSTRAINT IF EXISTS charge_owner_id_chk
GO
-- Length is 16 when EIC and 13 when GLN
ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
    ADD CONSTRAINT charge_owner_id_chk CHECK (LENGTH(charge_owner_id) = 13 OR LENGTH(charge_owner_id) = 16)
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
    DROP CONSTRAINT IF EXISTS amount_type_chk
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
    ADD CONSTRAINT amount_type_chk CHECK (amount_type IN ('amount_per_charge', 'monthly_amount_per_charge', 'total_monthly_amount'))
GO
