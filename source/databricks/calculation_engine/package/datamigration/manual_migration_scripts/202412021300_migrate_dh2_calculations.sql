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
FULL OUTER JOIN ctl_shres_t_we_001.shared_wholesale_input.calculations_view_v1 AS calc_view
ON calc_ids.calculation_id = calc_view.calculation_id
WHERE calc_view.calculation_id IS NULL or calc_ids.calculation_id IS NULL;

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


-- Result ID for the tables we are migrating is faking a MD5 hash based on the same group-by columns used for the UUID.
-- For the energy tables it is calculation_id, grid_area_code, from_grid_area_code, balance_responsible_party_id, energy_supplier_id and time_series_type.
-- If a view is missing one of them, we mark it as NULL.

-- Target table: wholesale_results_internal.energy
with energy_view_with_hash AS (
  select
    *,
    md5(
      CONCAT(
        CASE WHEN calculation_id IS NULL THEN 'null' ELSE calculation_id END,
        CASE WHEN grid_area_code IS NULL THEN 'null' ELSE grid_area_code END, 
        CASE WHEN from_grid_area_code IS NULL THEN 'null' ELSE from_grid_area_code END, 
        CASE WHEN balance_responsible_party_id IS NULL THEN 'null' ELSE balance_responsible_party_id END,
        CASE WHEN energy_supplier_id IS NULL THEN 'null' ELSE energy_supplier_id END,  
        CASE WHEN time_series_type IS NULL THEN 'null' ELSE time_series_type END,
        'energy_per_brp'
      )
    ) as md5_hash_of_result_id_group
  from
    ctl_shres_t_we_001.shared_wholesale_input.calculation_results_energy_per_brp_view_v1
)
INSERT INTO ctl_shres_t_we_001.wholesale_results_internal.energy 
SELECT 
  calculation_id,   
  CONCAT(
    SUBSTRING(md5_hash_of_result_id_group, 1, 8), '-',
    SUBSTRING(md5_hash_of_result_id_group, 9, 4), '-',
    SUBSTRING(md5_hash_of_result_id_group, 13, 4), '-',
    SUBSTRING(md5_hash_of_result_id_group, 17, 4), '-',
    SUBSTRING(md5_hash_of_result_id_group, 21, 12)
  ) as result_id,
  grid_area_code, 
  time_series_type, 
  resolution, 
  time, 
  quantity, 
  quantity_qualities
FROM energy_view_with_hash;


-- Target table: wholesale_results_internal.energy_per_brp
with energy_per_brp_view_with_hash AS (
  select
    *,
    md5(
      CONCAT(
        CASE WHEN calculation_id IS NULL THEN 'null' ELSE calculation_id END,
        CASE WHEN grid_area_code IS NULL THEN 'null' ELSE grid_area_code END, 
        'null', -- from_grid_area_code
        CASE WHEN balance_responsible_party_id IS NULL THEN 'null' ELSE balance_responsible_party_id END,
        'null', -- energy_supplier_id
        CASE WHEN time_series_type IS NULL THEN 'null' ELSE time_series_type END,
        'energy_per_brp'
      )
    ) as md5_hash_of_result_id_group
  from
    ctl_shres_t_we_001.shared_wholesale_input.calculation_results_energy_per_brp_view_v1
)
INSERT INTO ctl_shres_t_we_001.wholesale_results_internal.energy_per_brp
SELECT 
  calculation_id,
  CONCAT(
    SUBSTRING(md5_hash_of_result_id_group, 1, 8), '-',
    SUBSTRING(md5_hash_of_result_id_group, 9, 4), '-',
    SUBSTRING(md5_hash_of_result_id_group, 13, 4), '-',
    SUBSTRING(md5_hash_of_result_id_group, 17, 4), '-',
    SUBSTRING(md5_hash_of_result_id_group, 21, 12)
  ) as result_id,
  grid_area_code, 
  balance_responsible_party_id, 
  time_series_type, 
  resolution,
  time, 
  quantity, 
  quantity_qualities
FROM energy_per_brp_view_with_hash; 
 

-- Target table: wholesale_results_internal.energy_per_es
with energy_per_es_view_with_hash AS (
  select
    *,
    md5(
      CONCAT(
        CASE WHEN calculation_id IS NULL THEN 'null' ELSE calculation_id END,
        CASE WHEN grid_area_code IS NULL THEN 'null' ELSE grid_area_code END,
        'null', -- from_grid_area_code
        CASE WHEN balance_responsible_party_id IS NULL THEN 'null' ELSE balance_responsible_party_id END,
        CASE WHEN energy_supplier_id IS NULL THEN 'null' ELSE energy_supplier_id END,
        CASE WHEN time_series_type IS NULL THEN 'null' ELSE time_series_type END,
        'energy_per_es'
      )
    ) as md5_hash_of_result_id_group
  from
    ctl_shres_t_we_001.shared_wholesale_input.calculation_results_energy_per_es_view_v1
)
INSERT INTO ctl_shres_t_we_001.wholesale_results_internal.energy_per_es
SELECT 
  calculation_id,
  CONCAT(
    SUBSTRING(md5_hash_of_result_id_group, 1, 8), '-',
    SUBSTRING(md5_hash_of_result_id_group, 9, 4), '-',
    SUBSTRING(md5_hash_of_result_id_group, 13, 4), '-',
    SUBSTRING(md5_hash_of_result_id_group, 17, 4), '-',
    SUBSTRING(md5_hash_of_result_id_group, 21, 12)
  ) as result_id,
  grid_area_code, 
  energy_supplier_id,
  balance_responsible_party_id, 
  time_series_type, 
  resolution,
  time, 
  quantity, 
  quantity_qualities
