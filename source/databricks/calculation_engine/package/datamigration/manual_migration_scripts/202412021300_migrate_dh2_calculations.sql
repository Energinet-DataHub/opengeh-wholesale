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

-- STEP 0:
-- Manually declare your environment variables.
DECLARE OR REPLACE VARIABLE CATALOG_NAME STRING = "ctl_shres_t_we_001";
DECLARE OR REPLACE VARIABLE WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME STRING = "wholesale_results_internal";
DECLARE OR REPLACE VARIABLE WHOLESALE_INTERNAL_DATABASE_NAME STRING = "wholesale_internal";
DECLARE OR REPLACE VARIABLE SHARED_WHOLESALE_INPUT STRING = "shared_wholesale_input";


-- STEP 1: Delete existing rows across Wholesale's domain
MERGE INTO CATALOG_NAME.WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME.energy e1
USING CATALOG_NAME.WHOLESALE_INTERNAL_DATABASE_NAME.calculations c
ON c.calculation_id <=> e1.calculation_id and c.calculation_version_dh2 is not null
WHEN MATCHED THEN DELETE;

MERGE INTO CATALOG_NAME.WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME.energy_per_brp e2
USING CATALOG_NAME.WHOLESALE_INTERNAL_DATABASE_NAME.calculations c
ON c.calculation_id <=> e2.calculation_id and c.calculation_version_dh2 is not null
WHEN MATCHED THEN DELETE;

MERGE INTO CATALOG_NAME.WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME.energy_per_es e3
USING CATALOG_NAME.WHOLESALE_INTERNAL_DATABASE_NAME.calculations c
ON c.calculation_id <=> e3.calculation_id and c.calculation_version_dh2 is not null
WHEN MATCHED THEN DELETE;

MERGE INTO CATALOG_NAME.WHOLESALE_INTERNAL_DATABASE_NAME.calculation_grid_areas g1
USING CATALOG_NAME.WHOLESALE_INTERNAL_DATABASE_NAME.calculations c
ON c.calculation_id <=> g1.calculation_id and c.calculation_version_dh2 is not null
WHEN MATCHED THEN DELETE;

-- STEP 2: Remove the DH2 calculations from the main table
DELETE FROM CATALOG_NAME.WHOLESALE_INTERNAL_DATABASE_NAME.calculations
WHERE calculation_version_dh2 is not null;

-- STEP 3: Re-migrate each of the tables with calculations from DH2.
-- TODO: Replace "0" with whatever version is given by VOLT later.
INSERT INTO CATALOG_NAME.WHOLESALE_INTERNAL_DATABASE_NAME.calculations 
(calculation_id, calculation_type, calculation_period_start, calculation_period_end, calculation_execution_time_start, calculation_succeeded_time, is_internal_calculation, calculation_version_dh2, calculation_version)
SELECT (calculation_id, calculation_type, calculation_period_start, calculation_period_end, calculation_execution_time_start, calculation_succeeded_time, False, 0, 0) FROM CATALOG_NAME.SHARED_WHOLESALE_INPUT.calculations_view_v1;

-- Result ID for the energy-tables should be unique per: 
-- calculation_id, grid_area_code, balance_responsible_party_id, energy_supplier_id, time_series_type
-- [energy_supplier_id's last 8 digits]-[grid_area_code]-[BRP's last 4 digits]-[time_series_type abbreviated]-[calculation_id's final 12 characters]
INSERT INTO CATALOG_NAME.WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME.energy 
SELECT 
  calculation_id,   
  CONCAT(
    SUBSTRING(energy_supplier_id, -8), '-', 
    grid_area_code, '-', 
    SUBSTRING(balance_responsible_party_id, -4), '-', 
    CASE WHEN time_series_type = 'non_profiled_consumption' THEN 'nonp' 
         WHEN time_series_type = 'production' THEN 'prod' 
         WHEN time_series_type = 'flex_consumption' THEN 'flex'
    ELSE SUBSTRING(time_series_type, 1, 4) END, 
    '-', 
    SUBSTRING(calculation_id, -12)
  ) as result_id,  
  grid_area_code, 
  time_series_type, 
  resolution, 
  time, 
  quantity, 
  quantity_qualities
FROM CATALOG_NAME.SHARED_WHOLESALE_INPUT.calculation_results;

INSERT INTO CATALOG_NAME.WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME.energy_per_brp
SELECT 
  calculation_id,
  CONCAT(
    SUBSTRING(energy_supplier_id, -8), '-', 
    grid_area_code, '-', 
    SUBSTRING(balance_responsible_party_id, -4), '-', 
    CASE WHEN time_series_type = 'non_profiled_consumption' THEN 'nonp' 
         WHEN time_series_type = 'production' THEN 'prod' 
         WHEN time_series_type = 'flex_consumption' THEN 'flex'
    ELSE SUBSTRING(time_series_type, 1, 4) END, 
    '-', 
    SUBSTRING(calculation_id, -12)
  ) as result_id, 
  grid_area_code, 
  balance_responsible_party, 
  time_series_type, 
  resolution,
  time, 
  quantity, 
  quantity_qualities
FROM CATALOG_NAME.SHARED_WHOLESALE_INPUT.calculation_results_energy_per_brp_view_v1;

INSERT INTO CATALOG_NAME.WHOLESALE_RESULTS_INTERNAL_DATABASE_NAME.energy_per_es
SELECT 
  calculation_id, 
  CONCAT(
    SUBSTRING(energy_supplier_id, -8), '-', 
    grid_area_code, '-', 
    SUBSTRING(balance_responsible_party_id, -4), '-', 
    CASE WHEN time_series_type = 'non_profiled_consumption' THEN 'nonp' 
         WHEN time_series_type = 'production' THEN 'prod' 
         WHEN time_series_type = 'flex_consumption' THEN 'flex'
    ELSE SUBSTRING(time_series_type, 1, 4) END, 
    '-', 
    SUBSTRING(calculation_id, -12)
  ) as result_id, 
  grid_area_code, 
  energy_supplier_id, 
  balance_responsible_party, 
  time_series_type, 
  resolution, 
  time, 
  quantity, 
  quantity_qualities
FROM CATALOG_NAME.SHARED_WHOLESALE_INPUT.calculation_results_energy_per_es_view_v1;

INSERT INTO CATALOG_NAME.WHOLESALE_INTERNAL_DATABASE_NAME.calculation_grid_areas
SELECT calculation_id, grid_area_code FROM CATALOG_NAME.SHARED_WHOLESALE_INPUT.calculation_grid_areas_view_v1;
