--
-- On the 1st of November 2023 a bad migration messed with the
-- energy results quantity values.
-- This migration tries to fix the problem by
-- * Preserving the data added after the bad migration
-- * Restoring the state of the data added before the bad migration
--
-- The algorithm has the following problem:
-- It does not now the exact point in time of deployment of the bad
-- migration in each environment.
--

-- Step 1: Create a backup of the data added after the bad migration
CREATE TABLE IF NOT EXISTS {OUTPUT_DATABASE_NAME}.energy_results_backup
USING delta
AS (
    SELECT *
    FROM {OUTPUT_DATABASE_NAME}.energy_results
    WHERE calculation_execution_time_start > '2023-11-01 10:00:00'
)
GO

-- Step 2: Restore the delta table to the state before the bad migration
RESTORE {OUTPUT_DATABASE_NAME}.energy_results TO TIMESTAMP AS OF '2023-11-01 10:00:00'
GO

-- Step 3: Re-add the backed-up data to the restored table
INSERT INTO {OUTPUT_DATABASE_NAME}.energy_results
SELECT *
FROM {OUTPUT_DATABASE_NAME}.energy_results_backup
GO

-- Step 4: Delete the backup table after the restore and data re-addition
DROP TABLE IF EXISTS wholesale_output.energy_results_backup
GO

-- Step 5: Use some dummy value in place of NULL values before (re)enforcing NOT NULL constraint
UPDATE {OUTPUT_DATABASE_NAME}.energy_results
SET quantity = -999999.999
WHERE quantity IS NULL
GO

-- Step 6: Enforce quantity NOT NULL constraint
ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    ADD CONSTRAINT quantity_chk CHECK (quantity IS NOT NULL)
