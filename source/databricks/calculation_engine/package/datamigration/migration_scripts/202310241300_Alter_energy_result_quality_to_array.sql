ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    DROP CONSTRAINT quantity_qualities_chk
GO

--
-- Replace ['incomplete'] with ['missing']
-- Note that as of this migration all qualities arrays contain exactly one element
--
UPDATE TABLE {OUTPUT_DATABASE_NAME}.energy_results
SET qualities =
  CASE
    WHEN array_contains('incomplete') THEN array('missing')
    ELSE qualities
  END
)
GO

--
-- Update constraint to no more allow 'incomplete'
--
ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    ADD CONSTRAINT quantity_qualities_chk
    CHECK (array_size(array_except(quantity_qualities, array('missing', 'calculated', 'measured', 'estimated'))) = 0
           AND array_size(quantity_qualities) > 0)
GO
