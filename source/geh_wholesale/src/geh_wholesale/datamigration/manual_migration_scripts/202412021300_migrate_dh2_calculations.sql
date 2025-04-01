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
--   * amounts_per_charge
--   * monthly_amounts_per_charge
--   * total_monthly_amounts
-- * wholesale_internal
--   * calculation_grid_areas

-- NOTE:
-- When using this script, please pay attention to the two/three outlier IDs that you have to identify.
-- Khatozen knows what this is about, but in brief, there are a few transactions which have multiple versions where both versions should be allowed through.
-- But there are also one set of transactions which overlap in weird ways for their period start/end, where only the newest one should be manually taken.
-- The scripts to help identify them, are at the end of STEP 0.



-- STEP 0: 
-- Verify the input
CREATE OR REPLACE TEMP VIEW calc_ids_from_dh2_view AS
SELECT calc_ids.calculation_id
FROM (
    SELECT DISTINCT calculation_id 
    FROM ctl_shres_p_we_001.shared_wholesale_input.calculation_results_energy_view_v1
    UNION
    SELECT DISTINCT calculation_id 
    FROM ctl_shres_p_we_001.shared_wholesale_input.calculation_results_energy_per_brp_view_v1
    UNION
    SELECT DISTINCT calculation_id 
    FROM ctl_shres_p_we_001.shared_wholesale_input.calculation_results_energy_per_es_view_v1
    UNION
    SELECT DISTINCT calculation_id 
    FROM ctl_shres_p_we_001.shared_wholesale_input.calculation_grid_areas_view_v1
    UNION
    SELECT DISTINCT calculation_id 
    FROM ctl_shres_p_we_001.shared_wholesale_input.amounts_per_charge_view_v1
    UNION
    SELECT DISTINCT calculation_id 
    FROM ctl_shres_p_we_001.shared_wholesale_input.monthly_amounts_per_charge_view_v1
    UNION
    SELECT DISTINCT calculation_id 
    FROM ctl_shres_p_we_001.shared_wholesale_input.total_monthly_amounts_view_v1
) AS calc_ids
FULL OUTER JOIN ctl_shres_p_we_001.shared_wholesale_input.calculations_view_v1 AS calc_view
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

-- The following temp views help us identify multiple versions from DH2.
-- We want to only keep the newest ones, except for a specific outlier.
CREATE OR REPLACE TEMP VIEW calc_versions AS 
SELECT c.*, r.resolution, g.grid_area_code, row_number() OVER (PARTITION BY calculation_type, calculation_period_start, calculation_period_end, resolution, grid_area_code ORDER BY calculation_period_succeeded_time) AS row_num
FROM ctl_shres_p_we_001.shared_wholesale_input.calculations_view_v1 c
INNER JOIN ctl_shres_p_we_001.shared_wholesale_input.calculation_grid_areas_view_v1 g ON c.calculation_id = g.calculation_id 
INNER JOIN ( SELECT DISTINCT calculation_id, resolution FROM (
    SELECT DISTINCT calculation_id, resolution from ctl_shres_p_we_001.shared_wholesale_input.calculation_results_energy_view_v1
    UNION
    SELECT DISTINCT calculation_id, resolution from ctl_shres_p_we_001.shared_wholesale_input.calculation_results_energy_per_brp_view_v1
    UNION
    SELECT DISTINCT calculation_id, resolution from ctl_shres_p_we_001.shared_wholesale_input.calculation_results_energy_per_es_view_v1
    UNION
    SELECT DISTINCT calculation_id, resolution from ctl_shres_p_we_001.shared_wholesale_input.amounts_per_charge_view_v1
    UNION
    SELECT DISTINCT calculation_id, 'P1M' as resolution from ctl_shres_p_we_001.shared_wholesale_input.monthly_amounts_per_charge_view_v1
    )
) r on c.calculation_id = r.calculation_id; 

