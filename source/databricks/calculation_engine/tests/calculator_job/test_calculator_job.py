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


from pyspark.sql import SparkSession
from pyspark.sql.functions import StructType
import pytest
from unittest.mock import patch, Mock

import package.calculator_job
import package.infrastructure.storage_account_access
from package.infrastructure.storage_account_access import islocked
from package.calculator_job_args import get_calculator_args
from package.infrastructure.logging_configuration import configure_logging
from package.calculator_job import start
from package.calculation_input.schemas import (
    time_series_point_schema,
    metering_point_period_schema,
    charge_link_periods_schema,
    charge_master_data_periods_schema,
    charge_price_points_schema,
)
from package.infrastructure import paths

# TODO BJM: These tests are not calculator_job tests


def test__input_time_series_point_schema__matches_published_contract(
    spark: SparkSession, energy_input_data_written_to_delta: None
) -> None:
    # Act: Calculator job is executed just once per session. See the fixture `executed_balance_fixing`

    # Assert
    actual_input_data = spark.read.table(
        f"{paths.INPUT_DATABASE_NAME}.{paths.TIME_SERIES_POINTS_TABLE_NAME}"
    )

    # When asserting both that the calculator creates output and it does it with input data that matches
    # the time series points contract from the time-series domain (in the same test), then we can infer that the
    # calculator works with the format of the data published from the time-series domain.
    # NOTE:It is not evident from this test that it uses the same input as the calculator job
    # Apparently nullability is ignored for CSV sources so we have to compare schemas in this slightly odd way
    # See more at https://stackoverflow.com/questions/50609548/compare-schema-ignoring-nullable
    _assert_is_equal(actual_input_data.schema, time_series_point_schema)


def test__input_metering_point_period_schema__matches_published_contract(
    spark: SparkSession, energy_input_data_written_to_delta: None
) -> None:
    # Act: See the fixture `energy_input_data_written_to_delta`

    # Assert
    test_input_data = spark.read.table(
        f"{paths.INPUT_DATABASE_NAME}.{paths.METERING_POINT_PERIODS_TABLE_NAME}"
    )
    _assert_is_equal(test_input_data.schema, metering_point_period_schema)


def test__input_charge_link_period_schema__matches_published_contract(
    spark: SparkSession, price_input_data_written_to_delta: None
) -> None:
    # Act: See the fixture `energy_input_data_written_to_delta`

    # Assert
    test_input_data = spark.read.table(
        f"{paths.INPUT_DATABASE_NAME}.{paths.CHARGE_LINK_PERIODS_TABLE_NAME}"
    )
    _assert_is_equal(test_input_data.schema, charge_link_periods_schema)


def test__input_charge_price_points_schema__matches_published_contract(
    spark: SparkSession, price_input_data_written_to_delta: None
) -> None:
    # Act: See the fixture `energy_input_data_written_to_delta`

    # Assert
    test_input_data = spark.read.table(
        f"{paths.INPUT_DATABASE_NAME}.{paths.CHARGE_PRICE_POINTS_TABLE_NAME}"
    )
    _assert_is_equal(test_input_data.schema, charge_price_points_schema)


def test__input_charge_master_data_periods_schema__matches_published_contract(
    spark: SparkSession, price_input_data_written_to_delta: None
) -> None:
    # Act: See the fixture `energy_input_data_written_to_delta`

    # Assert
    test_input_data = spark.read.table(
        f"{paths.INPUT_DATABASE_NAME}.{paths.CHARGE_MASTER_DATA_PERIODS_TABLE_NAME}"
    )
    _assert_is_equal(test_input_data.schema, charge_master_data_periods_schema)


def _assert_is_equal(actual_schema: StructType, expected_schema: StructType) -> None:
    assert all(
        (a.name, a.dataType) == (b.name, b.dataType)
        for a, b in zip(actual_schema, expected_schema)
    )
