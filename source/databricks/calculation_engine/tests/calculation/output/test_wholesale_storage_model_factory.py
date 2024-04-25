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

from contract_utils import assert_contract_matches_schema
from package.calculation.calculator_args import CalculatorArgs
from package.calculation.output import wholesale_storage_model_factory as sut
from package.calculation.wholesale.data_structures.wholesale_results import (
    WholesaleResults,
    wholesale_results_schema,
)
from package.codelists import (
    ChargeQuality,
    WholesaleResultResolution,
    ChargeType,
    ChargeUnit,
    MeteringPointType,
    CalculationType,
    SettlementMethod,
    AmountType,
)
from package.constants import Colname, WholesaleResultColumnNames
from package.infrastructure.paths import (
    OUTPUT_DATABASE_NAME,
    WHOLESALE_RESULT_TABLE_NAME,
)

TABLE_NAME = f"{OUTPUT_DATABASE_NAME}.{WHOLESALE_RESULT_TABLE_NAME}"

# Writer constructor parameters
DEFAULT_CALCULATION_ID = "0b15a420-9fc8-409a-a169-fbd49479d718"
DEFAULT_CALCULATION_TYPE = CalculationType.FIRST_CORRECTION_SETTLEMENT
DEFAULT_CALCULATION_EXECUTION_START = datetime(2022, 6, 10, 13, 15)

# Input dataframe parameters
DEFAULT_AMOUNT_TYPE = AmountType.AMOUNT_PER_CHARGE
DEFAULT_ENERGY_SUPPLIER_ID = "9876543210123"
DEFAULT_GRID_AREA = "543"
DEFAULT_CHARGE_TIME = datetime(2022, 6, 10, 13, 30)
DEFAULT_INPUT_METERING_POINT_TYPE = MeteringPointType.ELECTRICAL_HEATING
DEFAULT_METERING_POINT_TYPE = (
    MeteringPointType.ELECTRICAL_HEATING
)  # Must correspond with the input type above
DEFAULT_SETTLEMENT_METHOD = SettlementMethod.FLEX
DEFAULT_CHARGE_KEY = "40000-tariff-5790001330552"
DEFAULT_CHARGE_CODE = "4000"
DEFAULT_CHARGE_TYPE = ChargeType.TARIFF
DEFAULT_CHARGE_OWNER_ID = "5790001330552"
DEFAULT_CHARGE_TAX = True
DEFAULT_RESOLUTION = WholesaleResultResolution.HOUR
DEFAULT_CHARGE_PRICE = Decimal("0.756998")
DEFAULT_TOTAL_QUANTITY = Decimal("1.1")
DEFAULT_TOTAL_AMOUNT = Decimal("123.456")
DEFAULT_UNIT = ChargeUnit.KWH
DEFAULT_QUALITY = ChargeQuality.CALCULATED


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
    charge_time: datetime = DEFAULT_CHARGE_TIME,
    metering_point_type: MeteringPointType = DEFAULT_INPUT_METERING_POINT_TYPE,
    settlement_method: SettlementMethod = DEFAULT_SETTLEMENT_METHOD,
    charge_key: str = DEFAULT_CHARGE_KEY,
    charge_code: str = DEFAULT_CHARGE_CODE,
    charge_type: ChargeType = DEFAULT_CHARGE_TYPE,
    charge_owner: str = DEFAULT_CHARGE_OWNER_ID,
    charge_tax: bool = DEFAULT_CHARGE_TAX,
    resolution: WholesaleResultResolution = DEFAULT_RESOLUTION,
    charge_price: Decimal = DEFAULT_CHARGE_PRICE,
    total_quantity: Decimal = DEFAULT_TOTAL_QUANTITY,
    total_amount: Decimal = DEFAULT_TOTAL_AMOUNT,
    unit: ChargeUnit = DEFAULT_UNIT,
    quality: ChargeQuality = DEFAULT_QUALITY,
) -> dict:
    row = {
        Colname.energy_supplier_id: energy_supplier_id,
        Colname.grid_area: grid_area,
        Colname.charge_time: charge_time,
        Colname.metering_point_type: metering_point_type.value,
        Colname.settlement_method: settlement_method.value,
        Colname.charge_key: charge_key,
        Colname.charge_code: charge_code,
        Colname.charge_type: charge_type.value,
        Colname.charge_owner: charge_owner,
        Colname.charge_tax: charge_tax,
        Colname.resolution: resolution.value,
        Colname.charge_price: charge_price,
        Colname.total_quantity: total_quantity,
        Colname.total_amount: total_amount,
        Colname.unit: unit.value,
        Colname.qualities: [quality.value],
    }

    return row


def _create_default_result(
    spark: SparkSession,
) -> WholesaleResults:
    row = [_create_result_row()]
    return WholesaleResults(
        spark.createDataFrame(data=row, schema=wholesale_results_schema)
    )


def _create_result_df_corresponding_to_multiple_calculation_results(
    spark: SparkSession,
) -> DataFrame:
    # 3 calculation results with just one row each
    rows = [
        _create_result_row(grid_area="001"),
        _create_result_row(grid_area="002"),
        _create_result_row(grid_area="003"),
    ]

    return spark.createDataFrame(data=rows)


