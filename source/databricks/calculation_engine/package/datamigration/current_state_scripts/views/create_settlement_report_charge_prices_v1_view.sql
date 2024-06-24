CREATE VIEW {SETTLEMENT_REPORT_DATABASE_NAME}.charge_prices_v1 as
SELECT
  c.calculation_id,
  COALESCE(FIRST_VALUE(c.calculation_type), 'ERROR') as calculation_type, -- Hack to make column NOT NULL. Defaults to 'ERROR'.
  COALESCE(FIRST_VALUE(c.version), -1) as calculation_version, -- Hack to make column NOT NULL. Defaults to -1'.
  COALESCE(FIRST_VALUE(cm.charge_type), 'ERROR') as charge_type, -- Hack to make column NOT NULL. Defaults to 'ERROR'.
  COALESCE(FIRST_VALUE(cm.charge_code), 'ERROR') as charge_code, -- Hack to make column NOT NULL. Defaults to 'ERROR'.
  COALESCE(FIRST_VALUE(cm.charge_owner_id), 'ERROR') as charge_owner_id, -- Hack to make column NOT NULL. Defaults to 'ERROR'.
  COALESCE(FIRST_VALUE(cm.resolution), 'ERROR') as resolution, -- Hack to make column NOT NULL. Defaults to 'ERROR'.
  COALESCE(FIRST_VALUE(cm.is_tax), false) as is_tax, -- Hack to make column NOT NULL. Defaults to false.
  COALESCE(TO_UTC_TIMESTAMP(DATE_TRUNC('day', FROM_UTC_TIMESTAMP(cp.charge_time, 'Europe/Copenhagen')),'Europe/Copenhagen'), TIMESTAMP '1970-01-01 00:00:00') AS start_date_time, -- Hack to make
  -- column NOT NULL. Defaults to false.
  ARRAY_SORT(ARRAY_AGG(struct(cp.charge_time AS time, cp.charge_price AS price))) AS price_points,
  es_ga.grid_area_code,
  es_ga.energy_supplier_id
FROM {BASIS_DATA_DATABASE_NAME}.charge_price_information_periods as cm
INNER JOIN {BASIS_DATA_DATABASE_NAME}.charge_price_points as cp ON cm.calculation_id = cp.calculation_id AND cm.charge_key = cp.charge_key
INNER JOIN (
  SELECT distinct mp.calculation_id, charge_key, energy_supplier_id, grid_area_code FROM {BASIS_DATA_DATABASE_NAME}.charge_link_periods AS cl
  INNER JOIN {BASIS_DATA_DATABASE_NAME}.metering_point_periods AS mp ON mp.calculation_id = cl.calculation_id AND mp.metering_point_id = cl.metering_point_id
) AS es_ga ON cm.calculation_id = es_ga.calculation_id AND cm.charge_key = es_ga.charge_key
INNER JOIN {BASIS_DATA_DATABASE_NAME}.calculations AS c ON cm.calculation_id = c.calculation_id
GROUP BY c.calculation_id, cm.charge_key, es_ga.grid_area_code, es_ga.energy_supplier_id, DATE_TRUNC('day', FROM_UTC_TIMESTAMP(cp.charge_time, 'Europe/Copenhagen'))
