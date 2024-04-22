ALTER TABLE {BASIS_DATA_DATABASE_NAME}.charge_link_periods
    DROP CONSTRAINT IF EXISTS calculation_id_chk
GO
ALTER TABLE {BASIS_DATA_DATABASE_NAME}.charge_link_periods
    ADD CONSTRAINT calculation_id_chk CHECK (LENGTH(calculation_id) = 36)
GO

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.charge_link_periods
    DROP CONSTRAINT IF EXISTS charge_type_chk
GO
ALTER TABLE {BASIS_DATA_DATABASE_NAME}.charge_link_periods
    ADD CONSTRAINT charge_type_chk CHECK (charge_type IN ('subscription', 'fee', 'tariff'))
GO

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.charge_link_periods
    DROP CONSTRAINT IF EXISTS charge_owner_id_chk
GO
-- Length is 16 when EIC and 13 when GLN
ALTER TABLE {BASIS_DATA_DATABASE_NAME}.charge_link_periods
    ADD CONSTRAINT charge_owner_id_chk CHECK (LENGTH(charge_owner_id) = 13 OR LENGTH(charge_owner_id) = 16)
GO

ALTER TABLE {BASIS_DATA_DATABASE_NAME}.charge_link_periods
    DROP CONSTRAINT IF EXISTS metering_point_id_chk
GO
ALTER TABLE {BASIS_DATA_DATABASE_NAME}.charge_link_periods
    ADD CONSTRAINT metering_point_id_chk CHECK  (LENGTH(metering_point_id) = 18)
GO
