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

import uuid
from copy import copy
from datetime import datetime, timedelta
from decimal import Decimal
from typing import Any, List

import pytest
from pyspark.sql import SparkSession, DataFrame

import package.codelists as e
from package.calculation.calculator_args import CalculatorArgs
from package.calculation.energy.energy_results import (
    energy_results_schema,
)
from package.calculation.output import energy_storage_model_factory as sut
from package.constants import Colname, EnergyResultColumnNames
from package.infrastructure.paths import OUTPUT_DATABASE_NAME, ENERGY_RESULT_TABLE_NAME
from tests.contract_utils import (
    assert_contract_matches_schema,
    get_column_names_from_contract,
)

# The calculation id is used in parameterized test executed using xdist, which does not allow parameters to change
DEFAULT_CALCULATION_ID = "0b15a420-9fc8-409a-a169-fbd49479d718"
DEFAULT_GRID_AREA = "105"
DEFAULT_FROM_GRID_AREA = "106"
DEFAULT_TO_GRID_AREA = "107"
DEFAULT_ENERGY_SUPPLIER_ID = "9876543210123"
DEFAULT_BALANCE_RESPONSIBLE_ID = "1234567890123"
DEFAULT_CALCULATION_TYPE = e.CalculationType.BALANCE_FIXING
DEFAULT_CALCULATION_EXECUTION_START = datetime(2022, 6, 10, 13, 15)
DEFAULT_QUANTITY = "1.1"
DEFAULT_QUALITY = e.QuantityQuality.MEASURED
DEFAULT_TIME_SERIES_TYPE = e.TimeSeriesType.PRODUCTION
DEFAULT_AGGREGATION_LEVEL = e.AggregationLevel.TOTAL_GA
DEFAULT_TIME_WINDOW_START = datetime(2020, 1, 1, 0, 0)
DEFAULT_TIME_WINDOW_END = datetime(2020, 1, 1, 1, 0)
DEFAULT_METERING_POINT_TYPE = e.MeteringPointType.PRODUCTION
DEFAULT_SETTLEMENT_METHOD = e.SettlementMethod.FLEX

OTHER_CALCULATION_ID = "0b15a420-9fc8-409a-a169-fbd49479d719"
OTHER_GRID_AREA = "205"
OTHER_FROM_GRID_AREA = "206"
OTHER_TO_GRID_AREA = "207"
OTHER_ENERGY_SUPPLIER_ID = "9876543210124"
OTHER_BALANCE_RESPONSIBLE_ID = "1234567890124"
OTHER_CALCULATION_TYPE = e.CalculationType.AGGREGATION
OTHER_CALCULATION_EXECUTION_START = datetime(2023, 6, 10, 13, 15)
OTHER_QUANTITY = "1.2"
OTHER_QUALITY = e.QuantityQuality.CALCULATED
OTHER_TIME_SERIES_TYPE = e.TimeSeriesType.NON_PROFILED_CONSUMPTION
OTHER_AGGREGATION_LEVEL = e.AggregationLevel.ES_PER_GA
OTHER_TIME_WINDOW_START = datetime(2021, 1, 1, 0, 0)
OTHER_TIME_WINDOW_END = datetime(2021, 1, 1, 1, 0)
OTHER_METERING_POINT_TYPE = e.MeteringPointType.CONSUMPTION
OTHER_SETTLEMENT_METHOD = e.SettlementMethod.NON_PROFILED


TABLE_NAME = f"{OUTPUT_DATABASE_NAME}.{ENERGY_RESULT_TABLE_NAME}"


@pytest.fixture(scope="module")
def args(any_calculator_args: CalculatorArgs) -> CalculatorArgs:
    args = copy(any_calculator_args)
    args.calculation_type = DEFAULT_CALCULATION_TYPE
    args.calculation_id = str(uuid.uuid4())
    args.calculation_execution_time_start = DEFAULT_CALCULATION_EXECUTION_START

    return args


