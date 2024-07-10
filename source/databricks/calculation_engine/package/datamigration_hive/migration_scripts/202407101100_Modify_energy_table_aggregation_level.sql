ALTER TABLE {HIVE_OUTPUT_DATABASE_NAME}.energy_results
    DROP CONSTRAINT IF EXISTS aggregation_level_chk
GO

UPDATE {HIVE_OUTPUT_DATABASE_NAME}.energy_results
SET aggregation_level = CASE
    WHEN aggregation_level = 'total_ga' THEN 'grid_area'
    WHEN aggregation_level = 'brp_ga' THEN 'balance_responsible_party'
    WHEN aggregation_level = 'es_brp_ga' THEN 'energy_supplier'
    ELSE aggregation_level
END
GO

ALTER TABLE {HIVE_OUTPUT_DATABASE_NAME}.energy_results
    ADD CONSTRAINT aggregation_level_chk CHECK (aggregation_level IN ('grid_area', 'balance_responsible_party', 'energy_supplier'))
GO