def test__create__columns_matching_contract(
    spark: SparkSession,
    contracts_path: str,
    args: CalculatorArgs,
) -> None:
    # Arrange
    contract_path = f"{contracts_path}/wholesale-result-table-column-names.json"
    result = _create_default_result(spark)

    # Act
    actual = sut.create(args, result, DEFAULT_AMOUNT_TYPE)

    # Assert
    assert_contract_matches_schema(contract_path, actual.schema)


@pytest.mark.parametrize(
    "column_name, column_value",
    [
        (WholesaleResultColumnNames.calculation_id, DEFAULT_CALCULATION_ID),
        (WholesaleResultColumnNames.calculation_type, DEFAULT_CALCULATION_TYPE.value),
        (
            WholesaleResultColumnNames.calculation_execution_time_start,
            DEFAULT_CALCULATION_EXECUTION_START,
        ),
        (WholesaleResultColumnNames.grid_area, DEFAULT_GRID_AREA),
        (WholesaleResultColumnNames.energy_supplier_id, DEFAULT_ENERGY_SUPPLIER_ID),
        (WholesaleResultColumnNames.quantity, DEFAULT_TOTAL_QUANTITY),
        (WholesaleResultColumnNames.quantity_unit, DEFAULT_UNIT.value),
        (WholesaleResultColumnNames.quantity_qualities, [DEFAULT_QUALITY.value]),
        (WholesaleResultColumnNames.time, DEFAULT_CHARGE_TIME),
        (WholesaleResultColumnNames.resolution, DEFAULT_RESOLUTION.value),
        (
            WholesaleResultColumnNames.metering_point_type,
            DEFAULT_METERING_POINT_TYPE.value,
        ),
        (WholesaleResultColumnNames.settlement_method, DEFAULT_SETTLEMENT_METHOD.value),
        (WholesaleResultColumnNames.price, DEFAULT_CHARGE_PRICE),
        (WholesaleResultColumnNames.amount, DEFAULT_TOTAL_AMOUNT),
        (WholesaleResultColumnNames.is_tax, DEFAULT_CHARGE_TAX),
        (WholesaleResultColumnNames.charge_code, DEFAULT_CHARGE_CODE),
        (WholesaleResultColumnNames.charge_type, DEFAULT_CHARGE_TYPE.value),
        (WholesaleResultColumnNames.charge_owner_id, DEFAULT_CHARGE_OWNER_ID),
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
    actual_df = sut.create(args, result_df, DEFAULT_AMOUNT_TYPE)

    # Assert
    actual_row = actual_df.collect()[0]
    assert actual_row[column_name] == column_value


def test__create__returns_dataframe_with_calculation_result_id(
    spark: SparkSession,
    args: CalculatorArgs,
) -> None:
    # Arrange
    result_df = WholesaleResults(
        _create_result_df_corresponding_to_multiple_calculation_results(spark)
    )
    expected_number_of_calculation_result_ids = 3

    # Act
    actual_df = sut.create(args, result_df, DEFAULT_AMOUNT_TYPE)

    # Assert
    assert actual_df.distinct().count() == expected_number_of_calculation_result_ids


def test__create__returns_dataframe_with_amount_type(
    spark: SparkSession,
    args: CalculatorArgs,
) -> None:
    # Arrange
    result_df = _create_default_result(spark)

    # Act
    actual_df = sut.create(args, result_df, DEFAULT_AMOUNT_TYPE)

    # Assert
    actual_row = actual_df.collect()[0]
    assert (
        actual_row[WholesaleResultColumnNames.amount_type] == DEFAULT_AMOUNT_TYPE.value
    )


def test__get_column_group_for_calculation_result_id__returns_expected_column_names(
    args: CalculatorArgs,
) -> None:
    # Arrange
    expected_column_names = [
        Colname.calculation_id,
        Colname.resolution,
        Colname.charge_type,
        Colname.charge_owner,
        Colname.charge_code,
        Colname.grid_area,
        Colname.energy_supplier_id,
        Colname.metering_point_type,
        Colname.settlement_method,
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
        WholesaleResultColumnNames.calculation_type,
        WholesaleResultColumnNames.calculation_execution_time_start,
        WholesaleResultColumnNames.calculation_result_id,
        WholesaleResultColumnNames.amount_type,
        WholesaleResultColumnNames.quantity,
        WholesaleResultColumnNames.quantity_unit,
        WholesaleResultColumnNames.quantity_qualities,
        WholesaleResultColumnNames.time,
        WholesaleResultColumnNames.price,
        WholesaleResultColumnNames.amount,
        WholesaleResultColumnNames.is_tax,
    ]
    all_columns = [
        getattr(WholesaleResultColumnNames, attribute_name)
        for attribute_name in dir(WholesaleResultColumnNames)
        if not attribute_name.startswith("__")
    ]

    all_columns = _map_metering_point_type_column_name(all_columns)

    # Act
    included_columns = sut._get_column_group_for_calculation_result_id()

    # Assert
    excluded_columns = set(all_columns) - set(included_columns)
    assert set(excluded_columns) == set(expected_excluded_columns)


def _map_metering_point_type_column_name(column_names: list[str]) -> list[str]:
    # this is a simple pragmatic workaround to deal with the fact that the column name for metering point type is not
    # the same in 'Colname' as it is in 'WholesaleResultColumnNames'
    return list(
        map(
            lambda x: x.replace(
                WholesaleResultColumnNames.metering_point_type,
                Colname.metering_point_type,
            ),
            column_names,
        )
    )
