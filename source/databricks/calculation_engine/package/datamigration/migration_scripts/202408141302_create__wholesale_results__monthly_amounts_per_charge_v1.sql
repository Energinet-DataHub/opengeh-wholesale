DROP VIEW IF EXISTS {CATALOG_NAME}.{WHOLESALE_RESULTS_DATABASE_NAME}.monthly_amounts_per_charge_v1
GO

CREATE VIEW {CATALOG_NAME}.{WHOLESALE_RESULTS_DATABASE_NAME}.monthly_amounts_per_charge_v1 AS
SELECT c.calculation_id,
       result_id,
       grid_area_code,
       energy_supplier_id,
       charge_code,
       charge_type,
       charge_owner_id,
       quantity_unit,
       is_tax,
       "DKK" as currency,
       time,
       amount
FROM {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.monthly_amounts_per_charge AS ma
INNER JOIN {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations AS c ON c.calculation_id = ma.calculation_id
