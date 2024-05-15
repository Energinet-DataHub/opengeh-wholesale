DROP VIEW IF EXISTS {SETTLEMENT_REPORT_DATABASE_NAME}.metering_point_periods_v1
GO

CREATE VIEW IF NOT EXISTS {SETTLEMENT_REPORT_DATABASE_NAME}.metering_point_periods_v1
    (calculation_id,
    calculation_type COMMENT '\'BalanceFixing\' | \'Aggregation\' | \'WholesaleFixing\' | \'FirstCorrectionSettlement\' | \'SecondCorrectionSettlement\' | \'ThirdCorrectionSettlement\'',
    metering_point_id,
    from_date,
    to_date COMMENT '<value> | NULL',
    grid_area_code,
    from_grid_area_code COMMENT '<value> | NULL',
    to_grid_area_code COMMENT '<value> | NULL',
    metering_point_type COMMENT '\'production\' | \'consumption\' | \'exchange\' | \'ve_production\' | \'net_production\' | \'supply_to_grid\' | \'consumption_from_grid\' | \'wholesale_services_information\' | \'own_production\' | \'net_from_grid\' | \'net_to_grid\' | \'total_consumption\' | \'electrical_heating\' | \'net_consumption\' | \'effect_settlement\'',
    settlement_method COMMENT '\'non_profiled\' | \'flex\' | NULL',
    energy_supplier_id COMMENT '<value> | NULL')
AS
SELECT c.calculation_id,
       c.calculation_type,
       m.metering_point_id,
       m.from_date,
       m.to_date,
       m.grid_area_code,
       m.from_grid_area_code,
       m.to_grid_area_code,
       m.metering_point_type,
       m.settlement_method,
       m.energy_supplier_id
FROM {BASIS_DATA_DATABASE_NAME}.metering_point_periods as m
INNER JOIN {BASIS_DATA_DATABASE_NAME}.calculations AS c ON c.calculation_id = m.calculation_id
