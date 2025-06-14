import uuid
from copy import copy
from datetime import datetime
from decimal import Decimal
from typing import Any, List

import pytest
from pyspark.sql import SparkSession
from pyspark.sql.functions import col

import geh_wholesale.codelists as e
from geh_wholesale.calculation.calculator_args import CalculatorArgs
from geh_wholesale.calculation.energy.data_structures.energy_results import (
    EnergyResults,
    energy_results_schema,
)
from geh_wholesale.constants import Colname
from geh_wholesale.databases.table_column_names import TableColumnNames
from geh_wholesale.databases.wholesale_results_internal import (
    energy_storage_model_factory as sut,
)
from geh_wholesale.databases.wholesale_results_internal.schemas import energy_schema

# The calculation id is used in parameterized test executed using xdist, which does not allow parameters to change
DEFAULT_CALCULATION_ID = "0b15a420-9fc8-409a-a169-fbd49479d718"
DEFAULT_GRID_AREA_CODE = "105"
DEFAULT_FROM_GRID_AREA_CODE = "106"
DEFAULT_TO_GRID_AREA_CODE = "107"
DEFAULT_ENERGY_SUPPLIER_ID = "9876543210123"
DEFAULT_BALANCE_RESPONSIBLE_PARTY_ID = "1234567890123"
DEFAULT_CALCULATION_TYPE = e.CalculationType.BALANCE_FIXING
DEFAULT_CALCULATION_EXECUTION_START = datetime(2022, 6, 10, 13, 15)
DEFAULT_QUANTITY = "1.1"
DEFAULT_QUALITY = e.QuantityQuality.MEASURED
DEFAULT_TIME_SERIES_TYPE = e.TimeSeriesType.PRODUCTION
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


@pytest.fixture
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
    balance_responsible_id: str = DEFAULT_BALANCE_RESPONSIBLE_PARTY_ID,
    quantity: str = DEFAULT_QUANTITY,
    quality: e.QuantityQuality = DEFAULT_QUALITY,
    observation_time: datetime = DEFAULT_OBSERVATION_TIME,
    metering_point_id: str | None = None,
) -> dict:
    row = {
        Colname.grid_area_code: grid_area_code,
        Colname.to_grid_area_code: to_grid_area_code,
        Colname.from_grid_area_code: from_grid_area_code,
        Colname.balance_responsible_party_id: balance_responsible_id,
        Colname.energy_supplier_id: energy_supplier_id,
        Colname.observation_time: observation_time,
        Colname.quantity: Decimal(quantity),
        Colname.qualities: [quality.value],
        Colname.settlement_method: [],
        Colname.metering_point_id: metering_point_id,
    }

    return row


def _create_energy_results(spark: SparkSession, row: List[dict]) -> EnergyResults:
    df = spark.createDataFrame(data=row, schema=energy_results_schema)
    return EnergyResults(df)


def _create_energy_results_corresponding_to_four_calculation_results(
    spark: SparkSession,
) -> EnergyResults:
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
    ("column_name", "column_value"),
    [
        (TableColumnNames.calculation_id, DEFAULT_CALCULATION_ID),
        (
            TableColumnNames.calculation_execution_time_start,
            DEFAULT_CALCULATION_EXECUTION_START,
        ),
        (TableColumnNames.calculation_type, DEFAULT_CALCULATION_TYPE.value),
        (TableColumnNames.time_series_type, DEFAULT_TIME_SERIES_TYPE.value),
        (TableColumnNames.grid_area_code, DEFAULT_GRID_AREA_CODE),
        (TableColumnNames.neighbor_grid_area_code, DEFAULT_FROM_GRID_AREA_CODE),
        (
            TableColumnNames.balance_responsible_party_id,
            DEFAULT_BALANCE_RESPONSIBLE_PARTY_ID,
        ),
        (TableColumnNames.energy_supplier_id, DEFAULT_ENERGY_SUPPLIER_ID),
        (TableColumnNames.time, datetime(2020, 1, 1, 0, 0)),
        (TableColumnNames.quantity, Decimal("1.100")),
        (TableColumnNames.quantity_qualities, [DEFAULT_QUALITY.value]),
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
    )

    # Assert
    assert actual.collect()[0][column_name] == column_value


def test__create__with_correct_number_of_calculation_result_ids(spark: SparkSession, args: CalculatorArgs) -> None:
    # Arrange
    result_df = _create_energy_results_corresponding_to_four_calculation_results(spark)
    EXPECTED_NUMBER_OF_CALCULATION_RESULT_IDS = 4

    # Act
    actual = sut.create(
        args,
        result_df,
        DEFAULT_TIME_SERIES_TYPE,
    )

    # Assert
    distinct_calculation_result_ids = (
        actual.where(col(TableColumnNames.calculation_id) == args.calculation_id)
        .select(col(TableColumnNames.result_id))
        .distinct()
        .count()
    )
    assert distinct_calculation_result_ids == EXPECTED_NUMBER_OF_CALCULATION_RESULT_IDS


@pytest.mark.parametrize(
    ("column_name", "value", "other_value"),
    [
        (Colname.grid_area_code, DEFAULT_GRID_AREA_CODE, OTHER_GRID_AREA_CODE),
        (
            Colname.from_grid_area_code,
            DEFAULT_FROM_GRID_AREA_CODE,
            OTHER_FROM_GRID_AREA_CODE,
        ),
        (
            Colname.balance_responsible_party_id,
            DEFAULT_BALANCE_RESPONSIBLE_PARTY_ID,
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
    )

    # Assert
    rows = actual.collect()
    assert rows[0][TableColumnNames.result_id] != rows[1][TableColumnNames.result_id]


@pytest.mark.parametrize(
    ("column_name", "value", "other_value"),
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
    )

    # Assert
    rows = actual.collect()
    assert rows[0][TableColumnNames.result_id] != rows[1][TableColumnNames.result_id]


def test__get_column_group_for_calculation_result_id__excludes_expected_other_column_names() -> None:
    # This class is a guard against adding new columns without considering how the column affects the generation of calculation result IDs

    # Arrange
    expected_other_columns = [
        # Data that doesn't vary for rows in a data frame
        # Data that does vary but does not define distinct results
        TableColumnNames.time,
        TableColumnNames.quantity_qualities,
        TableColumnNames.quantity,
        # The field that defines results
        TableColumnNames.resolution,
        TableColumnNames.result_id,
    ]
    all_columns = _get_energy_result_column_names()

    # Act
    included_columns = sut._get_column_group_for_calculation_result_id()

    # Assert
    actual_other_columns = set(all_columns) - set(included_columns)

    assert set(actual_other_columns) == set(expected_other_columns)


def _get_energy_result_column_names() -> List[str]:
    return [f.name for f in energy_schema.fields]
