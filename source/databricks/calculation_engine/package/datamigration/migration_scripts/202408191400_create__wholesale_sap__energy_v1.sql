CREATE VIEW IF NOT EXISTS {CATALOG_NAME}.{WHOLESALE_RESULTS_DATABASE_NAME}.energy_v1 AS
WITH all_energy AS (
    SELECT calculation_id,
           calculation_type,
           'total' AS aggregation_level,
           grid_area_code,
           time_series_type,
           resolution,
           NULL as energy_supplier_id,
           NULL as balance_responsible_party_id,
           NULL as neighbor_grid_area_code,
           time,
           quantity,
           quantity_qualities
    FROM {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.energy
    UNION ALL
    SELECT calculation_id,
           calculation_type,
           'es' AS aggregation_level,
           grid_area_code,
           time_series_type,
           resolution,
           energy_supplier_id,
           balance_responsible_party_id,
           NULL as neighbor_grid_area_code,
           time,
           quantity,
           quantity_qualities
    FROM {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.energy_per_es
    UNION ALL
    SELECT calculation_id,
           calculation_type,
           'brp' AS aggregation_level,
           grid_area_code,
           time_series_type,
           resolution,
           NULL as energy_supplier_id,
           balance_responsible_party_id,
           NULL as neighbor_grid_area_code,
           time,
           quantity,
           quantity_qualities
    FROM {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.energy_per_brp
    UNION ALL
    SELECT calculation_id,
           calculation_type,
           'total' AS aggregation_level,
           grid_area_code,
           time_series_type,
           resolution,
           NULL as energy_supplier_id,
           NULL as balance_responsible_party_id,
           NULL as neighbor_grid_area_code,
           time,
           quantity,
           quantity_qualities
    FROM {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.grid_loss_metering_point_time_series
    UNION ALL
    SELECT calculation_id,
           calculation_type,
           'total' AS aggregation_level,
           grid_area_code,
           time_series_type,
           resolution,
           NULL as energy_supplier_id,
           NULL as balance_responsible_party_id,
           neighbor_grid_area_code,
           time,
           quantity,
           quantity_qualities
    FROM {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.exchange_per_neighbor_grid_area
)
energy AS (
    SELECT calculation_id,
           calculation_type,
           aggregation_level,
           grid_area_code,
           resolution,
           energy_supplier_id,
           balance_responsible_party_id,
           neighbor_grid_area_code,
           time_series_type,
           CASE
               WHEN time_series_type = 'production' THEN 'production'
               WHEN time_series_type = 'non_profiled_consumption' THEN 'consumption'
               WHEN time_series_type = 'flex_consumption' THEN 'consumption'
               WHEN time_series_type = 'net_exchange_per_ga' THEN 'exchange'
               WHEN time_series_type = 'net_exchange_per_neighboring_ga' THEN 'exchange'
               WHEN time_series_type = 'total_consumption' THEN 'consumption'
               WHEN time_series_type = 'grid_loss' THEN NULL
               WHEN time_series_type = 'temp_flex_consumption' THEN 'consumption'
               WHEN time_series_type = 'temp_production' THEN 'production'
               WHEN time_series_type = 'negative_grid_loss' THEN NULL
               WHEN time_series_type = 'positive_grid_loss' THEN NULL
           END as metering_point_type,
           CASE
               WHEN time_series_type = 'production' THEN NULL
               WHEN time_series_type = 'non_profiled_consumption' THEN 'non_profiled'
               WHEN time_series_type = 'flex_consumption' THEN 'flex'
               WHEN time_series_type = 'net_exchange_per_ga' THEN NULL
               WHEN time_series_type = 'net_exchange_per_neighboring_ga' THEN NULL
               WHEN time_series_type = 'total_consumption' THEN NULL
               WHEN time_series_type = 'grid_loss' THEN NULL
               WHEN time_series_type = 'temp_flex_consumption' THEN 'flex'
               WHEN time_series_type = 'temp_production' THEN NULL
               WHEN time_series_type = 'negative_grid_loss' THEN NULL
               WHEN time_series_type = 'positive_grid_loss' THEN NULL
           END as settlement_method,
           time,
           quantity,
           quantity_qualities
           'kWh' as quantity_unit
    FROM all_energy
)