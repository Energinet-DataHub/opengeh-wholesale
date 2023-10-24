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

from datetime import datetime, timedelta
from decimal import Decimal
from typing import Any, List
import uuid

from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.functions import col
import pytest

import package.codelists as e
from package.constants import Colname, EnergyResultColumnNames
from package.infrastructure.paths import OUTPUT_DATABASE_NAME, ENERGY_RESULT_TABLE_NAME
from package.calculation_output import EnergyCalculationResultWriter
from package.calculation_output.energy_calculation_result_writer import (
    _write_input_schema,
)

from tests.contract_utils import (
    assert_contract_matches_schema,
    get_column_names_from_contract,
)

# The batch id is used in parameterized test executed using xdist, which does not allow parameters to change
DEFAULT_BATCH_ID = "0b15a420-9fc8-409a-a169-fbd49479d718"
DEFAULT_GRID_AREA = "105"
DEFAULT_FROM_GRID_AREA = "106"
DEFAULT_TO_GRID_AREA = "107"
DEFAULT_ENERGY_SUPPLIER_ID = "9876543210123"
DEFAULT_BALANCE_RESPONSIBLE_ID = "1234567890123"
DEFAULT_PROCESS_TYPE = e.ProcessType.BALANCE_FIXING
DEFAULT_BATCH_EXECUTION_START = datetime(2022, 6, 10, 13, 15)
DEFAULT_QUANTITY = "1.1"
DEFAULT_QUALITY = e.QuantityQuality.MEASURED
DEFAULT_TIME_SERIES_TYPE = e.TimeSeriesType.PRODUCTION
DEFAULT_AGGREGATION_LEVEL = e.AggregationLevel.TOTAL_GA
DEFAULT_TIME_WINDOW_START = datetime(2020, 1, 1, 0, 0)
DEFAULT_TIME_WINDOW_END = datetime(2020, 1, 1, 1, 0)
DEFAULT_METERING_POINT_TYPE = e.MeteringPointType.PRODUCTION
DEFAULT_SETTLEMENT_METHOD = e.SettlementMethod.FLEX

OTHER_BATCH_ID = "0b15a420-9fc8-409a-a169-fbd49479d719"
OTHER_GRID_AREA = "205"
OTHER_FROM_GRID_AREA = "206"
OTHER_TO_GRID_AREA = "207"
OTHER_ENERGY_SUPPLIER_ID = "9876543210124"
OTHER_BALANCE_RESPONSIBLE_ID = "1234567890124"
OTHER_PROCESS_TYPE = e.ProcessType.AGGREGATION
OTHER_BATCH_EXECUTION_START = datetime(2023, 6, 10, 13, 15)
OTHER_QUANTITY = "1.2"
OTHER_QUALITY = e.QuantityQuality.CALCULATED
OTHER_TIME_SERIES_TYPE = e.TimeSeriesType.NON_PROFILED_CONSUMPTION
OTHER_AGGREGATION_LEVEL = e.AggregationLevel.ES_PER_GA
OTHER_TIME_WINDOW_START = datetime(2021, 1, 1, 0, 0)
OTHER_TIME_WINDOW_END = datetime(2021, 1, 1, 1, 0)
OTHER_METERING_POINT_TYPE = e.MeteringPointType.CONSUMPTION
OTHER_SETTLEMENT_METHOD = e.SettlementMethod.NON_PROFILED


TABLE_NAME = f"{OUTPUT_DATABASE_NAME}.{ENERGY_RESULT_TABLE_NAME}"


def _create_result_row(
    grid_area: str = DEFAULT_GRID_AREA,
    from_grid_area: str = DEFAULT_FROM_GRID_AREA,
    energy_supplier_id: str = DEFAULT_ENERGY_SUPPLIER_ID,
    balance_responsible_id: str = DEFAULT_BALANCE_RESPONSIBLE_ID,
    quantity: str = DEFAULT_QUANTITY,
    quality: e.QuantityQuality = DEFAULT_QUALITY,
    time_window_start: datetime = DEFAULT_TIME_WINDOW_START,
    time_window_end: datetime = DEFAULT_TIME_WINDOW_END,
) -> dict:
    row = {
        Colname.grid_area: grid_area,
        Colname.from_grid_area: from_grid_area,
        Colname.sum_quantity: Decimal(quantity),
        Colname.qualities: [quality.value],
        Colname.time_window: {
            Colname.start: time_window_start,
            Colname.end: time_window_end,
        },
        Colname.energy_supplier_id: energy_supplier_id,
        Colname.balance_responsible_id: balance_responsible_id,
        Colname.time_series_type: e.TimeSeriesType.PRODUCTION.value,
    }

    return row


