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
from datetime import datetime
from decimal import Decimal
from typing import Any, List

import pytest
from pyspark.sql import SparkSession
from pyspark.sql.functions import col

import package.codelists as e
from package.calculation.calculator_args import CalculatorArgs
from package.calculation.energy.data_structures.energy_results import (
    energy_results_schema,
    EnergyResultsWrapper,
)
from package.constants import Colname
from package.databases.wholesale_results_internal import (
    energy_storage_model_factory as sut,
)
from package.databases.wholesale_results_internal.energy_result_column_names import (
    EnergyResultColumnNames,
)
from package.infrastructure.paths import HiveOutputDatabase

# The calculation id is used in parameterized test executed using xdist, which does not allow parameters to change
DEFAULT_CALCULATION_ID = "0b15a420-9fc8-409a-a169-fbd49479d718"
DEFAULT_GRID_AREA_CODE = "105"
DEFAULT_FROM_GRID_AREA_CODE = "106"
DEFAULT_TO_GRID_AREA_CODE = "107"
DEFAULT_ENERGY_SUPPLIER_ID = "9876543210123"
DEFAULT_BALANCE_RESPONSIBLE_ID = "1234567890123"
DEFAULT_CALCULATION_TYPE = e.CalculationType.BALANCE_FIXING
DEFAULT_CALCULATION_EXECUTION_START = datetime(2022, 6, 10, 13, 15)
DEFAULT_QUANTITY = "1.1"
DEFAULT_QUALITY = e.QuantityQuality.MEASURED
DEFAULT_TIME_SERIES_TYPE = e.TimeSeriesType.PRODUCTION
DEFAULT_AGGREGATION_LEVEL = e.AggregationLevel.GRID_AREA
DEFAULT_OBSERVATION_TIME = datetime(2020, 1, 1, 0, 0)
DEFAULT_METERING_POINT_TYPE = e.MeteringPointType.PRODUCTION
DEFAULT_SETTLEMENT_METHOD = e.SettlementMethod.FLEX

OTHER_CALCULATION_ID = "0b15a420-9fc8-409a-a169-fbd49479d719"
OTHER_GRID_AREA_CODE = "205"
OTHER_FROM_GRID_AREA_CODE = "206"
OTHER_TO_GRID_AREA_CODE = "207"
OTHER_ENERGY_SUPPLIER_ID = "9876543210124"
OTHER_BALANCE_RESPONSIBLE_ID = "1234567890124"
OTHER_CALCULATION_TYPE = e.CalculationType.AGGREGATION
OTHER_CALCULATION_EXECUTION_START = datetime(2023, 6, 10, 13, 15)
OTHER_QUANTITY = "1.2"
OTHER_QUALITY = e.QuantityQuality.CALCULATED
OTHER_TIME_SERIES_TYPE = e.TimeSeriesType.NON_PROFILED_CONSUMPTION
OTHER_OBSERVATION_TIME = datetime(2021, 1, 1, 0, 0)
OTHER_METERING_POINT_TYPE = e.MeteringPointType.CONSUMPTION
OTHER_SETTLEMENT_METHOD = e.SettlementMethod.NON_PROFILED


TABLE_NAME = (
    f"{HiveOutputDatabase.DATABASE_NAME}.{HiveOutputDatabase.ENERGY_RESULT_TABLE_NAME}"
)


@pytest.fixture(scope="module")
def args(any_calculator_args: CalculatorArgs) -> CalculatorArgs:
    args = copy(any_calculator_args)
    args.calculation_type = DEFAULT_CALCULATION_TYPE
    args.calculation_id = str(uuid.uuid4())
    args.calculation_execution_time_start = DEFAULT_CALCULATION_EXECUTION_START

    return args