-- This view is used throughout to filter out "bad" transactions from the migration.
-- It leaves only the newest calculation versions, except for 2 outlier-cases which are special-cased to be kept or excluded specifically.
-- Those two are covered in the 2 queries below this one, which are for diagnostics primarily.
CREATE OR REPLACE TEMP VIEW only_newest_calc_versions AS 
SELECT c.* FROM calc_versions c
INNER JOIN (
  SELECT calculation_type, calculation_period_start, calculation_period_end, resolution, grid_area_code, max(row_num) as max_row_num FROM calc_versions
  GROUP BY calculation_type, calculation_period_start, calculation_period_end, resolution, grid_area_code
) m ON c.calculation_type = m.calculation_type 
  and c.calculation_period_start = m.calculation_period_start 
  and c.calculation_period_end = m.calculation_period_end 
  and c.resolution = m.resolution 
  and c.grid_area_code = m.grid_area_code 
WHERE (max_row_num = row_num or 
calculation_id in ('4ee428d7-a52e-da21-3733-789d576d8f1c')) -- The two outliers, which are both kept.
 and (calculation_id not in ('24a14780-7748-2875-c414-76abf651b288', '590af23e-ad6b-da07-e50b-f5e9221815e8')); -- The transactions with overlap that are mis-registered above. (see query 2 steps down) 

-- This query contains the ~24 transactions that Khatozen should manually check to find the one special-case that we want to keep.
select t.* from calc_versions t
inner join (select * from only_newest_calc_versions where row_num > 1) u on  
t.calculation_type = u.calculation_type AND 	
t.calculation_period_start = u.calculation_period_start AND 	
t.calculation_period_end = u.calculation_period_end AND 	 
t.resolution = u.resolution AND 	
t.grid_area_code = u.grid_area_code  
order by t.calculation_type, t.calculation_period_start, t.calculation_period_end, t.resolution, t.grid_area_code;

-- This query contains a check for transactions with overlap that is not handled by the above.
-- Here the transactions with a weird overlap should be taken and allowed through.
-- If a row is present here, it means that A was fully covered by B, and that B is likely the correct one. (Check with Khatozen first)
SELECT * FROM (
select b.calculation_id as B, a.calculation_id as A, a.calculation_period_start as A_start, a.calculation_period_end as A_end, b.calculation_period_start as B_start, b.calculation_period_end as B_end, a.calculation_type, a.resolution, a.grid_area_code, a.row_num as A_row_num, b.row_num as B_row_num
FROM calc_versions a
INNER JOIN calc_versions b 
ON a.calculation_type <=> b.calculation_type and a.resolution <=> b.resolution and a.grid_area_code <=> b.grid_area_code and 
  (b.calculation_period_start between a.calculation_period_start and a.calculation_period_end) and 
  (b.calculation_period_end between a.calculation_period_start and a.calculation_period_end) and
  ( a.calculation_period_start != b.calculation_period_start or a.calculation_period_end != b.calculation_period_end)
WHERE a.calculation_id != b.calculation_id
ORDER BY a.calculation_period_start
);



-- STEP 1: Delete existing rows across Wholesale's domain
MERGE INTO ctl_shres_p_we_001.wholesale_results_internal.energy e1
USING ctl_shres_p_we_001.wholesale_internal.calculations c
ON c.calculation_id <=> e1.calculation_id and c.calculation_version_dh2 is not null
WHEN MATCHED THEN DELETE;

MERGE INTO ctl_shres_p_we_001.wholesale_results_internal.energy_per_brp e2
USING ctl_shres_p_we_001.wholesale_internal.calculations c
ON c.calculation_id <=> e2.calculation_id and c.calculation_version_dh2 is not null
WHEN MATCHED THEN DELETE;

MERGE INTO ctl_shres_p_we_001.wholesale_results_internal.energy_per_es e3
USING ctl_shres_p_we_001.wholesale_internal.calculations c
ON c.calculation_id <=> e3.calculation_id and c.calculation_version_dh2 is not null
WHEN MATCHED THEN DELETE;

MERGE INTO ctl_shres_p_we_001.wholesale_internal.calculation_grid_areas g1
USING ctl_shres_p_we_001.wholesale_internal.calculations c
ON c.calculation_id <=> g1.calculation_id and c.calculation_version_dh2 is not null
WHEN MATCHED THEN DELETE;

MERGE INTO ctl_shres_p_we_001.wholesale_results_internal.amounts_per_charge a1
USING ctl_shres_p_we_001.wholesale_internal.calculations c
ON c.calculation_id <=> a1.calculation_id and c.calculation_version_dh2 is not null
WHEN MATCHED THEN DELETE;

