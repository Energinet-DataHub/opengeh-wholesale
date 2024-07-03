DELETE FROM {HIVE_OUTPUT_DATABASE_NAME}.energy_results
WHERE aggregation_level = 'es_ga'
GO
ALTER TABLE {HIVE_OUTPUT_DATABASE_NAME}.energy_results
    DROP CONSTRAINT IF EXISTS aggregation_level_chk
GO
ALTER TABLE {HIVE_OUTPUT_DATABASE_NAME}.energy_results
    ADD CONSTRAINT aggregation_level_chk CHECK (aggregation_level IN ('grid_area', 'energy_supplier', 'balance_responsible_party'))
GO
