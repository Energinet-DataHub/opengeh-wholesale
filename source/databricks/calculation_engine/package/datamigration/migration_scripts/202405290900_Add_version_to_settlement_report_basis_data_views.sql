DROP VIEW IF EXISTS {SETTLEMENT_REPORT_DATABASE_NAME}.charge_link_periods_v1;
GO

CREATE VIEW IF NOT EXISTS {SETTLEMENT_REPORT_DATABASE_NAME}.charge_link_periods_v1 as
SELECT c.calculation_id,
       c.calculation_type,
       c.version as calculation_version,
       m.metering_point_id,
       m.metering_point_type,
       l.charge_type,
       l.charge_code,
       l.charge_owner_id,
       l.quantity,
       l.from_date,
       l.to_date,
       m.grid_area_code,
       m.energy_supplier_id
FROM {BASIS_DATA_DATABASE_NAME}.charge_link_periods AS l
INNER JOIN {BASIS_DATA_DATABASE_NAME}.metering_point_periods AS m ON m.metering_point_id = l.metering_point_id AND m.calculation_id = l.calculation_id
INNER JOIN {BASIS_DATA_DATABASE_NAME}.calculations AS c ON c.calculation_id = l.calculation_id
GO

DROP VIEW IF EXISTS {SETTLEMENT_REPORT_DATABASE_NAME}.charge_prices_v1;
GO

CREATE VIEW {SETTLEMENT_REPORT_DATABASE_NAME}.charge_prices_v1 as
SELECT
  c.calculation_id,
  FIRST_VALUE(c.calculation_type) as calculation_type,
  FIRST_VALUE(c.version) as calculation_version,
  FIRST_VALUE(cm.charge_type) as charge_type,
  FIRST_VALUE(cm.charge_code) as charge_code,
  FIRST_VALUE(cm.charge_owner_id) as charge_owner_id,
  FIRST_VALUE(cm.resolution) as resolution,
  FIRST_VALUE(cm.is_tax) as is_tax,
  TO_UTC_TIMESTAMP(DATE_TRUNC('day', FROM_UTC_TIMESTAMP(cp.charge_time, 'Europe/Copenhagen')),'Europe/Copenhagen') AS start_date_time,
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
GO

DROP VIEW IF EXISTS {SETTLEMENT_REPORT_DATABASE_NAME}.metering_point_periods_v1;
GO

CREATE VIEW IF NOT EXISTS {SETTLEMENT_REPORT_DATABASE_NAME}.metering_point_periods_v1 AS
SELECT c.calculation_id,
       c.calculation_type,
       c.version as calculation_version,
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
GO

DROP VIEW IF EXISTS {SETTLEMENT_REPORT_DATABASE_NAME}.metering_point_time_series_v1;
GO

CREATE VIEW IF NOT EXISTS {SETTLEMENT_REPORT_DATABASE_NAME}.metering_point_time_series_v1 AS
SELECT c.calculation_id,
       FIRST(c.calculation_type) as calculation_type,
       FIRST(c.version) as calculation_version,
       m.metering_point_id,
       m.metering_point_type,
       m.resolution,
       m.grid_area_code,
       m.energy_supplier_id,
       TO_UTC_TIMESTAMP(DATE_TRUNC('day', FROM_UTC_TIMESTAMP(t.observation_time, 'Europe/Copenhagen')),'Europe/Copenhagen') AS start_date_time,
       ARRAY_SORT(ARRAY_AGG(struct(t.observation_time, t.quantity)))                  AS quantities
FROM {BASIS_DATA_DATABASE_NAME}.metering_point_periods AS m
         INNER JOIN {BASIS_DATA_DATABASE_NAME}.calculations AS c ON c.calculation_id = m.calculation_id
         INNER JOIN {BASIS_DATA_DATABASE_NAME}.time_series_points AS t ON m.metering_point_id = t.metering_point_id AND m.calculation_id = t
             .calculation_id
WHERE t.observation_time >= m.from_date
  AND (m.to_date IS NULL OR t.observation_time < m.to_date)
GROUP BY c.calculation_id, m.metering_point_id, m.metering_point_type, DATE_TRUNC('day', FROM_UTC_TIMESTAMP(t.observation_time, 'Europe/Copenhagen')), m.resolution, m.grid_area_code, m.energy_supplier_id