MERGE INTO ctl_shres_p_we_001.wholesale_results_internal.monthly_amounts_per_charge a2
USING ctl_shres_p_we_001.wholesale_internal.calculations c
ON c.calculation_id <=> a2.calculation_id and c.calculation_version_dh2 is not null
WHEN MATCHED THEN DELETE;

MERGE INTO ctl_shres_p_we_001.wholesale_results_internal.total_monthly_amounts a3
USING ctl_shres_p_we_001.wholesale_internal.calculations c
ON c.calculation_id <=> a3.calculation_id and c.calculation_version_dh2 is not null
WHEN MATCHED THEN DELETE;



-- STEP 2: Remove the DH2 calculations from the main table
DELETE FROM ctl_shres_p_we_001.wholesale_internal.calculations
WHERE calculation_version_dh2 is not null;



-- STEP 3: Re-migrate each of the tables with calculations from DH2.
INSERT INTO ctl_shres_p_we_001.wholesale_internal.calculations 
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
  c.calculation_id,
  c.calculation_type,
  c.calculation_period_start,
  c.calculation_period_end,
  c.calculation_period_execution_time_start,
  c.calculation_period_succeeded_time,
  False AS is_internal_calculation,
  r.version AS calculation_version_dh2,
  r.version AS calculation_version
FROM ctl_shres_p_we_001.shared_wholesale_input.calculations_view_v1 c
INNER JOIN (SELECT DISTINCT calculation_id, version FROM ctl_shres_p_we_001.shared_wholesale_input.calculation_results) r on c.calculation_id = r.calculation_id
INNER JOIN (SELECT DISTINCT calculation_id FROM only_newest_calc_versions) n on c.calculation_id = n.calculation_id;


-- Result ID for the tables we are migrating is faking a MD5 hash based on the same group-by columns used for the UUID.
-- For the energy tables it is calculation_id, grid_area_code, from_grid_area_code, balance_responsible_party_id, energy_supplier_id and time_series_type.
-- If a view is missing one of them, we mark it as NULL.

-- Target table: wholesale_results_internal.energy
WITH energy_view_with_hash AS (
  SELECT
    *,
    md5(
      CONCAT(
        CASE WHEN calculation_id IS NULL THEN 'null' ELSE calculation_id END,
        CASE WHEN grid_area_code IS NULL THEN 'null' ELSE grid_area_code END, 
        'null', -- from_grid_area_code
        'null', -- balance_responsible_party_id
        'null', -- energy_supplier_id
        CASE WHEN time_series_type IS NULL THEN 'null' ELSE time_series_type END,
        'energy_per_brp'
      )
    ) AS md5_hash_of_result_id_group
  FROM
    ctl_shres_p_we_001.shared_wholesale_input.calculation_results_energy_view_v1
)
INSERT INTO ctl_shres_p_we_001.wholesale_results_internal.energy 
SELECT 
  c.calculation_id,   
  CONCAT(
    SUBSTRING(md5_hash_of_result_id_group, 1, 8), '-',
    SUBSTRING(md5_hash_of_result_id_group, 9, 4), '-',
    SUBSTRING(md5_hash_of_result_id_group, 13, 4), '-',
    SUBSTRING(md5_hash_of_result_id_group, 17, 4), '-',
    SUBSTRING(md5_hash_of_result_id_group, 21, 12)
  ) AS result_id,
  c.grid_area_code, 
  c.time_series_type, 
  c.resolution, 
  c.time, 
  c.quantity, 
  c.quantity_qualities
FROM energy_view_with_hash c
INNER JOIN (SELECT DISTINCT calculation_id FROM only_newest_calc_versions) n on c.calculation_id = n.calculation_id;


-- Target table: wholesale_results_internal.energy_per_brp
WITH energy_per_brp_view_with_hash AS (
  SELECT
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
    ) AS md5_hash_of_result_id_group
  FROM
    ctl_shres_p_we_001.shared_wholesale_input.calculation_results_energy_per_brp_view_v1
)
INSERT INTO ctl_shres_p_we_001.wholesale_results_internal.energy_per_brp
SELECT 
  c.calculation_id,
  CONCAT(
    SUBSTRING(md5_hash_of_result_id_group, 1, 8), '-',
    SUBSTRING(md5_hash_of_result_id_group, 9, 4), '-',
    SUBSTRING(md5_hash_of_result_id_group, 13, 4), '-',
    SUBSTRING(md5_hash_of_result_id_group, 17, 4), '-',
    SUBSTRING(md5_hash_of_result_id_group, 21, 12)
  ) AS result_id,
  c.grid_area_code, 
  c.balance_responsible_party_id, 
  c.time_series_type, 
  c.resolution,
  c.time, 
  c.quantity, 
  c.quantity_qualities