def _create_result_df(spark: SparkSession, row: List[dict]) -> DataFrame:
    return spark.createDataFrame(data=row, schema=_write_input_schema)


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


def test__write__when_invalid_results_schema__raises_assertion_error(
    spark: SparkSession,
) -> None:
    # Arrange
    invalid_df = spark.createDataFrame([{"foo": 42}])
    sut = EnergyCalculationResultWriter(
        DEFAULT_BATCH_ID,
        DEFAULT_PROCESS_TYPE,
        DEFAULT_BATCH_EXECUTION_START,
    )

    # Act and assert
    with pytest.raises(AssertionError) as excinfo:
        sut.write(
            invalid_df,
            DEFAULT_TIME_SERIES_TYPE,
            DEFAULT_AGGREGATION_LEVEL,
        )
    assert "Schema mismatch" in str(excinfo)


def test__write__when_results_schema_missing_optional_column__does_not_raise(
    spark: SparkSession,
    migrations_executed: None,
) -> None:
    # Arrange
    row = [_create_result_row()]
    result = _create_result_df(spark, row)
    df_missing_optional_column = result.drop(col(Colname.balance_responsible_id))
    sut = EnergyCalculationResultWriter(
        DEFAULT_BATCH_ID,
        DEFAULT_PROCESS_TYPE,
        DEFAULT_BATCH_EXECUTION_START,
    )

    # Act and assert (implicitly that no error is raised)
    sut.write(
        df_missing_optional_column,
        DEFAULT_TIME_SERIES_TYPE,
        DEFAULT_AGGREGATION_LEVEL,
    )


@pytest.mark.parametrize(
    "aggregation_level",
    e.AggregationLevel,
)
def test__write__writes_aggregation_level(
    spark: SparkSession,
    aggregation_level: e.AggregationLevel,
    migrations_executed: None,
) -> None:
    # Arrange
    row = [_create_result_row()]
    result_df = _create_result_df(spark, row)
    batch_id = str(uuid.uuid4())
    sut = EnergyCalculationResultWriter(
        batch_id,
        DEFAULT_PROCESS_TYPE,
        DEFAULT_BATCH_EXECUTION_START,
    )

    # Act
    sut.write(
        result_df,
        e.TimeSeriesType.PRODUCTION,
        aggregation_level,
    )

    # Assert
    actual_df = spark.read.table(TABLE_NAME).where(
        col(EnergyResultColumnNames.calculation_id) == batch_id
    )
    assert actual_df.collect()[0][Colname.aggregation_level] == aggregation_level.value


