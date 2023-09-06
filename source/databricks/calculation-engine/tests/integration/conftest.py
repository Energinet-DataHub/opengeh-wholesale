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

import pytest
from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.types import StructType
from package.infrastructure import paths
from package.calculation_input.schemas import (
    time_series_point_schema, metering_point_period_schema, charge_master_data_periods_schema, charge_price_points_schema, charge_link_periods_schema
)


@pytest.fixture(scope="session")
def test_files_folder_path(integration_tests_path: str) -> str:
    return f"{integration_tests_path}/test_files"


@pytest.fixture(scope="session")
def grid_loss_responsible_test_data(
    spark: SparkSession,
    test_files_folder_path: str,
) -> DataFrame:
    return spark.read.csv(f"{test_files_folder_path}/GridLossResponsible.csv", header=True)


@pytest.fixture(scope="session")
def energy_input_data_written_to_delta(
    spark: SparkSession,
    test_files_folder_path: str,
    calculation_input_path: str
) -> None:
    # Metering point periods
    _write_input_test_data_to_table(
        spark,
        file_name=f"{test_files_folder_path}/MeteringPointsPeriods.csv",
        table_name=paths.METERING_POINT_PERIODS_TABLE_NAME,
        schema=metering_point_period_schema,
        table_location=f"{calculation_input_path}/metering_point_periods"
    )

    # Time series points
    _write_input_test_data_to_table(
        spark,
        file_name=f"{test_files_folder_path}/TimeSeriesPoints.csv",
        table_name=paths.TIME_SERIES_POINTS_TABLE_NAME,
        schema=time_series_point_schema,
        table_location=f"{calculation_input_path}/time_series_points"
    )


@pytest.fixture(scope="session")
def price_input_data_written_to_delta(
    spark: SparkSession,
    test_files_folder_path: str,
    calculation_input_path: str
) -> None:
    # Charge master data periods
    _write_input_test_data_to_table(
        spark,
        file_name=f"{test_files_folder_path}/ChargeMasterDataPeriods.csv",
        table_name=paths.CHARGE_MASTER_DATA_PERIODS_TABLE_NAME,
        schema=charge_master_data_periods_schema,
        table_location=f"{calculation_input_path}/charge_master_data_periods"
    )

    # Charge link periods
    _write_input_test_data_to_table(
        spark,
        file_name=f"{test_files_folder_path}/ChargeLinkPeriods.csv",
        table_name=paths.CHARGE_LINK_PERIODS_TABLE_NAME,
        schema=charge_link_periods_schema,
        table_location=f"{calculation_input_path}/charge_link_periods"
    )

    # Charge price points
    _write_input_test_data_to_table(
        spark,
        file_name=f"{test_files_folder_path}/ChargePricePoints.csv",
        table_name=paths.CHARGE_PRICE_POINTS_TABLE_NAME,
        schema=charge_price_points_schema,
        table_location=f"{calculation_input_path}/charge_price_points"
    )


def _write_input_test_data_to_table(
        spark: SparkSession,
        file_name: str,
        table_name: str,
        table_location: str,
        schema: StructType,
) -> None:
    spark.sql(f"CREATE DATABASE IF NOT EXISTS {paths.INPUT_DATABASE_NAME}")
    spark.sql(f"CREATE TABLE IF NOT EXISTS {paths.INPUT_DATABASE_NAME}.{table_name} USING DELTA LOCATION '{table_location}'")

    df = spark.read.csv(
        file_name,
        header=True,
        schema=schema,
    )

    df.write.format("delta").mode("overwrite").option("overwriteSchema", "true").saveAsTable(
        f"{paths.INPUT_DATABASE_NAME}.{table_name}"
    )
