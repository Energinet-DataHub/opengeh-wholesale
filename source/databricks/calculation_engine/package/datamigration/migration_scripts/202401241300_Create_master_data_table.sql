CREATE TABLE IF NOT EXISTS {OUTPUT_DATABASE_NAME}.master_basis_data
(
    calculation_id STRING NOT NULL,
    calculation_type STRING NOT NULL,
    grid_area STRING NOT NULL,
    energy_supplier_id STRING
    metering_point_id STRING NOT NULL,
    period_start TIMESTAMP NOT NULL,
    period_end TIMESTAMP,
    in_grid_area STRING,
    out_grid_area STRING,
    metering_point_type STRING NOT NULL,
    settlement_method STRING,
)
USING DELTA
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
{TEST}LOCATION '{CONTAINER_PATH}/{OUTPUT_FOLDER}/master_basis_data'