@pytest.mark.parametrize(
    "column_name, column_value",
    [
        (EnergyResultColumnNames.calculation_id, DEFAULT_BATCH_ID),
        (
            EnergyResultColumnNames.calculation_execution_time_start,
            DEFAULT_BATCH_EXECUTION_START,
        ),
        (EnergyResultColumnNames.calculation_type, DEFAULT_PROCESS_TYPE.value),
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
) -> None:
    # Arrange
    row = [_create_result_row()]
    result_df = _create_result_df(spark, row)
    sut = EnergyCalculationResultWriter(
        DEFAULT_BATCH_ID,
        DEFAULT_PROCESS_TYPE,
        DEFAULT_BATCH_EXECUTION_START,
    )

    # Act
    sut.write(
        result_df,
        DEFAULT_TIME_SERIES_TYPE,
        DEFAULT_AGGREGATION_LEVEL,
    )

    # Assert
    actual_df = spark.read.table(TABLE_NAME).where(
        col(EnergyResultColumnNames.calculation_id) == DEFAULT_BATCH_ID
    )
    assert actual_df.collect()[0][column_name] == column_value


def test__write__writes_columns_matching_contract(
    spark: SparkSession,
    contracts_path: str,
    migrations_executed: None,
) -> None:
    # Arrange
    contract_path = f"{contracts_path}/energy-result-table-column-names.json"
    row = [_create_result_row()]
    result_df = _create_result_df(spark, row)
    sut = EnergyCalculationResultWriter(
        DEFAULT_BATCH_ID,
        DEFAULT_PROCESS_TYPE,
        DEFAULT_BATCH_EXECUTION_START,
    )

    # Act
    sut.write(
        result_df,
        DEFAULT_TIME_SERIES_TYPE,
        DEFAULT_AGGREGATION_LEVEL,
    )

    # Assert
    actual_df = spark.read.table(TABLE_NAME).where(
        col(EnergyResultColumnNames.calculation_id) == DEFAULT_BATCH_ID
    )

    assert_contract_matches_schema(contract_path, actual_df.schema)


def test__write__writes_calculation_result_id(
    spark: SparkSession, contracts_path: str, migrations_executed: None
) -> None:
    # Arrange
    result_df = _create_result_df_corresponding_to_four_calculation_results(spark)
    EXPECTED_NUMBER_OF_CALCULATION_RESULT_IDS = 4
    calculation_id = str(uuid.uuid4())
    sut = EnergyCalculationResultWriter(
        calculation_id,
        DEFAULT_PROCESS_TYPE,
        DEFAULT_BATCH_EXECUTION_START,
    )

    # Act
    sut.write(
        result_df,
        DEFAULT_TIME_SERIES_TYPE,
        DEFAULT_AGGREGATION_LEVEL,
    )

    # Assert
    actual_df = (
        spark.read.table(TABLE_NAME)
        .where(col(EnergyResultColumnNames.calculation_id) == calculation_id)
        .select(col(EnergyResultColumnNames.calculation_result_id))
    )

    assert actual_df.distinct().count() == EXPECTED_NUMBER_OF_CALCULATION_RESULT_IDS


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
) -> None:
    # Arrange
    row1 = _create_result_row()
    row1[column_name] = value
    row2 = _create_result_row()
    row2[column_name] = other_value
    result_df = _create_result_df(spark, [row1, row2])

    calculation_id = str(uuid.uuid4())
    sut = EnergyCalculationResultWriter(
        calculation_id,
        DEFAULT_PROCESS_TYPE,
        DEFAULT_BATCH_EXECUTION_START,
    )

    # Act
    sut.write(
        result_df,
        DEFAULT_TIME_SERIES_TYPE,
        DEFAULT_AGGREGATION_LEVEL,
    )

    # Assert
    actual = (
        spark.read.table(TABLE_NAME)
        .where(col(EnergyResultColumnNames.calculation_id) == calculation_id)
        .collect()
    )
    assert (
        actual[0][EnergyResultColumnNames.calculation_result_id]
        != actual[1][EnergyResultColumnNames.calculation_result_id]
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
) -> None:
    # Arrange
    row1 = _create_result_row()
    row1[column_name] = value
    row2 = _create_result_row()
    row2[column_name] = other_value
    result_df = _create_result_df(spark, [row1, row2])

    calculation_id = str(uuid.uuid4())
    sut = EnergyCalculationResultWriter(
        calculation_id,
        DEFAULT_PROCESS_TYPE,
        DEFAULT_BATCH_EXECUTION_START,
    )

    # Act
    sut.write(
        result_df,
        DEFAULT_TIME_SERIES_TYPE,
        DEFAULT_AGGREGATION_LEVEL,
    )

    # Assert
    actual = (
        spark.read.table(TABLE_NAME)
        .where(col(EnergyResultColumnNames.calculation_id) == calculation_id)
        .collect()
    )
    assert (
        actual[0][EnergyResultColumnNames.calculation_result_id]
        == actual[1][EnergyResultColumnNames.calculation_result_id]
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
    ]
    contract_path = f"{contracts_path}/energy-result-table-column-names.json"
    all_columns = get_column_names_from_contract(contract_path)

    # Act
    included_columns = (
        EnergyCalculationResultWriter._get_column_group_for_calculation_result_id()
    )

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
