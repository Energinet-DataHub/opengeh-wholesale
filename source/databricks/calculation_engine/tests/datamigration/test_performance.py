# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
import datetime
import shutil

from pyspark.sql import SparkSession

from helpers import spark_sql_migration_helper


def test_migrations_executed(
    spark: SparkSession,
    data_lake_path: str,
    calculation_input_folder: str,
    calculation_output_path: str,
    energy_input_data_written_to_delta: None,
) -> None:
    start = datetime.datetime.now()
    # Clean up to prevent problems from previous test runs
    shutil.rmtree(calculation_output_path, ignore_errors=True)
    spark.sql(f"DROP DATABASE IF EXISTS wholesale_output CASCADE")

    end = datetime.datetime.now()
    print(f"DROP DATABASE Execution time: {end - start}")

    start = datetime.datetime.now()
    # Execute all migrations
    spark_sql_migration_helper.migrate(spark)

    end = datetime.datetime.now()
    print(f"MIGRATE Execution time: {end - start}")


def test_raw_sql(spark: SparkSession) -> None:
    sql = [
        """
        CREATE DATABASE IF NOT EXISTS wholesale_output
COMMENT 'Contains result data from wholesale domain.'
""",
        """CREATE TABLE IF NOT EXISTS wholesale_output.energy_results
(
    grid_area STRING NOT NULL,
    energy_supplier_id STRING,
    balance_responsible_id STRING,
    -- Energy quantity in kWh for the given observation time.
    -- Example: 1234.534
    quantity DECIMAL(18, 3) NOT NULL,
    quantity_qualities ARRAY<STRING> NOT NULL,
    -- The time when the energy was consumed/produced/exchanged
    time TIMESTAMP NOT NULL,
    aggregation_level STRING NOT NULL,
    time_series_type STRING NOT NULL,
    calculation_id STRING NOT NULL,
    calculation_type STRING NOT NULL,
    calculation_execution_time_start TIMESTAMP NOT NULL,
    out_grid_area STRING,
    calculation_result_id STRING NOT NULL,
    metering_point_id STRING
)
USING DELTA
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
--{TEST}LOCATION '{CONTAINER_PATH}/{OUTPUT_FOLDER}/result'


""",
        """ALTER TABLE wholesale_output.energy_results
    DROP CONSTRAINT IF EXISTS calculation_type_chk
""",
        """
ALTER TABLE wholesale_output.energy_results
    ADD CONSTRAINT calculation_type_chk CHECK (calculation_type IN ('BalanceFixing', 'Aggregation', 'WholesaleFixing', 'FirstCorrectionSettlement', 'SecondCorrectionSettlement', 'ThirdCorrectionSettlement'))
""",
        """

ALTER TABLE wholesale_output.energy_results
    DROP CONSTRAINT IF EXISTS time_series_type_chk
""",
        """
ALTER TABLE wholesale_output.energy_results
    ADD CONSTRAINT time_series_type_chk
        CHECK (time_series_type IN (
            'production',
            'non_profiled_consumption',
            'net_exchange_per_neighboring_ga',
            'net_exchange_per_ga',
            'flex_consumption',
            'grid_loss',
            'negative_grid_loss',
            'positive_grid_loss',
            'total_consumption',
            'temp_flex_consumption',
            'temp_production'))
""",
        """

ALTER TABLE wholesale_output.energy_results
    DROP CONSTRAINT IF EXISTS grid_area_chk
""",
        """
ALTER TABLE wholesale_output.energy_results
    ADD CONSTRAINT grid_area_chk CHECK (LENGTH(grid_area) = 3)
""",
        """

ALTER TABLE wholesale_output.energy_results
    DROP CONSTRAINT IF EXISTS out_grid_area_chk
""",
        """
ALTER TABLE wholesale_output.energy_results
    ADD CONSTRAINT out_grid_area_chk CHECK (out_grid_area IS NULL OR LENGTH(out_grid_area) = 3)
""",
        """

ALTER TABLE wholesale_output.energy_results
    DROP CONSTRAINT IF EXISTS quantity_qualities_chk
""",
        """
ALTER TABLE wholesale_output.energy_results
    ADD CONSTRAINT quantity_qualities_chk
    CHECK (array_size(array_except(quantity_qualities, array('missing', 'calculated', 'measured', 'estimated'))) = 0
           AND array_size(quantity_qualities) > 0)
""",
        """

ALTER TABLE wholesale_output.energy_results
    DROP CONSTRAINT IF EXISTS aggregation_level_chk
""",
        """
ALTER TABLE wholesale_output.energy_results
    ADD CONSTRAINT aggregation_level_chk CHECK (aggregation_level IN ('total_ga', 'es_brp_ga', 'es_ga', 'brp_ga'))
""",
        """

ALTER TABLE wholesale_output.energy_results
    DROP CONSTRAINT IF EXISTS energy_supplier_id_chk
""",
        """
-- Length is 16 when EIC and 13 when GLN
ALTER TABLE wholesale_output.energy_results
    ADD CONSTRAINT energy_supplier_id_chk CHECK (energy_supplier_id IS NULL OR LENGTH(energy_supplier_id) = 13 OR LENGTH(energy_supplier_id) = 16)
""",
        """

ALTER TABLE wholesale_output.energy_results
    DROP CONSTRAINT IF EXISTS balance_responsible_id_chk
""",
        """
-- Length is 16 when EIC and 13 when GLN
ALTER TABLE wholesale_output.energy_results
    ADD CONSTRAINT balance_responsible_id_chk CHECK (balance_responsible_id IS NULL OR LENGTH(balance_responsible_id) = 13 OR LENGTH(balance_responsible_id) = 16)
""",
        """

ALTER TABLE wholesale_output.energy_results
    DROP CONSTRAINT IF EXISTS calculation_id_chk
""",
        """
ALTER TABLE wholesale_output.energy_results
    ADD CONSTRAINT calculation_id_chk CHECK (LENGTH(calculation_id) = 36)
""",
        """

ALTER TABLE wholesale_output.energy_results
    DROP CONSTRAINT IF EXISTS calculation_result_id_chk
""",
        """
ALTER TABLE wholesale_output.energy_results
    ADD CONSTRAINT calculation_result_id_chk CHECK (LENGTH(calculation_result_id) = 36)
""",
        """

ALTER TABLE wholesale_output.energy_results
    DROP CONSTRAINT IF EXISTS metering_point_id_chk
""",
        """
ALTER TABLE wholesale_output.energy_results
    ADD CONSTRAINT metering_point_id_chk CHECK  (metering_point_id IS NULL OR LENGTH(metering_point_id) = 18)
""",
        """

ALTER TABLE wholesale_output.energy_results
    DROP CONSTRAINT IF EXISTS metering_point_id_conditional_chk
""",
        """
--If time_series_type is 'negative_grid_loss' or 'positive_grid_loss', then metering_point_id must not be null.
--If time_series_type is neither 'negative_grid_loss' nor 'positive_grid_loss', then metering_point_id must be null.
ALTER TABLE wholesale_output.energy_results
    ADD CONSTRAINT metering_point_id_conditional_chk
    CHECK (
        (time_series_type IN ('negative_grid_loss', 'positive_grid_loss') AND metering_point_id IS NOT NULL)
        OR
        (time_series_type NOT IN ('negative_grid_loss', 'positive_grid_loss') AND metering_point_id IS NULL)
    )
""",
        """CREATE TABLE IF NOT EXISTS wholesale_output.wholesale_results
(
    -- 36 characters UUID
    calculation_id STRING NOT NULL,
    -- Enum
    calculation_type STRING NOT NULL,
    calculation_execution_time_start TIMESTAMP NOT NULL,
    
    -- 36 characters UUID
    calculation_result_id STRING NOT NULL,

    grid_area STRING NOT NULL,
    energy_supplier_id STRING NOT NULL,
    -- Energy quantity for the given observation time and duration as defined by `resolution`.
    -- Example: 1234.534
    quantity DECIMAL(18, 3) NOT NULL,
    quantity_unit STRING NOT NULL,
    quantity_qualities ARRAY<STRING>,
    -- The time when the energy was consumed/produced/exchanged
    time TIMESTAMP NOT NULL,
    resolution STRING NOT NULL,
    -- Null when monthly sum
    metering_point_type STRING,
    -- Null when metering point type is not consumption or when monthly sum
    settlement_method STRING,
    -- Null when monthly sum or when no price data.
    price DECIMAL(18, 6),
    amount DECIMAL(18, 6),
    -- Applies only to tariff. Null when subscription or fee.
    is_tax BOOLEAN,
    charge_code STRING,
    charge_type STRING,
    charge_owner_id STRING,
    amount_type STRING NOT NULL
)
USING DELTA
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used. 
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.    
--{TEST}LOCATION '{CONTAINER_PATH}/{OUTPUT_FOLDER}/wholesale_results'
""",
        """ALTER TABLE wholesale_output.wholesale_results
    DROP CONSTRAINT IF EXISTS calculation_id_chk
""",
        """
ALTER TABLE wholesale_output.wholesale_results
    ADD CONSTRAINT calculation_id_chk CHECK (LENGTH(calculation_id) = 36)
""",
        """

ALTER TABLE wholesale_output.wholesale_results
    DROP CONSTRAINT IF EXISTS calculation_type_chk
""",
        """
ALTER TABLE wholesale_output.wholesale_results
    ADD CONSTRAINT calculation_type_chk CHECK (calculation_type IN ('WholesaleFixing', 'FirstCorrectionSettlement', 'SecondCorrectionSettlement', 'ThirdCorrectionSettlement'))
""",
        """

ALTER TABLE wholesale_output.wholesale_results
    DROP CONSTRAINT IF EXISTS calculation_result_id_chk
""",
        """
ALTER TABLE wholesale_output.wholesale_results
    ADD CONSTRAINT calculation_result_id_chk CHECK (LENGTH(calculation_result_id) = 36)
""",
        """

ALTER TABLE wholesale_output.wholesale_results
    DROP CONSTRAINT IF EXISTS grid_area_chk
""",
        """
ALTER TABLE wholesale_output.wholesale_results
    ADD CONSTRAINT grid_area_chk CHECK (LENGTH(grid_area) = 3)
""",
        """

ALTER TABLE wholesale_output.wholesale_results
    DROP CONSTRAINT IF EXISTS energy_supplier_id_chk
""",
        """
-- Length is 16 when EIC and 13 when GLN
ALTER TABLE wholesale_output.wholesale_results
    ADD CONSTRAINT energy_supplier_id_chk CHECK (LENGTH(energy_supplier_id) = 13 OR LENGTH(energy_supplier_id) = 16)
""",
        """

ALTER TABLE wholesale_output.wholesale_results
    DROP CONSTRAINT IF EXISTS quantity_unit_chk
""",
        """
-- Unit is kWh when tariff, and pcs (number of pieces) when subscription or fee
ALTER TABLE wholesale_output.wholesale_results
    ADD CONSTRAINT quantity_unit_chk CHECK (quantity_unit IN ('kWh', 'pcs'))
""",
        """

ALTER TABLE wholesale_output.wholesale_results
    DROP CONSTRAINT IF EXISTS quantity_qualities_chk
""",
        """
ALTER TABLE wholesale_output.wholesale_results
    ADD CONSTRAINT quantity_qualities_chk
    CHECK ((quantity_qualities IS NULL) OR (array_size(array_except(quantity_qualities, array('missing', 'calculated', 'measured', 'estimated'))) = 0
           AND array_size(quantity_qualities) > 0))
""",
        """

ALTER TABLE wholesale_output.wholesale_results
    DROP CONSTRAINT IF EXISTS resolution_chk
""",
        """
ALTER TABLE wholesale_output.wholesale_results
    ADD CONSTRAINT resolution_chk CHECK (resolution IN ('PT1H', 'P1D', 'P1M'))
""",
        """

ALTER TABLE wholesale_output.wholesale_results
    DROP CONSTRAINT IF EXISTS metering_point_type_chk
""",
        """
ALTER TABLE wholesale_output.wholesale_results
    ADD CONSTRAINT metering_point_type_chk CHECK (metering_point_type IS NULL OR metering_point_type IN (
        'production',
        'consumption',
        'exchange',
        've_production',
        'net_production',
        'supply_to_grid',
        'consumption_from_grid',
        'wholesale_services_information',
        'own_production',
        'net_from_grid',
        'net_to_grid',
        'total_consumption',
        'electrical_heating',
        'net_consumption',
        'effect_settlement'))
""",
        """

ALTER TABLE wholesale_output.wholesale_results
    DROP CONSTRAINT IF EXISTS settlement_method_chk
""",
        """
ALTER TABLE wholesale_output.wholesale_results
    ADD CONSTRAINT settlement_method_chk CHECK (settlement_method IS NULL OR settlement_method IN ('non_profiled', 'flex'))
""",
        """

-- TODO: Any constraints for charge_id?
ALTER TABLE wholesale_output.wholesale_results
    DROP CONSTRAINT IF EXISTS charge_type_chk
""",
        """
ALTER TABLE wholesale_output.wholesale_results
    ADD CONSTRAINT charge_type_chk CHECK (charge_type IN ('subscription', 'fee', 'tariff'))
""",
        """

ALTER TABLE wholesale_output.wholesale_results
    DROP CONSTRAINT IF EXISTS charge_owner_id_chk
""",
        """
-- Length is 16 when EIC and 13 when GLN
ALTER TABLE wholesale_output.wholesale_results
    ADD CONSTRAINT charge_owner_id_chk CHECK (LENGTH(charge_owner_id) = 13 OR LENGTH(charge_owner_id) = 16)
""",
        """

ALTER TABLE wholesale_output.wholesale_results
    DROP CONSTRAINT IF EXISTS amount_type_chk
""",
        """
ALTER TABLE wholesale_output.wholesale_results
    ADD CONSTRAINT amount_type_chk CHECK (amount_type IN ('amount_per_charge', 'monthly_amount_per_charge', 'total_monthly_amount'))
""",
        """CREATE DATABASE IF NOT EXISTS wholesale_input
COMMENT 'Contains input data from wholesale domain.'
""",
        """CREATE TABLE IF NOT EXISTS wholesale_input.grid_loss_metering_points
(
    metering_point_id STRING NOT NULL
)
USING DELTA
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
--{TEST}LOCATION '{CONTAINER_PATH}/{INPUT_FOLDER}/grid_loss_metering_points'
""",
        """
ALTER TABLE wholesale_input.grid_loss_metering_points
    DROP CONSTRAINT IF EXISTS metering_point_id_chk
""",
        """
ALTER TABLE wholesale_input.grid_loss_metering_points
    ADD CONSTRAINT metering_point_id_chk CHECK (LENGTH(metering_point_id) = 18)
""",
        """CREATE EXTERNAL TABLE if not exists wholesale_input.metering_point_periods
    USING DELTA --LOCATION '{CONTAINER_PATH}/{INPUT_FOLDER}/metering_point_periods'
""",
        """CREATE EXTERNAL TABLE if not exists wholesale_input.time_series_points
    USING DELTA --LOCATION '{CONTAINER_PATH}/{INPUT_FOLDER}/time_series_points'
""",
        """CREATE EXTERNAL TABLE if not exists wholesale_input.charge_link_periods
    USING DELTA --LOCATION '{CONTAINER_PATH}/{INPUT_FOLDER}/charge_link_periods'
""",
        """CREATE EXTERNAL TABLE if not exists wholesale_input.charge_masterdata_periods
    USING DELTA --LOCATION '{CONTAINER_PATH}/{INPUT_FOLDER}/charge_masterdata_periods'
""",
        """CREATE EXTERNAL TABLE if not exists wholesale_input.charge_price_points
    USING DELTA --LOCATION '{CONTAINER_PATH}/{INPUT_FOLDER}/charge_price_points'
""",
        """ALTER TABLE wholesale_output.wholesale_results
    ALTER COLUMN quantity DROP NOT NULL
""",
        """CREATE DATABASE IF NOT EXISTS basis_data
COMMENT 'Contains basis data from wholesale subsystem.'
""",
        """CREATE TABLE IF NOT EXISTS basis_data.metering_point_periods
(
    calculation_id STRING NOT NULL,
    metering_point_id STRING NOT NULL,
    metering_point_type STRING NOT NULL,
    settlement_method STRING,
    grid_area_code STRING NOT NULL,
    resolution STRING NOT NULL,
    from_grid_area_code STRING,
    to_grid_area_code STRING,
    parent_metering_point_id STRING,
    energy_supplier_id STRING,
    balance_responsible_id STRING,
    from_date TIMESTAMP NOT NULL,
    to_date TIMESTAMP
)
USING DELTA
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
--{TEST}LOCATION '{CONTAINER_PATH}/{BASIS_DATA_FOLDER}/metering_point_periods'
""",
        """CREATE TABLE IF NOT EXISTS basis_data.time_series_points
(
    calculation_id STRING NOT NULL,
    metering_point_id STRING NOT NULL,
    quantity DECIMAL(18, 3) NOT NULL,
    quality STRING NOT NULL,
    observation_time TIMESTAMP NOT NULL
)
USING DELTA
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
--{TEST}LOCATION '{CONTAINER_PATH}/{BASIS_DATA_FOLDER}/time_series'
""",
        """CREATE TABLE IF NOT EXISTS basis_data.charge_price_points
(
    calculation_id STRING NOT NULL,
    charge_key STRING NOT NULL,
    charge_code STRING NOT NULL,
    charge_type STRING NOT NULL,
    charge_owner_id STRING NOT NULL,
    charge_price DECIMAL(18, 6) NOT NULL,
    charge_time TIMESTAMP NOT NULL
)
USING DELTA
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
--{TEST}LOCATION '{CONTAINER_PATH}/{BASIS_DATA_FOLDER}/charge_price_points'
""",
        """CREATE TABLE IF NOT EXISTS basis_data.charge_masterdata_periods
(
    calculation_id STRING NOT NULL,
    charge_key STRING NOT NULL,
    charge_code STRING NOT NULL,
    charge_type STRING NOT NULL,
    charge_owner_id STRING NOT NULL,
    resolution STRING NOT NULL,
    is_tax BOOLEAN NOT NULL,
    from_date TIMESTAMP NOT NULL,
    to_date TIMESTAMP
)
USING DELTA
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
--{TEST}LOCATION '{CONTAINER_PATH}/{BASIS_DATA_FOLDER}/charge_masterdata_periods'
""",
        """

CREATE TABLE IF NOT EXISTS basis_data.charge_link_periods
(
    calculation_id STRING NOT NULL,
    charge_key STRING NOT NULL,
    charge_code STRING NOT NULL,
    charge_type STRING NOT NULL,
    charge_owner_id STRING NOT NULL,
    metering_point_id STRING NOT NULL,
    quantity int NOT NULL,
    from_date TIMESTAMP NOT NULL,
    to_date TIMESTAMP
)
USING DELTA
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
--{TEST}LOCATION '{CONTAINER_PATH}/{BASIS_DATA_FOLDER}/charge_link_periods'
""",
        """

CREATE TABLE IF NOT EXISTS basis_data.grid_loss_metering_points
(
    calculation_id STRING NOT NULL,
    metering_point_id STRING NOT NULL
)
USING DELTA
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
--{TEST}LOCATION '{CONTAINER_PATH}/{BASIS_DATA_FOLDER}/grid_loss_metering_points'
""",
        """

CREATE TABLE IF NOT EXISTS basis_data.calculations
(
    calculation_id STRING NOT NULL,
    calculation_type STRING NOT NULL,
    period_start TIMESTAMP NOT NULL,
    period_end TIMESTAMP NOT NULL,
    execution_time_start TIMESTAMP NOT NULL,
    created_time TIMESTAMP NOT NULL,
    created_by_user_id STRING NOT NULL,
    version BIGINT NOT NULL
)
USING DELTA
-- In the test environment the TEST keyword is set to "--" (commented out) and the default location is used.
-- In the production it is set to empty and the respective location is used. This means the production tables won't be deleted if the schema is.
--{TEST}LOCATION '{CONTAINER_PATH}/{BASIS_DATA_FOLDER}/calculations'
""",
    ]

    start = datetime.datetime.now()
    for statement in sql:
        try:
            spark.sql(statement)
        except Exception as e:
            print(f"Error in statement: {statement}")
            print(e)

    end = datetime.datetime.now()
    print(f"Execution time: {end - start}")
