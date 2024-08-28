INSERT INTO {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.time_series_points
(
    calculation_id,
    calculation_type,
    calculation_version,
    is_internal_calculation,
    metering_point_id,
    metering_point_type,
    resolution,
    grid_area_code,
    energy_supplier_id,
    observation_time,
    quantity,
    quality
)
SELECT 
    t.calculation_id,
    c.calculation_type,
    c.calculation_version,
    c.is_internal_calculation,
    m.metering_point_id,
    m.metering_point_type,
    m.resolution,
    m.grid_area_code,
    m.energy_supplier_id,
    t.observation_time,
    t.quantity,
    t.quality
FROM {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.metering_point_periods AS m
INNER JOIN {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations AS c ON c.calculation_id = m.calculation_id
INNER JOIN {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.time_series_points_old AS t ON m.metering_point_id = t.metering_point_id AND m.calculation_id = t.calculation_id
