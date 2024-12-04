-- Reusable migration script for DH2 calculations in Wholesale.
-- It works in three general steps:
-- * 1: Delete all rows from all tables with calculation version = 0 in calculations.
-- * 2: Remove the calculations from our main table
-- * 3: Re-insert the new calculations from DH2 into the main table with version = 0
-- * 3: Re-migrate everything from the DH2 calculations input.
--
-- Currently implemented tables: 
-- * wholesale_results_internal
--   * energy_per_brp
--   * energy_per_es
--   * energy
-- * wholesale_internal
--   * calculation_grid_areas
--

-- STEP 1: Delete existing rows across Wholesale's domain
{DATABRICKS-ONLY}MERGE INTO {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.energy e1
{DATABRICKS-ONLY}USING {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations c
{DATABRICKS-ONLY}ON c.calculation_id <=> e1.calculation_id and c.calculation_version_dh2 is not null
{DATABRICKS-ONLY}WHEN MATCHED THEN DELETE 
{DATABRICKS-ONLY}GO 

{DATABRICKS-ONLY}MERGE INTO {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.energy_per_brp e2
{DATABRICKS-ONLY}USING {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations c
{DATABRICKS-ONLY}ON c.calculation_id <=> e2.calculation_id and c.calculation_version_dh2 is not null
{DATABRICKS-ONLY}WHEN MATCHED THEN DELETE 
{DATABRICKS-ONLY}GO

{DATABRICKS-ONLY}MERGE INTO {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.energy_per_es e3
{DATABRICKS-ONLY}USING {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations c
{DATABRICKS-ONLY}ON c.calculation_id <=> e3.calculation_id and c.calculation_version_dh2 is not null
{DATABRICKS-ONLY}WHEN MATCHED THEN DELETE 
{DATABRICKS-ONLY}GO

{DATABRICKS-ONLY}MERGE INTO {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculation_grid_areas g1
{DATABRICKS-ONLY}USING {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations c
{DATABRICKS-ONLY}ON c.calculation_id <=> g1.calculation_id and c.calculation_version_dh2 is not null
{DATABRICKS-ONLY}WHEN MATCHED THEN DELETE 
{DATABRICKS-ONLY}GO

-- STEP 2: Remove the DH2 calculations from the main table
{DATABRICKS-ONLY}DELETE FROM {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations
{DATABRICKS-ONLY}WHERE calculation_version_dh2 is not null
{DATABRICKS-ONLY}GO 

-- STEP 3: Re-migrate each of the tables with calculations from DH2.
{DATABRICKS-ONLY}INSERT INTO {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculations 
{DATABRICKS-ONLY}(calculation_id, calculation_type, calculation_period_start, calculation_period_end, calculation_execution_time_start, calculation_succeeded_time, is_internal_calculation, calculation_version_dh2, calculation_version)
{DATABRICKS-ONLY}SELECT (calculation_id, calculation_type, calculation_period_start, calculation_period_end, calculation_execution_time_start, calculation_succeeded_time, False, 0, 0) FROM {CATALOG_NAME}.{SHARED_WHOLESALE_INPUT}.calculations_view_v1
{DATABRICKS-ONLY}GO 

-- Result ID for the energy-tables should be unique per: 
-- calculation_id, grid_area_code, balance_responsible_party_id, energy_supplier_id, time_series_type
-- [energy_supplier_id's last 8 digits]-[grid_area_code]-[BRP's last 4 digits]-[time_series_type abbreviated]-[calculation_id's final 12 characters]
{DATABRICKS-ONLY}INSERT INTO {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.energy 
{DATABRICKS-ONLY}SELECT 
{DATABRICKS-ONLY}  calculation_id,   
{DATABRICKS-ONLY}  CONCAT(
{DATABRICKS-ONLY}    SUBSTRING(energy_supplier_id, -8), '-', 
{DATABRICKS-ONLY}    grid_area_code, '-', 
{DATABRICKS-ONLY}    SUBSTRING(balance_responsible_party_id, -4), '-', 
{DATABRICKS-ONLY}    CASE WHEN time_series_type = 'non_profiled_consumption' THEN 'nonp' 
{DATABRICKS-ONLY}         WHEN time_series_type = 'production' THEN 'prod' 
{DATABRICKS-ONLY}         WHEN time_series_type = 'flex_consumption' THEN 'flex'
{DATABRICKS-ONLY}    ELSE SUBSTRING(time_series_type, 1, 4) END, 
{DATABRICKS-ONLY}    '-', 
{DATABRICKS-ONLY}    SUBSTRING(calculation_id, -12)
{DATABRICKS-ONLY}  ) as result_id,  
{DATABRICKS-ONLY}  grid_area_code, 
{DATABRICKS-ONLY}  time_series_type, 
{DATABRICKS-ONLY}  resolution, 
{DATABRICKS-ONLY}  time, 
{DATABRICKS-ONLY}  quantity, 
{DATABRICKS-ONLY}  quantity_qualities
{DATABRICKS-ONLY}FROM {CATALOG_NAME}.{SHARED_WHOLESALE_INPUT}.calculation_results
{DATABRICKS-ONLY}GO

{DATABRICKS-ONLY}INSERT INTO {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.energy_per_brp
{DATABRICKS-ONLY}SELECT 
{DATABRICKS-ONLY}  calculation_id,
{DATABRICKS-ONLY}  CONCAT(
{DATABRICKS-ONLY}    SUBSTRING(energy_supplier_id, -8), '-', 
{DATABRICKS-ONLY}    grid_area_code, '-', 
{DATABRICKS-ONLY}    SUBSTRING(balance_responsible_party_id, -4), '-', 
{DATABRICKS-ONLY}    CASE WHEN time_series_type = 'non_profiled_consumption' THEN 'nonp' 
{DATABRICKS-ONLY}         WHEN time_series_type = 'production' THEN 'prod' 
{DATABRICKS-ONLY}         WHEN time_series_type = 'flex_consumption' THEN 'flex'
{DATABRICKS-ONLY}    ELSE SUBSTRING(time_series_type, 1, 4) END, 
{DATABRICKS-ONLY}    '-', 
{DATABRICKS-ONLY}    SUBSTRING(calculation_id, -12)
{DATABRICKS-ONLY}  ) as result_id, 
{DATABRICKS-ONLY}  grid_area_code, 
{DATABRICKS-ONLY}  balance_responsible_party, 
{DATABRICKS-ONLY}  time_series_type, 
{DATABRICKS-ONLY}  resolution,
{DATABRICKS-ONLY}  time, 
{DATABRICKS-ONLY}  quantity, 
{DATABRICKS-ONLY}  quantity_qualities
{DATABRICKS-ONLY}FROM {CATALOG_NAME}.{SHARED_WHOLESALE_INPUT}.calculation_results_energy_per_brp_view_v1
{DATABRICKS-ONLY}GO

{DATABRICKS-ONLY}INSERT INTO {CATALOG_NAME}.{WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME}.energy_per_es
{DATABRICKS-ONLY}SELECT 
{DATABRICKS-ONLY}  calculation_id, 
{DATABRICKS-ONLY}  CONCAT(
{DATABRICKS-ONLY}    SUBSTRING(energy_supplier_id, -8), '-', 
{DATABRICKS-ONLY}    grid_area_code, '-', 
{DATABRICKS-ONLY}    SUBSTRING(balance_responsible_party_id, -4), '-', 
{DATABRICKS-ONLY}    CASE WHEN time_series_type = 'non_profiled_consumption' THEN 'nonp' 
{DATABRICKS-ONLY}         WHEN time_series_type = 'production' THEN 'prod' 
{DATABRICKS-ONLY}         WHEN time_series_type = 'flex_consumption' THEN 'flex'
{DATABRICKS-ONLY}    ELSE SUBSTRING(time_series_type, 1, 4) END, 
{DATABRICKS-ONLY}    '-', 
{DATABRICKS-ONLY}    SUBSTRING(calculation_id, -12)
{DATABRICKS-ONLY}  ) as result_id, 
{DATABRICKS-ONLY}  grid_area_code, 
{DATABRICKS-ONLY}  energy_supplier_id, 
{DATABRICKS-ONLY}  balance_responsible_party, 
{DATABRICKS-ONLY}  time_series_type, 
{DATABRICKS-ONLY}  resolution, 
{DATABRICKS-ONLY}  time, 
{DATABRICKS-ONLY}  quantity, 
{DATABRICKS-ONLY}  quantity_qualities
{DATABRICKS-ONLY}FROM {CATALOG_NAME}.{SHARED_WHOLESALE_INPUT}.calculation_results_energy_per_es_view_v1
{DATABRICKS-ONLY}GO
{DATABRICKS-ONLY}
{DATABRICKS-ONLY}INSERT INTO {CATALOG_NAME}.{WHOLESALE_INTERNAL_DATABASE_NAME}.calculation_grid_areas
{DATABRICKS-ONLY}SELECT calculation_id, grid_area_code FROM {CATALOG_NAME}.{SHARED_WHOLESALE_INPUT}.calculation_grid_areas_view_v1

{TEST-ONLY}SELECT 1;