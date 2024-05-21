CREATE VIEW {SETTLEMENT_REPORT_DATABASE_NAME}.charge_prices_v1 as
SELECT
  c.calculation_id,
  FIRST_VALUE(c.calculation_type) as calculation_type,
  FIRST_VALUE(cm.charge_type) as charge_type,
  FIRST_VALUE(cm.charge_code) as charge_code,
  FIRST_VALUE(cm.charge_owner_id) as charge_owner_id,
  FIRST_VALUE(cm.resolution) as resolution,
  FIRST_VALUE(cm.is_tax) as is_tax,
  TO_UTC_TIMESTAMP(DATE_TRUNC('day', FROM_UTC_TIMESTAMP(cp.charge_time, 'Europe/Copenhagen')),'Europe/Copenhagen') AS start_date_time,
  ARRAY_SORT(ARRAY_AGG(struct(cp.charge_time, cp.charge_price))) AS prices,
  es_ga.grid_area_code,
  es_ga.energy_supplier_id
FROM {BASIS_DATA_DATABASE_NAME}.charge_masterdata_periods as cm
INNER JOIN {BASIS_DATA_DATABASE_NAME}.charge_price_points as cp ON cm.calculation_id = cp.calculation_id AND cm.charge_key = cp.charge_key
INNER JOIN (
  SELECT distinct mp.calculation_id, charge_key, energy_supplier_id, grid_area_code FROM {BASIS_DATA_DATABASE_NAME}.charge_link_periods AS cl
  INNER JOIN {BASIS_DATA_DATABASE_NAME}.metering_point_periods AS mp ON mp.calculation_id = cl.calculation_id AND mp.metering_point_id = cl.metering_point_id
) AS es_ga ON cm.calculation_id = es_ga.calculation_id AND cm.charge_key = es_ga.charge_key
INNER JOIN {BASIS_DATA_DATABASE_NAME}.calculations AS c ON cm.calculation_id = c.calculation_id
GROUP BY c.calculation_id, cm.charge_key, es_ga.grid_area_code, es_ga.energy_supplier_id, DATE_TRUNC('day', FROM_UTC_TIMESTAMP(cp.charge_time, 'Europe/Copenhagen'))
