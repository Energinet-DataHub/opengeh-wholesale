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

import pytest
from pyspark.sql import SparkSession, DataFrame

from package.calculation.calculator_args import CalculatorArgs
from package.databases.table_column_names import TableColumnNames
from package.databases.wholesale_results_internal import (
    monthly_amounts_per_charge_storage_model_factory as sut,
)
from package.calculation.wholesale.data_structures import MonthlyAmountPerCharge
from package.calculation.wholesale.data_structures.monthly_amount_per_charge import (
    monthly_amount_per_charge_schema,
)
from package.codelists import (
    ChargeType,
    ChargeUnit,
    CalculationType,
)
from package.constants import Colname
from package.infrastructure.paths import (
    WholesaleResultsInternalDatabase,
)

TABLE_NAME = f"{WholesaleResultsInternalDatabase.DATABASE_NAME}.{WholesaleResultsInternalDatabase.MONTHLY_AMOUNTS_PER_CHARGE_TABLE_NAME}"

# Writer constructor parameters
DEFAULT_CALCULATION_ID = "0b15a420-9fc8-409a-a169-fbd49479d718"
DEFAULT_CALCULATION_TYPE = CalculationType.FIRST_CORRECTION_SETTLEMENT
DEFAULT_CALCULATION_EXECUTION_START = datetime(2022, 6, 10, 13, 15)

# Input dataframe parameters
DEFAULT_ENERGY_SUPPLIER_ID = "9876543210123"
DEFAULT_GRID_AREA_CODE = "543"
DEFAULT_CHARGE_TIME = datetime(2022, 6, 10, 13, 30)
DEFAULT_CHARGE_CODE = "4000"
DEFAULT_CHARGE_TYPE = ChargeType.TARIFF
DEFAULT_CHARGE_OWNER_ID = "5790001330552"
DEFAULT_CHARGE_TAX = True
DEFAULT_TOTAL_AMOUNT = Decimal("123.456")
DEFAULT_UNIT = ChargeUnit.KWH


@pytest.fixture(scope="module")
def args(any_calculator_args: CalculatorArgs) -> CalculatorArgs:
    args = copy(any_calculator_args)

    args.calculation_type = DEFAULT_CALCULATION_TYPE
    args.calculation_id = DEFAULT_CALCULATION_ID
    args.calculation_execution_time_start = DEFAULT_CALCULATION_EXECUTION_START

    return args


def _create_result_row(
    grid_area_code: str = DEFAULT_GRID_AREA_CODE,
    energy_supplier_id: str = DEFAULT_ENERGY_SUPPLIER_ID,
    unit: ChargeUnit = DEFAULT_UNIT,
    charge_time: datetime = DEFAULT_CHARGE_TIME,
    total_amount: Decimal = DEFAULT_TOTAL_AMOUNT,
    charge_tax: bool = DEFAULT_CHARGE_TAX,
    charge_code: str = DEFAULT_CHARGE_CODE,
    charge_type: ChargeType = DEFAULT_CHARGE_TYPE,
    charge_owner: str = DEFAULT_CHARGE_OWNER_ID,
) -> dict:
    row = {
        Colname.grid_area_code: grid_area_code,
        Colname.energy_supplier_id: energy_supplier_id,
        Colname.unit: unit.value,
        Colname.charge_time: charge_time,
        Colname.total_amount: total_amount,
        Colname.charge_tax: charge_tax,
        Colname.charge_code: charge_code,
        Colname.charge_type: charge_type.value,
        Colname.charge_owner: charge_owner,
    }

    return row


def _create_default_result(
    spark: SparkSession,
) -> MonthlyAmountPerCharge:
    row = [_create_result_row()]
    return MonthlyAmountPerCharge(
        spark.createDataFrame(data=row, schema=monthly_amount_per_charge_schema)
    )


def _create_result_df_corresponding_to_multiple_calculation_results(
    spark: SparkSession,
) -> DataFrame:
    # 3 calculation results with just one row each
    rows = [
        _create_result_row(grid_area_code="001"),
        _create_result_row(grid_area_code="002"),
        _create_result_row(grid_area_code="003"),
    ]

    return spark.createDataFrame(data=rows)


@pytest.mark.parametrize(
    "column_name, column_value",
    [
        (TableColumnNames.calculation_id, DEFAULT_CALCULATION_ID),
        (TableColumnNames.calculation_type, DEFAULT_CALCULATION_TYPE.value),
        (
            TableColumnNames.calculation_execution_time_start,
            DEFAULT_CALCULATION_EXECUTION_START,
        ),
        (TableColumnNames.grid_area_code, DEFAULT_GRID_AREA_CODE),
        (TableColumnNames.energy_supplier_id, DEFAULT_ENERGY_SUPPLIER_ID),
        (TableColumnNames.quantity_unit, DEFAULT_UNIT.value),
        (TableColumnNames.time, DEFAULT_CHARGE_TIME),
        (TableColumnNames.amount, DEFAULT_TOTAL_AMOUNT),
        (TableColumnNames.is_tax, DEFAULT_CHARGE_TAX),
        (TableColumnNames.charge_code, DEFAULT_CHARGE_CODE),
        (TableColumnNames.charge_type, DEFAULT_CHARGE_TYPE.value),
        (TableColumnNames.charge_owner_id, DEFAULT_CHARGE_OWNER_ID),
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
    result_df = _create_default_result(spark)

    # Act
    actual_df = sut.create(args, result_df)

    # Assert
    actual_row = actual_df.collect()[0]
    assert actual_row[column_name] == column_value


def test__create__returns_dataframe_with_calculation_result_id(
    spark: SparkSession,
    args: CalculatorArgs,
) -> None:
    # Arrange
    result_df = MonthlyAmountPerCharge(
        _create_result_df_corresponding_to_multiple_calculation_results(spark)
    )
    expected_number_of_calculation_result_ids = 3

    # Act
    actual_df = sut.create(args, result_df)

    # Assert
    assert actual_df.distinct().count() == expected_number_of_calculation_result_ids


def test__get_column_group_for_calculation_result_id__returns_expected_column_names(
    args: CalculatorArgs,
) -> None:
    # Arrange
    expected_column_names = [
        Colname.calculation_id,
        Colname.charge_type,
        Colname.charge_code,
        Colname.charge_owner,
        Colname.grid_area_code,
        Colname.energy_supplier_id,
    ]

    # Act
    actual = sut._get_column_group_for_calculation_result_id()

    # Assert
    assert actual == expected_column_names