def _create_result_row(
    grid_area: str = DEFAULT_GRID_AREA,
    to_grid_area: str = DEFAULT_TO_GRID_AREA,
    from_grid_area: str = DEFAULT_FROM_GRID_AREA,
    energy_supplier_id: str = DEFAULT_ENERGY_SUPPLIER_ID,
    balance_responsible_id: str = DEFAULT_BALANCE_RESPONSIBLE_ID,
    quantity: str = DEFAULT_QUANTITY,
    quality: e.QuantityQuality = DEFAULT_QUALITY,
    time_window_start: datetime = DEFAULT_TIME_WINDOW_START,
    time_window_end: datetime = DEFAULT_TIME_WINDOW_END,
    metering_point_id: str | None = None,
) -> dict:
    row = {
        Colname.grid_area: grid_area,
        Colname.to_grid_area: to_grid_area,
        Colname.from_grid_area: from_grid_area,
        Colname.balance_responsible_id: balance_responsible_id,
        Colname.energy_supplier_id: energy_supplier_id,
        Colname.time_window: {
            Colname.start: time_window_start,
            Colname.end: time_window_end,
        },
        Colname.sum_quantity: Decimal(quantity),
        Colname.qualities: [quality.value],
        Colname.settlement_method: [],
        Colname.metering_point_id: metering_point_id,
    }

    return row


def _create_result_df(spark: SparkSession, row: List[dict]) -> DataFrame:
    return spark.createDataFrame(data=row, schema=energy_results_schema)


def _create_result_df_corresponding_to_four_calculation_results(
    spark: SparkSession,
) -> DataFrame:
    OTHER_TIME_WINDOW_START = DEFAULT_TIME_WINDOW_END
    OTHER_TIME_WINDOW_END = OTHER_TIME_WINDOW_START + timedelta(hours=1)
    OTHER_GRID_AREA = "111"
    OTHER_FROM_GRID_AREA = "222"
    OTHER_ENERGY_SUPPLIER_ID = "1234567890123"

    rows = [
        # First result
        _create_result_row(),
        _create_result_row(
            time_window_start=OTHER_TIME_WINDOW_START,
            time_window_end=OTHER_TIME_WINDOW_END,
        ),
        # Second result
        _create_result_row(grid_area=OTHER_GRID_AREA),
        _create_result_row(
            grid_area=OTHER_GRID_AREA,
            time_window_start=OTHER_TIME_WINDOW_START,
            time_window_end=OTHER_TIME_WINDOW_END,
        ),
        # Third result
        _create_result_row(from_grid_area=OTHER_FROM_GRID_AREA),
        _create_result_row(
            from_grid_area=OTHER_FROM_GRID_AREA,
            time_window_start=OTHER_TIME_WINDOW_START,
            time_window_end=OTHER_TIME_WINDOW_END,
        ),
        # Fourth result
        _create_result_row(energy_supplier_id=OTHER_ENERGY_SUPPLIER_ID),
    ]

    return _create_result_df(spark, rows)


@pytest.mark.parametrize(
    "aggregation_level",
    e.AggregationLevel,
)
def test__write__writes_aggregation_level(
    spark: SparkSession,
    aggregation_level: e.AggregationLevel,
    migrations_executed: None,
    args: CalculatorArgs,
) -> None:
    # Arrange
    row = [_create_result_row()]
    result_df = _create_result_df(spark, row)

    # Act
    actual = sut.create(
        args,
        result_df,
        e.TimeSeriesType.PRODUCTION,
        aggregation_level,
    )

    # Assert
    assert actual.collect()[0][Colname.aggregation_level] == aggregation_level.value


