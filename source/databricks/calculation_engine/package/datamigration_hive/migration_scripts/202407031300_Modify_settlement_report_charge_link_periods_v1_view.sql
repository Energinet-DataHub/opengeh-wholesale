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
       l.quantity as charge_link_quantity,
    CASE
        WHEN l.from_date > m.from_date THEN l.from_date
        ELSE m.from_date
    END as from_date,
    CASE
        WHEN l.to_date < m.to_date THEN l.to_date
        ELSE m.to_date
    END as to_date,
       m.grid_area_code,
       m.energy_supplier_id,
       cp.is_tax
FROM {BASIS_DATA_DATABASE_NAME}.charge_link_periods AS l
INNER JOIN (
    SELECT DISTINCT
        calculation_id,
        metering_point_id,
        metering_point_type,
        from_date,
        to_date,
        energy_supplier_id,
        grid_area_code
    FROM
        {BASIS_DATA_DATABASE_NAME}.metering_point_periods
) AS m ON m.metering_point_id = l.metering_point_id
    AND m.calculation_id = l.calculation_id
INNER JOIN {BASIS_DATA_DATABASE_NAME}.calculations AS c ON c.calculation_id = l.calculation_id
INNER JOIN {BASIS_DATA_DATABASE_NAME}.charge_price_information_periods AS cp ON cp.calculation_id = l.calculation_id AND cp.charge_key = l.charge_key
WHERE l.from_date < m.to_date AND l.to_date > m.from_date;