FROM energy_per_brp_view_with_hash c
INNER JOIN (SELECT DISTINCT calculation_id FROM only_newest_calc_versions) n on c.calculation_id = n.calculation_id;
 

-- Target table: wholesale_results_internal.energy_per_es
WITH energy_per_es_view_with_hash AS (
  SELECT
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
    ) AS md5_hash_of_result_id_group
  FROM
    ctl_shres_p_we_001.shared_wholesale_input.calculation_results_energy_per_es_view_v1
)
INSERT INTO ctl_shres_p_we_001.wholesale_results_internal.energy_per_es
SELECT 
  c.calculation_id,
  CONCAT(
    SUBSTRING(md5_hash_of_result_id_group, 1, 8), '-',
    SUBSTRING(md5_hash_of_result_id_group, 9, 4), '-',
    SUBSTRING(md5_hash_of_result_id_group, 13, 4), '-',
    SUBSTRING(md5_hash_of_result_id_group, 17, 4), '-',
    SUBSTRING(md5_hash_of_result_id_group, 21, 12)
  ) as result_id,
  c.grid_area_code, 
  c.energy_supplier_id,
  c.balance_responsible_party_id, 
  c.time_series_type, 
  c.resolution,
  c.time, 
  c.quantity, 
  c.quantity_qualities
FROM energy_per_es_view_with_hash c
INNER JOIN (SELECT DISTINCT calculation_id FROM only_newest_calc_versions) n on c.calculation_id = n.calculation_id;


-- Target table: wholesale_internal.calculation_grid_areas
INSERT INTO ctl_shres_p_we_001.wholesale_internal.calculation_grid_areas
SELECT c.calculation_id, grid_area_code FROM ctl_shres_p_we_001.shared_wholesale_input.calculation_grid_areas_view_v1 c
INNER JOIN (SELECT DISTINCT calculation_id FROM only_newest_calc_versions) n on c.calculation_id = n.calculation_id;


-- Target table: wholesale_results_internal.amounts_per_charge
WITH amounts_per_charge_view_with_hash AS (
  SELECT
    *,
    md5(
      CONCAT(
        CASE WHEN calculation_id IS NULL THEN 'null' ELSE calculation_id END,
        CASE WHEN resolution IS NULL THEN 'null' ELSE resolution END,
        CASE WHEN charge_type IS NULL THEN 'null' ELSE charge_type END,
        CASE WHEN charge_owner_id IS NULL THEN 'null' ELSE charge_owner_id END,
        CASE WHEN grid_area_code IS NULL THEN 'null' ELSE grid_area_code END,
        CASE WHEN energy_supplier_id IS NULL THEN 'null' ELSE energy_supplier_id END,
        CASE WHEN metering_point_type IS NULL THEN 'null' ELSE metering_point_type END,
        CASE WHEN settlement_method IS NULL THEN 'null' ELSE settlement_method END,
        'amounts_per_charge'
      )
    ) AS md5_hash_of_result_id_group
  FROM
    ctl_shres_p_we_001.shared_wholesale_input.amounts_per_charge_view_v1
  WHERE calculation_id not in ('4ee428d7-a52e-da21-3733-789d576d8f1c')
)
INSERT INTO ctl_shres_p_we_001.wholesale_results_internal.amounts_per_charge 
SELECT
  c.calculation_id,
  CONCAT(
    SUBSTRING(md5_hash_of_result_id_group, 1, 8), '-',
    SUBSTRING(md5_hash_of_result_id_group, 9, 4), '-',
    SUBSTRING(md5_hash_of_result_id_group, 13, 4), '-',
    SUBSTRING(md5_hash_of_result_id_group, 17, 4), '-',
    SUBSTRING(md5_hash_of_result_id_group, 21, 12)
  ) as result_id,
  c.grid_area_code,
  c.energy_supplier_id,
  c.quantity,
  c.quantity_unit,
  c.quantity_qualities,
  c.time,
  c.resolution,
  c.metering_point_type,
  c.settlement_method,
  c.price,
  c.amount,
  c.is_tax,
  c.charge_code,
  c.charge_type,
  c.charge_owner_id