@pytest.mark.parametrize(
    "column_name, column_value",
    [
        (EnergyResultColumnNames.calculation_id, DEFAULT_CALCULATION_ID),
        (
            EnergyResultColumnNames.calculation_execution_time_start,
            DEFAULT_CALCULATION_EXECUTION_START,
        ),
        (EnergyResultColumnNames.calculation_type, DEFAULT_CALCULATION_TYPE.value),
        (EnergyResultColumnNames.time_series_type, DEFAULT_TIME_SERIES_TYPE.value),
        (EnergyResultColumnNames.grid_area, DEFAULT_GRID_AREA),
        (EnergyResultColumnNames.from_grid_area, DEFAULT_FROM_GRID_AREA),
        (
            EnergyResultColumnNames.balance_responsible_id,
            DEFAULT_BALANCE_RESPONSIBLE_ID,
        ),
        (EnergyResultColumnNames.energy_supplier_id, DEFAULT_ENERGY_SUPPLIER_ID),
        (EnergyResultColumnNames.time, datetime(2020, 1, 1, 0, 0)),
        (EnergyResultColumnNames.quantity, Decimal("1.100")),
        (EnergyResultColumnNames.quantity_qualities, [DEFAULT_QUALITY.value]),
    ],
)
def test__write__writes_column(
    spark: SparkSession,
    column_name: str,
    column_value: Any,
    migrations_executed: None,
    args: CalculatorArgs,
) -> None:
    # Arrange
    row = [_create_result_row()]
    result_df = _create_result_df(spark, row)

    # Act
    actual = sut.create(
        args,
        result_df,
        DEFAULT_TIME_SERIES_TYPE,
        DEFAULT_AGGREGATION_LEVEL,
    )

    # Assert
    assert actual.collect()[0][column_name] == column_value


def test__write__writes_columns_matching_contract(
    spark: SparkSession,
    contracts_path: str,
    migrations_executed: None,
    args: CalculatorArgs,
) -> None:
    # Arrange
    contract_path = f"{contracts_path}/energy-result-table-column-names.json"
    row = [_create_result_row()]
    result_df = _create_result_df(spark, row)

    # Act
    actual = sut.create(
        args,
        result_df,
        DEFAULT_TIME_SERIES_TYPE,
        DEFAULT_AGGREGATION_LEVEL,
    )

    # Assert
    assert_contract_matches_schema(contract_path, actual.schema)


def test__write__writes_calculation_result_id(
    spark: SparkSession,
    contracts_path: str,
    migrations_executed: None,
    args: CalculatorArgs,
) -> None:
    # Arrange
    result_df = _create_result_df_corresponding_to_four_calculation_results(spark)
    EXPECTED_NUMBER_OF_CALCULATION_RESULT_IDS = 4

    # Act
    actual = sut.create(
        args,
        result_df,
        DEFAULT_TIME_SERIES_TYPE,
        DEFAULT_AGGREGATION_LEVEL,
    )

    # Assert
    assert actual.distinct().count() == EXPECTED_NUMBER_OF_CALCULATION_RESULT_IDS


@pytest.mark.parametrize(
    "column_name, value, other_value",
    [
        (Colname.grid_area, DEFAULT_GRID_AREA, OTHER_GRID_AREA),
        (Colname.from_grid_area, DEFAULT_FROM_GRID_AREA, OTHER_FROM_GRID_AREA),
        (
            Colname.balance_responsible_id,
            DEFAULT_BALANCE_RESPONSIBLE_ID,
            OTHER_BALANCE_RESPONSIBLE_ID,
        ),
        (
            Colname.energy_supplier_id,
            DEFAULT_ENERGY_SUPPLIER_ID,
            OTHER_ENERGY_SUPPLIER_ID,
        ),
    ],
)
def test__write__when_rows_belong_to_different_results__adds_different_calculation_result_id(
    spark: SparkSession,
    column_name: str,
    value: Any,
    other_value: Any,
    migrations_executed: None,
    args: CalculatorArgs,
) -> None:
    # Arrange
    row1 = _create_result_row()
    row1[column_name] = value
    row2 = _create_result_row()
    row2[column_name] = other_value
    result_df = _create_result_df(spark, [row1, row2])

    # Act
    actual = sut.create(
        args,
        result_df,
        DEFAULT_TIME_SERIES_TYPE,
        DEFAULT_AGGREGATION_LEVEL,
    )

    # Assert
    rows = actual.collect()
    assert (
        rows[0][EnergyResultColumnNames.calculation_result_id]
        != rows[1][EnergyResultColumnNames.calculation_result_id]
    )


