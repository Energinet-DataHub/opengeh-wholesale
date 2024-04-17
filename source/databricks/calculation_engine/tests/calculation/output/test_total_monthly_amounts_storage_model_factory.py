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
from copy import copy
from datetime import datetime
from decimal import Decimal
from typing import Any
from typing import List

import pytest
from pyspark.sql import SparkSession, DataFrame

from contract_utils import assert_contract_matches_schema
from package.calculation.calculator_args import CalculatorArgs
from package.calculation.output import (
    total_monthly_amounts_storage_model_factory as sut,
)
from package.calculation.wholesale.data_structures import TotalMonthlyAmount
from package.calculation.wholesale.data_structures.total_monthly_amount import (
    total_monthly_amount_schema,
)
from package.codelists import (
    CalculationType,
)
from package.constants import Colname, TotalMonthlyAmountsColumnNames
from package.infrastructure.paths import (
    OUTPUT_DATABASE_NAME,
    TOTAL_MONTHLY_AMOUNTS_TABLE_NAME,
)

TABLE_NAME = f"{OUTPUT_DATABASE_NAME}.{TOTAL_MONTHLY_AMOUNTS_TABLE_NAME}"

# Writer constructor parameters
DEFAULT_CALCULATION_ID = "0b15a420-9fc8-409a-a169-fbd49479d718"
DEFAULT_CALCULATION_TYPE = CalculationType.FIRST_CORRECTION_SETTLEMENT
DEFAULT_CALCULATION_EXECUTION_START = datetime(2022, 6, 10, 13, 15)

# Input dataframe parameters
DEFAULT_ENERGY_SUPPLIER_ID = "9876543210123"
DEFAULT_GRID_AREA = "543"
DEFAULT_CHARGE_TIME = datetime(2022, 6, 10, 13, 30)
DEFAULT_CHARGE_OWNER_ID = "5790001330552"
DEFAULT_TOTAL_AMOUNT = Decimal("123.456")


@pytest.fixture(scope="module")
def args(any_calculator_args: CalculatorArgs) -> CalculatorArgs:
    args = copy(any_calculator_args)

    args.calculation_type = DEFAULT_CALCULATION_TYPE
    args.calculation_id = DEFAULT_CALCULATION_ID
    args.calculation_execution_time_start = DEFAULT_CALCULATION_EXECUTION_START

    return args


def _create_result_row(
    energy_supplier_id: str = DEFAULT_ENERGY_SUPPLIER_ID,
    grid_area: str = DEFAULT_GRID_AREA,
    charge_owner: str = DEFAULT_CHARGE_OWNER_ID,
    charge_time: datetime = DEFAULT_CHARGE_TIME,
    total_amount: Decimal = DEFAULT_TOTAL_AMOUNT,
) -> dict:
    row = {
        Colname.energy_supplier_id: energy_supplier_id,
        Colname.grid_area: grid_area,
        Colname.charge_time: charge_time,
        Colname.charge_owner: charge_owner,
        Colname.total_amount: total_amount,
    }

    return row


def _create_default_total_monthly_amounts(
    spark: SparkSession,
) -> TotalMonthlyAmount:
    row = [_create_result_row()]
    return TotalMonthlyAmount(
        spark.createDataFrame(data=row, schema=total_monthly_amount_schema)
    )


def _create_multiple_total_monthly_amounts(
    spark: SparkSession,
) -> TotalMonthlyAmount:
    # 3 calculation results with just one row each
    rows = [
        _create_result_row(grid_area="001"),
        _create_result_row(grid_area="002"),
        _create_result_row(grid_area="003"),
    ]

    return TotalMonthlyAmount(spark.createDataFrame(data=rows))


def test__create__columns_matching_contract(
    spark: SparkSession,
    contracts_path: str,
    migrations_executed: None,
    args: CalculatorArgs,
) -> None:
    # Arrange
    contract_path = f"{contracts_path}/total-monthly-amounts-table-column-names.json"
    total_monthly_amounts = _create_default_total_monthly_amounts(spark)

    # Act
    actual = sut.create(args, total_monthly_amounts)

    # Assert
    assert_contract_matches_schema(contract_path, actual.schema)


@pytest.mark.parametrize(
    "column_name, column_value",
    [
        (TotalMonthlyAmountsColumnNames.calculation_id, DEFAULT_CALCULATION_ID),
        (
            TotalMonthlyAmountsColumnNames.calculation_type,
            DEFAULT_CALCULATION_TYPE.value,
        ),
        (
            TotalMonthlyAmountsColumnNames.calculation_execution_time_start,
            DEFAULT_CALCULATION_EXECUTION_START,
        ),
        (TotalMonthlyAmountsColumnNames.grid_area, DEFAULT_GRID_AREA),
        (TotalMonthlyAmountsColumnNames.energy_supplier_id, DEFAULT_ENERGY_SUPPLIER_ID),
        (TotalMonthlyAmountsColumnNames.time, DEFAULT_CHARGE_TIME),
        (TotalMonthlyAmountsColumnNames.amount, DEFAULT_TOTAL_AMOUNT),
        (TotalMonthlyAmountsColumnNames.charge_owner_id, DEFAULT_CHARGE_OWNER_ID),
    ],
)
def test__create__returns_dataframe_with_column(
    spark: SparkSession,
    args: CalculatorArgs,
    column_name: str,
    column_value: Any,
) -> None:
    """Test all columns except calculation_result_id. It is tested separately in another test."""

    # Arrange
    total_monthly_amounts = _create_default_total_monthly_amounts(spark)

    # Act
    actual_df = sut.create(args, total_monthly_amounts)

    # Assert
    actual_row = actual_df.collect()[0]
    assert actual_row[column_name] == column_value


def test__create__returns_dataframe_with_calculation_result_id(
    spark: SparkSession,
    args: CalculatorArgs,
) -> None:
    # Arrange
    total_monthly_amounts = _create_multiple_total_monthly_amounts(spark)
    expected_number_of_calculation_result_ids = 3

    # Act
    actual_df = sut.create(args, total_monthly_amounts)

    # Assert
    assert actual_df.distinct().count() == expected_number_of_calculation_result_ids


def test__get_column_group_for_calculation_result_id__returns_expected_column_names(
    args: CalculatorArgs,
) -> None:
    # Arrange
    expected_column_names = [
        Colname.calculation_id,
        Colname.charge_owner,
        Colname.grid_area,
        Colname.energy_supplier_id,
    ]

    # Act
    actual = sut._get_column_group_for_calculation_result_id()

    # Assert
    assert actual == expected_column_names


def test__get_column_group_for_calculation_result_id__excludes_expected_other_column_names() -> (
    None
):
    # This class is a guard against adding new columns without considering how the column affects the generation of
    # calculation result IDs

    # Arrange
    expected_excluded_columns = [
        TotalMonthlyAmountsColumnNames.calculation_type,
        TotalMonthlyAmountsColumnNames.calculation_execution_time_start,
        TotalMonthlyAmountsColumnNames.calculation_result_id,
        TotalMonthlyAmountsColumnNames.grid_area,
        TotalMonthlyAmountsColumnNames.time,
        TotalMonthlyAmountsColumnNames.amount,
    ]
    all_columns = [
        attr
        for attr in dir(TotalMonthlyAmountsColumnNames)
        if not attr.startswith("__")
    ]

    # Act
    included_columns = sut._get_column_group_for_calculation_result_id()

    # Assert
    excluded_columns = set(all_columns) - set(included_columns)
    assert set(excluded_columns) == set(expected_excluded_columns)