FROM energy_per_es_view_with_hash; 


-- Target table: wholesale_internal.calculation_grid_areas
INSERT INTO ctl_shres_t_we_001.wholesale_internal.calculation_grid_areas
SELECT calculation_id, grid_area_code FROM ctl_shres_t_we_001.shared_wholesale_input.calculation_grid_areas_view_v1;


-- Target table: wholesale_results_internal.amounts_per_charge
with amounts_per_charge_view_with_hash AS (
  select
    *,
    md5(CONCAT(calculation_id,resolution,charge_type,CASE WHEN charge_owner_id IS NULL THEN 'null' ELSE charge_owner_id END,grid_area_code,energy_supplier_id,metering_point_type,CASE WHEN settlement_method IS NULL THEN 'null' ELSE settlement_method END,'amounts_per_charge')) as md5_hash_of_result_id_group
  from
    ctl_shres_t_we_001.shared_wholesale_input.amounts_per_charge_view_v1
)
INSERT INTO ctl_shres_t_we_001.wholesale_results_internal.amounts_per_charge 
SELECT
  calculation_id,
  CONCAT(
    SUBSTRING(md5_hash_of_result_id_group, 1, 8), '-',
    SUBSTRING(md5_hash_of_result_id_group, 9, 4), '-',
    SUBSTRING(md5_hash_of_result_id_group, 13, 4), '-',
    SUBSTRING(md5_hash_of_result_id_group, 17, 4), '-',
    SUBSTRING(md5_hash_of_result_id_group, 21, 12)
  ) as result_id,
  energy_supplier_id,
  quantity_unit,
  time,
  amount,
  is_tax,
  charge_code,
  charge_type,
  charge_owner_id
from
  amounts_per_charge_view_with_hash
  where md5_hash_of_result_id_group is null;
  
 
-- Target table: wholesale_results_internal.monthly_amounts_per_charge
with monthly_amounts_per_charge_view_with_hash AS (
  select
    *,
    md5(CONCAT(calculation_id,charge_type,charge_code, CASE WHEN charge_owner_id IS NULL THEN 'null' ELSE charge_owner_id END, grid_area_code, energy_supplier_id, 'monthly_amounts_per_charge')) as md5_hash_of_result_id_group
  from
    ctl_shres_t_we_001.shared_wholesale_input.monthly_amounts_per_charge_view_v1
)
INSERT INTO ctl_shres_t_we_001.wholesale_results_internal.monthly_amounts_per_charge 
SELECT
  calculation_id,
  CONCAT(
    SUBSTRING(md5_hash_of_result_id_group, 1, 8), '-',
    SUBSTRING(md5_hash_of_result_id_group, 9, 4), '-',
    SUBSTRING(md5_hash_of_result_id_group, 13, 4), '-',
    SUBSTRING(md5_hash_of_result_id_group, 17, 4), '-',
    SUBSTRING(md5_hash_of_result_id_group, 21, 12)
  ) as result_id,
  grid_area_code,
  energy_supplier_id,
  quantity_unit,
  time,
  amount,
  is_tax,
  charge_code,
  charge_type,
  charge_owner_id
from
  monthly_amounts_per_charge_view_with_hash;

 
-- Target table: wholesale_results_internal.monthly_amounts_per_charge
with total_amounts_per_charge_view_with_hash AS (
  select
    *,
    md5(CONCAT(calculation_id, CASE WHEN charge_owner_id IS NULL THEN 'null' ELSE charge_owner_id END, grid_area_code, energy_supplier_id, 'total_amounts_per_charge')) as md5_hash_of_result_id_group
  from
    ctl_shres_t_we_001.shared_wholesale_input.total_amounts_per_charge_view_v1
)
INSERT INTO ctl_shres_t_we_001.wholesale_results_internal.monthly_amounts_per_charge 
SELECT
  calculation_id,
  CONCAT(
    SUBSTRING(md5_hash_of_result_id_group, 1, 8), '-',
    SUBSTRING(md5_hash_of_result_id_group, 9, 4), '-',
    SUBSTRING(md5_hash_of_result_id_group, 13, 4), '-',
    SUBSTRING(md5_hash_of_result_id_group, 17, 4), '-',
    SUBSTRING(md5_hash_of_result_id_group, 21, 12)
  ) as result_id,
  grid_area_code,
  energy_supplier_id,
  time,
  amount,
  charge_owner_id
from
  total_amounts_per_charge_view_with_hash;