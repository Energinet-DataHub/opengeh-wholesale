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
from unittest import mock
from unittest.mock import patch

from pyspark.sql import Row, SparkSession

from package.calculation.basis_data.schemas import calculations_schema
from package.calculation.calculator_args import CalculatorArgs
from package.calculation.output.calculation_writer import _create_calculation
from package.calculation.preparation import PreparedDataReader
from package.constants.calculation_column_names import CalculationColumnNames


def test__when_valid_input__creates_calculation_with_expected_schema(
    any_calculator_args: CalculatorArgs,
    spark: SparkSession,
) -> None:
    prepared_data_reader = PreparedDataReader(mock.Mock())
    with patch.object(
        prepared_data_reader,
        prepared_data_reader.get_latest_calculation_version.__name__,
        return_value=0,
    ):
        actual = _create_calculation(any_calculator_args, prepared_data_reader, spark)
        assert actual.schema == calculations_schema


def test__when_valid_input__creates_expected_calculation(
    any_calculator_args: CalculatorArgs,
    spark: SparkSession,
) -> None:
    # Arrange
    latest_version = 12
    next_version = 13
    expected = {
        CalculationColumnNames.calculation_id: any_calculator_args.calculation_id,
        CalculationColumnNames.calculation_type: any_calculator_args.calculation_type.value,
        CalculationColumnNames.period_start: any_calculator_args.calculation_period_start_datetime,
        CalculationColumnNames.period_end: any_calculator_args.calculation_period_end_datetime,
        CalculationColumnNames.execution_time_start: any_calculator_args.calculation_execution_time_start,
        CalculationColumnNames.created_by_user_id: any_calculator_args.created_by_user_id,
        CalculationColumnNames.version: next_version,
    }
    prepared_data_reader = PreparedDataReader(mock.Mock())
    with patch.object(
        prepared_data_reader,
        prepared_data_reader.get_latest_calculation_version.__name__,
        return_value=latest_version,
    ):
        actual = _create_calculation(any_calculator_args, prepared_data_reader, spark)
        assert actual.collect()[0] == Row(**expected)


def test__when_no_calculation_exists__creates_new_calculation_with_version_1(
    any_calculator_args: CalculatorArgs,
    spark: SparkSession,
) -> None:
    latest_version = None
    prepared_data_reader = PreparedDataReader(mock.Mock())
    with patch.object(
        prepared_data_reader,
        prepared_data_reader.get_latest_calculation_version.__name__,
        return_value=latest_version,
    ):
        actual = _create_calculation(any_calculator_args, prepared_data_reader, spark)
        assert actual.collect()[0].version == 1


def test__when_calculation_exists__creates_new_calculation_with_latest_version_plus_1(
    any_calculator_args: CalculatorArgs,
    spark: SparkSession,
) -> None:
    prepared_data_reader = PreparedDataReader(mock.Mock())
    with patch.object(
        prepared_data_reader,
        prepared_data_reader.get_latest_calculation_version.__name__,
        return_value=7,
    ):
        actual = _create_calculation(any_calculator_args, prepared_data_reader, spark)
        assert actual.collect()[0].version == 8
