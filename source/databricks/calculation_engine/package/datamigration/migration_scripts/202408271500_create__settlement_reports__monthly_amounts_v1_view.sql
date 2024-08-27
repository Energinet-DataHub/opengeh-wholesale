DROP VIEW IF EXISTS {CATALOG_NAME}.{WHOLESALE_SETTLEMENT_REPORTS_DATABASE_NAME}.monthly_amounts_v1
GO

CREATE VIEW IF NOT EXISTS {WHOLESALE_SETTLEMENT_REPORTS_DATABASE_NAME}.monthly_amounts_v1 as
SELECT calculation_id,
       calculation_type,
       calculation_version,
       result_id,
       grid_area_code,
       energy_supplier_id,
       time,
       "P1M" as resolution,
       quantity_unit,
       "DKK" as currency,
       amount,
       charge_type,
       charge_code,
       charge_owner_id,
       is_tax
FROM {CATALOG_NAME}.{WHOLESALE_SETTLEMENT_REPORTS_DATABASE_NAME}.monthly_amounts_per_charge_v1

UNION

SELECT calculation_id,
       calculation_type,
       calculation_version,
       result_id,
       grid_area_code,
       energy_supplier_id,
       time,
       "P1M" as resolution,
       NULL as quantity_unit,
       "DKK" as currency,
       amount,
       NULL as charge_type,
       NULL as charge_code,
       charge_owner_id,
       NULL as is_tax
FROM {CATALOG_NAME}.{WHOLESALE_SETTLEMENT_REPORTS_DATABASE_NAME}.total_monthly_amounts_v1