def _create_result_row(
    grid_area_code: str = DEFAULT_GRID_AREA_CODE,
    to_grid_area_code: str = DEFAULT_TO_GRID_AREA_CODE,
    from_grid_area_code: str = DEFAULT_FROM_GRID_AREA_CODE,
    energy_supplier_id: str = DEFAULT_ENERGY_SUPPLIER_ID,
    balance_responsible_id: str = DEFAULT_BALANCE_RESPONSIBLE_ID,
    quantity: str = DEFAULT_QUANTITY,
    quality: e.QuantityQuality = DEFAULT_QUALITY,
    observation_time: datetime = DEFAULT_OBSERVATION_TIME,
    metering_point_id: str | None = None,
) -> dict:
    row = {
        Colname.grid_area_code: grid_area_code,
        Colname.to_grid_area_code: to_grid_area_code,
        Colname.from_grid_area_code: from_grid_area_code,
        Colname.balance_responsible_id: balance_responsible_id,
        Colname.energy_supplier_id: energy_supplier_id,
        Colname.observation_time: observation_time,
        Colname.quantity: Decimal(quantity),
        Colname.qualities: [quality.value],
        Colname.settlement_method: [],
        Colname.metering_point_id: metering_point_id,
    }

    return row


def _create_energy_results(
    spark: SparkSession, row: List[dict]
) -> EnergyResultsWrapper:
    df = spark.createDataFrame(data=row, schema=energy_results_schema)
    return EnergyResultsWrapper(df)


def _create_energy_results_corresponding_to_four_calculation_results(
    spark: SparkSession,
) -> EnergyResultsWrapper:
    OTHER_GRID_AREA = "111"
    OTHER_FROM_GRID_AREA = "222"
    OTHER_ENERGY_SUPPLIER_ID = "1234567890123"

    rows = [
        # First result
        _create_result_row(),
        _create_result_row(
            observation_time=OTHER_OBSERVATION_TIME,
        ),
        # Second result
        _create_result_row(grid_area_code=OTHER_GRID_AREA),
        _create_result_row(
            grid_area_code=OTHER_GRID_AREA,
            observation_time=OTHER_OBSERVATION_TIME,
        ),
        # Third result
        _create_result_row(from_grid_area_code=OTHER_FROM_GRID_AREA),
        _create_result_row(
            from_grid_area_code=OTHER_FROM_GRID_AREA,
            observation_time=OTHER_OBSERVATION_TIME,
        ),
        # Fourth result
        _create_result_row(energy_supplier_id=OTHER_ENERGY_SUPPLIER_ID),
    ]

    return _create_energy_results(spark, rows)


@pytest.mark.parametrize(
    "aggregation_level",
    e.AggregationLevel,
)
def test__create__with_correct_aggregation_level(
    spark: SparkSession,
    aggregation_level: e.AggregationLevel,
    args: CalculatorArgs,
) -> None:
    # Arrange
    row = [_create_result_row()]
    result_df = _create_energy_results(spark, row)

    # Act
    actual = sut.create(
        args,
        result_df,
        e.TimeSeriesType.PRODUCTION,
        aggregation_level,
    )

    # Assert
    assert (
        actual.collect()[0][EnergyResultColumnNames.aggregation_level]
        == aggregation_level.value
    )


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
        (EnergyResultColumnNames.grid_area_code, DEFAULT_GRID_AREA_CODE),
        (EnergyResultColumnNames.neighbor_grid_area_code, DEFAULT_FROM_GRID_AREA_CODE),
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
def test__create__with_correct_row_values(
    spark: SparkSession,
    column_name: str,
    column_value: Any,
    args: CalculatorArgs,
) -> None:
    # Arrange
    row = [_create_result_row()]
    result_df = _create_energy_results(spark, row)
    args.calculation_id = DEFAULT_CALCULATION_ID

    # Act
    actual = sut.create(
        args,
        result_df,
        DEFAULT_TIME_SERIES_TYPE,
        DEFAULT_AGGREGATION_LEVEL,
    )

    # Assert
    assert actual.collect()[0][column_name] == column_value


