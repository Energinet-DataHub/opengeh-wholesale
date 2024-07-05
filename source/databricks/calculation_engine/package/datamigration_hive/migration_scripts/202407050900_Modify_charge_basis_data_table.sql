ALTER TABLE {{BASIS_DATA_DATABASE_NAME}}.charge_link_periods
    ALTER COLUMN to_date SET NOT NULL
GO

ALTER TABLE {{BASIS_DATA_DATABASE_NAME}}.charge_price_information_periods
    ALTER COLUMN to_date SET NOT NULL
GO