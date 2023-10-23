ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    DROP CONSTRAINT quantity_qualities_chk
GO

ALTER TABLE {OUTPUT_DATABASE_NAME}.energy_results
    ADD CONSTRAINT quantity_qualities_chk
    CHECK (array_size(array_except(quantity_qualities, array('missing', 'calculated', 'measured', 'estimated', 'incomplete'))) = 0)
GO
