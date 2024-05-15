CREATE VIEW IF NOT EXISTS {SETTLEMENT_REPORT_DATABASE_NAME}.charge_prices_v1 as
SELECT c.calculation_id,
       cm.charge_type,
       cm.charge_owner_id,
       cm.charge_code,
       cm.resolution,
       cm.is_tax,
       DATE_TRUNC('day', FROM_UTC_TIMESTAMP(t.observation_time, 'Europe/Copenhagen')) AS observation_day_local,
       TO_UTC_TIMESTAMP(observation_day_local, 'Europe/Copenhagen') AS start_date_time,
       ARRAY_SORT(ARRAY_AGG(struct(cp.charge_time, cp.charge_price))) AS prices
       -- For filtering:
       c.calculation_type,
       m.grid_area_code,
       m.energy_supplier_id
FROM {BASIS_DATA_DATABASE_NAME}.charge_masterdata_periods AS cm
INNER JOIN {BASIS_DATA_DATABASE_NAME}.charge_price_points AS cp AS cm ON cm.calculation_id = cp.calculation_id AND cp.charge_key = cm.charge_key
INNER JOIN (
    SELECT distinct(charge_key, energy_supplier, grid_area_code)
    FROM {BASIS_DATA_DATABASE_NAME}.charge_link_periods AS cl
    INNER JOIN {BASIS_DATA_DATABASE_NAME}.metering_point_periods AS m ON cm.calculation_id = t.calculation_id AND cl.metering_point_id = m.metering_point_id
    ) as ck_es_ga ON ck_es_ga.calculation_id = cm.charge_key AND ck_es_ga.charge_key = cm.charge_key
INNER JOIN {BASIS_DATA_DATABASE_NAME}.calculations AS c ON cm.calculation_id = l.calculation_id
GROUP BY c.calculation_id, cm.charge_key, observation_day_local, m.energy_supplier_id





charge_key | grid_area_code | energy_supplier

charge_owner | grid_area_code | energy_supplier


--
-- -- For netvirksomhed:
-- SELECT * FROM charge_prices_v1
-- WHERE grid_area_code = '804'
--
-- -- For system operator:
-- SELECT * FROM charge_prices_v1
-- WHERE grid_area_code = '802'
-- AND charge_owner_id = so_gln
--
-- -- For energy supplier:
-- SELECT * FROM charge_prices_v1
-- WHERE grid_area_code = '802'
-- AND energy_supplier = es_gln