FROM
  amounts_per_charge_view_with_hash c
INNER JOIN (SELECT DISTINCT calculation_id FROM only_newest_calc_versions) n on c.calculation_id = n.calculation_id;
  

-- Target table: wholesale_results_internal.monthly_amounts_per_charge
WITH monthly_amounts_per_charge_view_with_hash AS (
  SELECT
    *,
    md5(
      CONCAT(
        CASE WHEN calculation_id IS NULL THEN 'null' ELSE calculation_id end,
        CASE WHEN charge_type IS NULL THEN 'null' ELSE charge_type end,
        CASE WHEN charge_code IS NULL THEN 'null' ELSE charge_code end,
        CASE WHEN charge_owner_id IS NULL THEN 'null' ELSE charge_owner_id end,
        CASE WHEN grid_area_code IS NULL THEN 'null' ELSE grid_area_code end,
        CASE WHEN energy_supplier_id IS NULL THEN 'null' ELSE energy_supplier_id end,
        'monthly_amounts_per_charge'
      )
    ) AS md5_hash_of_result_id_group
  FROM
    ctl_shres_p_we_001.shared_wholesale_input.monthly_amounts_per_charge_view_v1
  WHERE calculation_id not in ('4ee428d7-a52e-da21-3733-789d576d8f1c')
)
INSERT INTO ctl_shres_p_we_001.wholesale_results_internal.monthly_amounts_per_charge 
SELECT
  c.calculation_id,
  CONCAT(
    SUBSTRING(md5_hash_of_result_id_group, 1, 8), '-',
    SUBSTRING(md5_hash_of_result_id_group, 9, 4), '-',
    SUBSTRING(md5_hash_of_result_id_group, 13, 4), '-',
    SUBSTRING(md5_hash_of_result_id_group, 17, 4), '-',
    SUBSTRING(md5_hash_of_result_id_group, 21, 12)
  ) as result_id,
  c.grid_area_code,
  c.energy_supplier_id,
  c.quantity_unit,
  c.time,
  c.amount,
  c.is_tax,
  c.charge_code,
  c.charge_type,
  c.charge_owner_id
FROM
  monthly_amounts_per_charge_view_with_hash c
INNER JOIN (SELECT DISTINCT calculation_id FROM only_newest_calc_versions) n on c.calculation_id = n.calculation_id;


-- Target table: wholesale_results_internal.monthly_amounts_per_charge
WITH total_amounts_per_charge_view_with_hash AS (
  SELECT
    *,
    md5(
      CONCAT(
        CASE WHEN calculation_id IS NULL THEN 'null' ELSE calculation_id END,
        CASE WHEN charge_owner_id IS NULL THEN 'null' ELSE charge_owner_id END,
        CASE WHEN grid_area_code IS NULL THEN 'null' ELSE grid_area_code END,
        CASE WHEN energy_supplier_id IS NULL THEN 'null' ELSE energy_supplier_id END,
        'total_amounts_per_charge'
      )
    ) as md5_hash_of_result_id_group
  FROM
    ctl_shres_p_we_001.shared_wholesale_input.total_monthly_amounts_view_v1
  WHERE calculation_id not in ('4ee428d7-a52e-da21-3733-789d576d8f1c')
)
INSERT INTO ctl_shres_p_we_001.wholesale_results_internal.total_monthly_amounts 
SELECT
  c.calculation_id,
  CONCAT(
    SUBSTRING(md5_hash_of_result_id_group, 1, 8), '-',
    SUBSTRING(md5_hash_of_result_id_group, 9, 4), '-',
    SUBSTRING(md5_hash_of_result_id_group, 13, 4), '-',
    SUBSTRING(md5_hash_of_result_id_group, 17, 4), '-',
    SUBSTRING(md5_hash_of_result_id_group, 21, 12)
  ) as result_id,
  c.grid_area_code,
  c.energy_supplier_id,
  c.time,
  c.amount,
  c.charge_owner_id
FROM
  total_amounts_per_charge_view_with_hash c
INNER JOIN (SELECT DISTINCT calculation_id FROM only_newest_calc_versions) n on c.calculation_id = n.calculation_id;