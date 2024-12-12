-- Reusable migration script for DH2 calculations in Wholesale.
-- It works in three general steps:
-- * 0: Verify that all calculations we are about to migrate also have a corresponding calculation_id in it's input.
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
-- Verify the input
CREATE OR REPLACE TEMP VIEW calc_ids_from_dh2_view AS
SELECT calc_ids.calculation_id
FROM (
    SELECT DISTINCT calculation_id 
    FROM ctl_shres_t_we_001.shared_wholesale_input.calculation_results_energy_view_v1
    UNION
    SELECT DISTINCT calculation_id 
    FROM ctl_shres_t_we_001.shared_wholesale_input.calculation_results_energy_per_brp_view_v1
    UNION
    SELECT DISTINCT calculation_id 
    FROM ctl_shres_t_we_001.shared_wholesale_input.calculation_results_energy_per_es_view_v1
    UNION
    SELECT DISTINCT calculation_id 
    FROM ctl_shres_t_we_001.shared_wholesale_input.calculation_grid_areas_view_v1
) AS calc_ids
LEFT JOIN ctl_shres_t_we_001.shared_wholesale_input.calculations_view_v1 AS calc_view
ON calc_ids.calculation_id = calc_view.calculation_id
WHERE calc_view.calculation_id IS NULL;

-- Check the count and throw an error if the count is greater than 0
WITH no_unparanted_calculations_check AS (
    SELECT COUNT(*) AS cnt FROM calc_ids_from_dh2_view
)
SELECT CASE 
    WHEN cnt > 0 THEN RAISE_ERROR("Some calculations are unparented! Please check the input data before running this script!")
    ELSE "All calculations have an entry in the input's calculation table"
END AS UnparentedCalculationIdsCheck
FROM no_unparanted_calculations_check;

-- STEP 1: Delete existing rows across Wholesale's domain
MERGE INTO ctl_shres_t_we_001.wholesale_results_internal.energy e1
USING ctl_shres_t_we_001.wholesale_internal.calculations c
ON c.calculation_id <=> e1.calculation_id and c.calculation_version_dh2 is not null
WHEN MATCHED THEN DELETE;

MERGE INTO ctl_shres_t_we_001.wholesale_results_internal.energy_per_brp e2
USING ctl_shres_t_we_001.wholesale_internal.calculations c
ON c.calculation_id <=> e2.calculation_id and c.calculation_version_dh2 is not null
WHEN MATCHED THEN DELETE;

MERGE INTO ctl_shres_t_we_001.wholesale_results_internal.energy_per_es e3
USING ctl_shres_t_we_001.wholesale_internal.calculations c
ON c.calculation_id <=> e3.calculation_id and c.calculation_version_dh2 is not null
WHEN MATCHED THEN DELETE;

MERGE INTO ctl_shres_t_we_001.wholesale_internal.calculation_grid_areas g1
USING ctl_shres_t_we_001.wholesale_internal.calculations c
ON c.calculation_id <=> g1.calculation_id and c.calculation_version_dh2 is not null
WHEN MATCHED THEN DELETE;

-- STEP 2: Remove the DH2 calculations from the main table
DELETE FROM ctl_shres_t_we_001.wholesale_internal.calculations
WHERE calculation_version_dh2 is not null;

-- STEP 3: Re-migrate each of the tables with calculations from DH2.
-- TODO: Replace "0" with whatever version is given by VOLT later.
INSERT INTO ctl_shres_t_we_001.wholesale_internal.calculations 
  (
    calculation_id,
    calculation_type,
    calculation_period_start,
    calculation_period_end,
    calculation_execution_time_start,
    calculation_succeeded_time,
    is_internal_calculation,
    calculation_version_dh2,
    calculation_version
  )
SELECT
  calculation_id,
  calculation_type,
  calculation_period_start,
  calculation_period_end,
  calculation_period_execution_time_start,
  calculation_period_succeeded_time,
  False as is_internal_calculation,
  0 as calculation_version_dh2,
  0 as calculation_version
FROM
  ctl_shres_t_we_001.shared_wholesale_input.calculations_view_v1;

-- Result ID for the energy-tables should be unique per: 
-- table_name, calculation_id, grid_area_code, balance_responsible_party_id, energy_supplier_id, time_series_type
-- To make it unique per table, each one's result_id follows a different formula:
-- [time_series_type's first 8 characters]-[grid_area_code]-Null-Null-[calculation_id's final 10 characters]P1 
INSERT INTO ctl_shres_t_we_001.wholesale_results_internal.energy 
SELECT 
  calculation_id,   
  CONCAT(
    SUBSTRING(time_series_type, 1, 8), '-',
    LPAD(grid_area_code, 4, '0'), '-', 
    'Null', '-', 
    'Null', '-', 
    SUBSTRING(calculation_id, -10), 'P1'
  ) as result_id,  
  grid_area_code, 
  time_series_type, 
  resolution, 
  time, 
  quantity, 
  quantity_qualities
FROM ctl_shres_t_we_001.shared_wholesale_input.calculation_results_energy_view_v1;

-- ResultID follows:
-- [balance_responsible_party_id's last 8 characters]-[grid_area_code]-[time_series_type's first 4 characters]-[time_series_type's last 4 characters]-[calculation_id's final 10 characters]P2 
INSERT INTO ctl_shres_t_we_001.wholesale_results_internal.energy_per_brp
SELECT 
  calculation_id,
  CONCAT(
    SUBSTRING(balance_responsible_party_id, -8), '-',
    LPAD(grid_area_code, 4, '0'), '-', 
    SUBSTRING(time_series_type, 1, 4), '-',
    SUBSTRING(time_series_type, -4), '-',
    SUBSTRING(calculation_id, -8), 'Ver2'
  ) as result_id,  
  grid_area_code, 
  balance_responsible_party_id, 
  time_series_type, 
  resolution,
  time, 
  quantity, 
  quantity_qualities
FROM ctl_shres_t_we_001.shared_wholesale_input.calculation_results_energy_per_brp_view_v1; 
 
-- ResultID follows:
-- [balance_responsible_party_id's last 8 characters]-[grid_area_code]-[time_series_type's first 4 characters]-[energy_supplier's last 4 characters]-[calculation_id's final 10 characters]P2 
INSERT INTO ctl_shres_t_we_001.wholesale_results_internal.energy_per_es
SELECT 
  calculation_id,
  CONCAT(
    SUBSTRING(balance_responsible_party_id, -8), '-',
    LPAD(grid_area_code, 4, '0'), '-', 
    SUBSTRING(time_series_type, 1, 4), '-',
    SUBSTRING(energy_supplier_id, -4), '-',
    SUBSTRING(calculation_id, -8), 'Ver2'
  ) as result_id,  
  grid_area_code, 
  energy_supplier_id,
  balance_responsible_party_id, 
  time_series_type, 
  resolution,
  time, 
  quantity, 
  quantity_qualities
FROM ctl_shres_t_we_001.shared_wholesale_input.calculation_results_energy_per_es_view_v1; 


INSERT INTO ctl_shres_t_we_001.wholesale_internal.calculation_grid_areas
SELECT calculation_id, grid_area_code FROM ctl_shres_t_we_001.shared_wholesale_input.calculation_grid_areas_view_v1;