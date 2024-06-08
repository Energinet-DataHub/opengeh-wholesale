-- These migrations constitutes a rename from EDI specific to general public data model naming
-- The recreation of the renamed views are located in the following migration script files

DROP VIEW IF EXISTS wholesale_edi_results.energy_result_points_per_ga_v1
GO

DROP VIEW IF EXISTS wholesale_edi_results.energy_result_points_per_brp_ga_v1
GO

DROP VIEW IF EXISTS wholesale_edi_results.energy_result_points_per_es_brp_ga_v1
GO

DROP DATABASE IF EXISTS wholesale_edi_results
GO

CREATE DATABASE IF NOT EXISTS {CALCULATION_RESULTS_DATABASE_NAME}
