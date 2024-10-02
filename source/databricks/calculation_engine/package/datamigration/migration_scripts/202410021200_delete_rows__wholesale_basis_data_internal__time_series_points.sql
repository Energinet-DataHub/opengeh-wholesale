DELETE FROM {CATALOG_NAME}.{WHOLESALE_BASIS_DATA_INTERNAL_DATABASE_NAME}.time_series_points
WHERE calculation_id IN (
    SELECT calculation_id
    FROM {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations
    WHERE calculation_type IN ('balance_fixing', 'aggregation')
)
AND metering_point_id IN (
    SELECT metering_point_id
    FROM {CATALOG_NAME}.{SHARED_WHOLESALE_INPUT_DATABASE_NAME}.metering_point_periods_view_v1
    WHERE type IN ('D01', 'D05', 'D06', 'D07', 'D08', 'D09', 'D10', 'D11', 'D12', 'D14', 'D15', 'D19')
)
