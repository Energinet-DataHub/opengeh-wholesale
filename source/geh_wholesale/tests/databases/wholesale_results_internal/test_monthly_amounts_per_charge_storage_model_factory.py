from copy import copy
from datetime import datetime
from decimal import Decimal
from typing import Any

import pytest
from pyspark.sql import DataFrame, SparkSession

from geh_wholesale.calculation.calculator_args import CalculatorArgs
from geh_wholesale.calculation.wholesale.data_structures import MonthlyAmountPerCharge
from geh_wholesale.calculation.wholesale.data_structures.monthly_amount_per_charge import (
    monthly_amount_per_charge_schema,
)
from geh_wholesale.codelists import (
    CalculationType,
    ChargeType,
    ChargeUnit,
)
from geh_wholesale.constants import Colname
from geh_wholesale.databases.table_column_names import TableColumnNames
from geh_wholesale.databases.wholesale_results_internal import (
    monthly_amounts_per_charge_storage_model_factory as sut,
)
from geh_wholesale.databases.wholesale_results_internal.schemas import (
    monthly_amounts_schema_uc,
)
from geh_wholesale.infrastructure.paths import (
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


@pytest.fixture
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
    return MonthlyAmountPerCharge(spark.createDataFrame(data=row, schema=monthly_amount_per_charge_schema))


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
    ("column_name", "column_value"),
    [
        (TableColumnNames.calculation_id, DEFAULT_CALCULATION_ID),
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
    result_df = MonthlyAmountPerCharge(_create_result_df_corresponding_to_multiple_calculation_results(spark))
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


def test__get_column_group_for_calculation_result_id__excludes_expected_other_column_names() -> None:
    # This class is a guard against adding new columns without considering how the column affects the generation of
    # calculation result IDs

    # Arrange
    expected_excluded_columns = [
        TableColumnNames.result_id,
        TableColumnNames.quantity_unit,
        TableColumnNames.time,
        TableColumnNames.amount,
        TableColumnNames.is_tax,
    ]
    all_columns = [f.name for f in monthly_amounts_schema_uc.fields]

    # Act
    included_columns = sut._get_column_group_for_calculation_result_id()

    # Assert
    excluded_columns = set(all_columns) - set(included_columns)
    assert set(excluded_columns) == set(expected_excluded_columns)
