MERGE INTO {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.time_series_points AS t
    USING (
    WITH ranked_rows AS (
    SELECT
        *,
        ROW_NUMBER() OVER (PARTITION BY calculation_id, metering_point_id, from_date, to_date ORDER BY energy_supplier_id) AS row_num
    FROM
        (SELECT DISTINCT * FROM (SELECT DISTINCT(*) FROM {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.metering_point_periods))
    WHERE
        calculation_id IN (
            SELECT calculation_id
            FROM (SELECT DISTINCT(*) FROM {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.metering_point_periods)
            GROUP BY calculation_id, metering_point_id, from_date, to_date
            HAVING COUNT(DISTINCT energy_supplier_id) > 1
        )
)
SELECT
    calculation_id, metering_point_id, metering_point_type, settlement_method, grid_area_code, resolution, from_grid_area_code, to_grid_area_code, parent_metering_point_id, energy_supplier_id, balance_responsible_party_id, from_date, to_date
FROM
    ranked_rows
WHERE
    row_num = 1
    ) AS m
ON t.calculation_id = m.calculation_id
   AND t.metering_point_id = m.metering_point_id
   AND t.observation_time >= m.from_date
   AND (m.to_date IS NULL OR t.observation_time < m.to_date)
WHEN MATCHED AND t.metering_point_type IS NULL
              AND t.resolution IS NULL
              AND t.grid_area_code IS NULL
              AND t.energy_supplier_id IS NULL THEN
UPDATE SET
    t.metering_point_type = m.metering_point_type,
    t.resolution = m.resolution,
    t.grid_area_code = m.grid_area_code,
    t.energy_supplier_id = m.energy_supplier_id
