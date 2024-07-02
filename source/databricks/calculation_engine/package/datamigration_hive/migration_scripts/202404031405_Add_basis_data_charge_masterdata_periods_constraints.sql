ALTER TABLE {BASIS_DATA_DATABASE_NAME}.charge_masterdata_periods
    DROP CONSTRAINT IF EXISTS calculation_id_chk
GO
ALTER TABLE {BASIS_DATA_DATABASE_NAME}.charge_masterdata_periods
    ADD CONSTRAINT calculation_id_chk CHECK (LENGTH(calculation_id) = 36)
GO

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.charge_masterdata_periods
    DROP CONSTRAINT IF EXISTS charge_type_chk
GO
ALTER TABLE {BASIS_DATA_DATABASE_NAME}.charge_masterdata_periods
    ADD CONSTRAINT charge_type_chk CHECK (charge_type IN ('subscription', 'fee', 'tariff'))
GO

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.charge_masterdata_periods
    DROP CONSTRAINT IF EXISTS charge_owner_id_chk
GO
-- Length is 16 when EIC and 13 when GLN
ALTER TABLE {BASIS_DATA_DATABASE_NAME}.charge_masterdata_periods
    ADD CONSTRAINT charge_owner_id_chk CHECK (LENGTH(charge_owner_id) = 13 OR LENGTH(charge_owner_id) = 16)
GO

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.charge_masterdata_periods
    DROP CONSTRAINT IF EXISTS resolution_chk
GO
ALTER TABLE {BASIS_DATA_DATABASE_NAME}.charge_masterdata_periods
    ADD CONSTRAINT resolution_chk CHECK (resolution IN ('PT1H', 'P1D', 'P1M'))
GO