@pytest.mark.parametrize(
    "column_name, value, other_value",
    [
        (Colname.sum_quantity, Decimal(DEFAULT_QUANTITY), Decimal(OTHER_QUANTITY)),
        (
            Colname.quality,
            DEFAULT_QUALITY.value,
            OTHER_QUALITY.value,
        ),
        (Colname.to_grid_area, DEFAULT_TO_GRID_AREA, OTHER_TO_GRID_AREA),
        (
            Colname.metering_point_type,
            DEFAULT_METERING_POINT_TYPE.value,
            OTHER_METERING_POINT_TYPE.value,
        ),
        (
            Colname.settlement_method,
            DEFAULT_SETTLEMENT_METHOD.value,
            OTHER_SETTLEMENT_METHOD.value,
        ),
        (Colname.time_window_start, DEFAULT_TIME_WINDOW_START, OTHER_TIME_WINDOW_START),
        (Colname.time_window_end, DEFAULT_TIME_WINDOW_END, OTHER_TIME_WINDOW_END),
    ],
)
def test__write__when_rows_belong_to_same_result__adds_same_calculation_result_id(
    spark: SparkSession,
    column_name: str,
    value: Any,
    other_value: Any,
    migrations_executed: None,
    args: CalculatorArgs,
) -> None:
    # Arrange
    row1 = _create_result_row(grid_area="804")
    row1[column_name] = value
    row2 = _create_result_row(grid_area="803")
    row2[column_name] = other_value
    result_df = _create_result_df(spark, [row1, row2])

    # Act
    actual = sut.create(
        args,
        result_df,
        DEFAULT_TIME_SERIES_TYPE,
        DEFAULT_AGGREGATION_LEVEL,
    )

    # Assert
    rows = actual.collect()
    assert (
        rows[0][EnergyResultColumnNames.calculation_result_id]
        != rows[1][EnergyResultColumnNames.calculation_result_id]
    )


def test__get_column_group_for_calculation_result_id__excludes_expected_other_column_names(
    contracts_path: str,
) -> None:
    # This class is a guard against adding new columns without considering how the column affects the generation of calculation result IDs

    # Arrange
    expected_other_columns = [
        # Data that doesn't vary for rows in a data frame
        EnergyResultColumnNames.calculation_id,
        EnergyResultColumnNames.calculation_type,
        EnergyResultColumnNames.calculation_execution_time_start,
        EnergyResultColumnNames.time_series_type,
        EnergyResultColumnNames.aggregation_level,
        # Data that does vary but does not define distinct results
        EnergyResultColumnNames.time,
        EnergyResultColumnNames.quantity_qualities,
        EnergyResultColumnNames.quantity,
        # The field that defines results
        EnergyResultColumnNames.calculation_result_id,
        EnergyResultColumnNames.metering_point_id,
    ]
    contract_path = f"{contracts_path}/energy-result-table-column-names.json"
    all_columns = get_column_names_from_contract(contract_path)

    # Act
    included_columns = sut._get_column_group_for_calculation_result_id()

    # Assert
    included_columns = list(
        map(_map_colname_to_energy_result_column_name, included_columns)
    )
    actual_other_columns = set(all_columns) - set(included_columns)
    assert set(actual_other_columns) == set(expected_other_columns)


def _map_colname_to_energy_result_column_name(field_name: str) -> str:
    """
    Test workaround as the contract specifies the Delta table column names
    while some of the data frame column names are using `Colname` names.
    """
    if field_name == Colname.grid_area:
        return EnergyResultColumnNames.grid_area
    if field_name == Colname.from_grid_area:
        return EnergyResultColumnNames.from_grid_area
    if field_name == Colname.balance_responsible_id:
        return EnergyResultColumnNames.balance_responsible_id
    if field_name == Colname.energy_supplier_id:
        return EnergyResultColumnNames.energy_supplier_id
    return field_name
