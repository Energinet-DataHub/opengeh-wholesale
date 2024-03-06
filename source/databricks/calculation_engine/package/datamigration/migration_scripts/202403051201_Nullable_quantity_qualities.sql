ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
    ALTER COLUMN quantity_qualities DROP NOT NULL
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
    DROP CONSTRAINT IF EXISTS quantity_qualities_chk
GO
ALTER TABLE {OUTPUT_DATABASE_NAME}.wholesale_results
    ADD CONSTRAINT quantity_qualities_chk
    CHECK ((quantity_qualities IS NULL) OR (array_size(array_except(quantity_qualities, array('missing', 'calculated', 'measured', 'estimated'))) = 0
           AND array_size(quantity_qualities) > 0))
GO
