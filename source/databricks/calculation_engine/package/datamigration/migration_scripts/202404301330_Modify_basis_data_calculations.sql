ALTER TABLE {BASIS_DATA_DATABASE_NAME}.calculations
DROP COLUMN created_time
USING DELTA
TBLPROPERTIES (
    delta.constraints.created_by_user_id_chk = "LENGTH ( created_by_user_id ) = 36"
)
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
{TEST}LOCATION '{CONTAINER_PATH}/{BASIS_DATA_FOLDER}/calculations'
GO
