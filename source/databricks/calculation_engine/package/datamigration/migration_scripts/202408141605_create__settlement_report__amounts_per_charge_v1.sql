DROP VIEW IF EXISTS  {CATALOG_NAME}.{WHOLESALE_SETTLEMENT_REPORTS_DATABASE_NAME}.amounts_per_charge_v1
GO

CREATE VIEW {CATALOG_NAME}.{WHOLESALE_SETTLEMENT_REPORTS_DATABASE_NAME}.amounts_per_charge_v1 as
SELECT calculation_id,
       result_id,
       grid_area_code,
       energy_supplier_id,
       time,
       resolution,
       metering_point_type,
       settlement_method,
       quantity_unit,
       quantity,
       price,
       amount,
       charge_type,
       charge_code,
       charge_owner_id,
       is_tax
FROM {CATALOG_NAME}.{WHOLESALE_RESULTS_DATABASE_NAME}.amounts_per_charge_v1