def test__create__with_correct_number_of_calculation_result_ids(
    spark: SparkSession,
    contracts_path: str,
    args: CalculatorArgs,
) -> None:
    # Arrange
    result_df = _create_energy_results_corresponding_to_four_calculation_results(spark)
    EXPECTED_NUMBER_OF_CALCULATION_RESULT_IDS = 4

    # Act
    actual = sut.create(
        args,
        result_df,
        DEFAULT_TIME_SERIES_TYPE,
        DEFAULT_AGGREGATION_LEVEL,
    )

    # Assert
    distinct_calculation_result_ids = (
        actual.where(col(EnergyResultColumnNames.calculation_id) == args.calculation_id)
        .select(col(EnergyResultColumnNames.calculation_result_id))
        .distinct()
        .count()
    )
    assert distinct_calculation_result_ids == EXPECTED_NUMBER_OF_CALCULATION_RESULT_IDS


@pytest.mark.parametrize(
    "column_name, value, other_value",
    [
        (Colname.grid_area_code, DEFAULT_GRID_AREA_CODE, OTHER_GRID_AREA_CODE),
        (
            Colname.from_grid_area_code,
            DEFAULT_FROM_GRID_AREA_CODE,
            OTHER_FROM_GRID_AREA_CODE,
        ),
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
def test__create__when_rows_belong_to_different_results__adds_different_calculation_result_id(
    spark: SparkSession,
    column_name: str,
    value: Any,
    other_value: Any,
    args: CalculatorArgs,
) -> None:
    # Arrange
    row1 = _create_result_row()
    row1[column_name] = value
    row2 = _create_result_row()
    row2[column_name] = other_value
    result_df = _create_energy_results(spark, [row1, row2])

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
        (Colname.quantity, Decimal(DEFAULT_QUANTITY), Decimal(OTHER_QUANTITY)),
        (
            Colname.quality,
            DEFAULT_QUALITY.value,
            OTHER_QUALITY.value,
        ),
        (Colname.to_grid_area_code, DEFAULT_TO_GRID_AREA_CODE, OTHER_TO_GRID_AREA_CODE),
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
        (Colname.observation_time, DEFAULT_OBSERVATION_TIME, OTHER_OBSERVATION_TIME),
    ],
)
def test__write__when_rows_belong_to_same_result__adds_same_calculation_result_id(
    spark: SparkSession,
    column_name: str,
    value: Any,
    other_value: Any,
    args: CalculatorArgs,
) -> None:
    # Arrange
    row1 = _create_result_row(grid_area_code="804")
    row1[column_name] = value
    row2 = _create_result_row(grid_area_code="803")
    row2[column_name] = other_value
    result_df = _create_energy_results(spark, [row1, row2])

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
        EnergyResultColumnNames.metering_point_type,
        # The field that defines results
        EnergyResultColumnNames.calculation_result_id,
        EnergyResultColumnNames.result_id,
        EnergyResultColumnNames.metering_point_id,
        EnergyResultColumnNames.resolution,
        EnergyResultColumnNames.balance_responsible_party_id,  # Remove from this list when switching to this from balance_responsible_id
    ]
    all_columns = _get_energy_result_column_names()

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
    if field_name == Colname.grid_area_code:
        return EnergyResultColumnNames.grid_area_code
    if field_name == Colname.from_grid_area_code:
        return EnergyResultColumnNames.neighbor_grid_area_code
    if field_name == Colname.balance_responsible_id:
        return EnergyResultColumnNames.balance_responsible_id
    if field_name == Colname.balance_responsible_party_id:
        return EnergyResultColumnNames.balance_responsible_party_id
    if field_name == Colname.energy_supplier_id:
        return EnergyResultColumnNames.energy_supplier_id
    return field_name


def _get_energy_result_column_names() -> List[str]:
    return [
        getattr(EnergyResultColumnNames, key)
        for key in dir(EnergyResultColumnNames)
        if not key.startswith("__")
    ]
