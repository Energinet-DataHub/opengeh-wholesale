ALTER TABLE {HIVE_OUTPUT_DATABASE_NAME}.energy_results
    DROP CONSTRAINT IF EXISTS aggregation_level_chk
GO
UPDATE {HIVE_OUTPUT_DATABASE_NAME}.energy_results
SET aggregation_level = 'energy_supplier'
WHERE aggregation_level = 'es_brp_ga'
GO
ALTER TABLE {HIVE_OUTPUT_DATABASE_NAME}.energy_results
    ADD CONSTRAINT aggregation_level_chk CHECK (aggregation_level IN ('total_ga', 'brp_ga', 'energy_supplier'))
GO
