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
from typing import List

from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.functions import col
import pytest
import uuid

from package.codelists import (
    AggregationLevel,
    MeteringPointResolution,
    ProcessType,
    TimeSeriesQuality,
    TimeSeriesType,
)
from package.constants import Colname, EnergyResultColumnNames
from package.infrastructure.paths import OUTPUT_DATABASE_NAME, ENERGY_RESULT_TABLE_NAME
from package.calculation_output import EnergyCalculationResultWriter
from package.calculation_output.energy_calculation_result_writer import (
    _get_column_group_for_calculation_result_id,
)
from tests.contract_utils import (
    assert_contract_matches_schema,
    get_column_names_from_contract,
)
from typing import Any

DEFAULT_BATCH_ID = "0b15a420-9fc8-409a-a169-fbd49479d718"
DEFAULT_GRID_AREA = "105"
DEFAULT_FROM_GRID_AREA = "106"
DEFAULT_ENERGY_SUPPLIER_ID = "9876543210123"
DEFAULT_BALANCE_RESPONSIBLE_ID = "1234567890123"
DEFAULT_PROCESS_TYPE = ProcessType.BALANCE_FIXING
DEFAULT_BATCH_EXECUTION_START = datetime(2022, 6, 10, 13, 15)
DEFAULT_QUANTITY = "1.1"
DEFAULT_QUALITY = TimeSeriesQuality.MEASURED
DEFAULT_TIME_SERIES_TYPE = TimeSeriesType.PRODUCTION
DEFAULT_AGGREGATION_LEVEL = AggregationLevel.TOTAL_GA
DEFAULT_TIME_WINDOW_START = datetime(2020, 1, 1, 0, 0)
DEFAULT_TIME_WINDOW_END = datetime(2020, 1, 1, 1, 0)

TABLE_NAME = f"{OUTPUT_DATABASE_NAME}.{ENERGY_RESULT_TABLE_NAME}"


def _create_result_row(
    grid_area: str = DEFAULT_GRID_AREA,
    from_grid_area: str = DEFAULT_FROM_GRID_AREA,
    energy_supplier_id: str = DEFAULT_ENERGY_SUPPLIER_ID,
    balance_responsible_id: str = DEFAULT_BALANCE_RESPONSIBLE_ID,
    quantity: str = DEFAULT_QUANTITY,
    quality: TimeSeriesQuality = DEFAULT_QUALITY,
    time_window_start: datetime = DEFAULT_TIME_WINDOW_START,
    time_window_end: datetime = DEFAULT_TIME_WINDOW_END,
) -> dict:
    row = {
        Colname.grid_area: grid_area,
        Colname.from_grid_area: from_grid_area,
        Colname.sum_quantity: Decimal(quantity),
        Colname.quality: quality.value,
        Colname.resolution: MeteringPointResolution.QUARTER.value,
        Colname.time_window: {
            Colname.start: time_window_start,
            Colname.end: time_window_end,
        },
        Colname.energy_supplier_id: energy_supplier_id,
        Colname.balance_responsible_id: balance_responsible_id,
    }

    return row


def _create_result_df(spark: SparkSession, row: List[dict]) -> DataFrame:
    return spark.createDataFrame(data=row).withColumn(
        Colname.sum_quantity, col(Colname.sum_quantity).cast("decimal(18, 3)")
    )


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

    return spark.createDataFrame(data=rows).withColumn(
        Colname.sum_quantity, col(Colname.sum_quantity).cast("decimal(18, 3)")
    )


@pytest.mark.parametrize(
    "aggregation_level",
    AggregationLevel,
)
def test__write__writes_aggregation_level(
    spark: SparkSession,
    aggregation_level: AggregationLevel,
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
        TimeSeriesType.PRODUCTION,
        aggregation_level,
    )

    # Assert
    actual_df = spark.read.table(TABLE_NAME).where(
        col(EnergyResultColumnNames.calculation_id) == batch_id
    )
    assert actual_df.collect()[0][Colname.aggregation_level] == aggregation_level.value


# The batch id is used in parameterized test executed using xdist, which does not allow parameters to change
batch_id = "0b15a420-9fc8-409a-a169-fbd49479d718"  # Needed in both test param and test implementation


@pytest.mark.parametrize(
    "column_name, column_value",
    [
        (EnergyResultColumnNames.calculation_id, batch_id),
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
        (EnergyResultColumnNames.quantity_quality, DEFAULT_QUALITY.value),
        (EnergyResultColumnNames.aggregation_level, DEFAULT_AGGREGATION_LEVEL.value),
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
        batch_id,
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
        col(EnergyResultColumnNames.calculation_id) == batch_id
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
        batch_id,
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
        col(EnergyResultColumnNames.calculation_id) == batch_id
    )

    assert_contract_matches_schema(contract_path, actual_df.schema)


def test__write__writes_calculation_result_id(
    spark: SparkSession, contracts_path: str, migrations_executed_per_test: None
) -> None:
    # Arrange
    result_df = _create_result_df_corresponding_to_four_calculation_results(spark)
    EXPECTED_NUMBER_OF_CALCULATION_RESULT_IDS = 4
    sut = EnergyCalculationResultWriter(
        batch_id,
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
    actual_df = spark.read.table(TABLE_NAME).select(
        col(EnergyResultColumnNames.calculation_result_id)
    )

    assert actual_df.distinct().count() == EXPECTED_NUMBER_OF_CALCULATION_RESULT_IDS


def test__get_column_group_for_calculation_result_id__returns_expected_column_names() -> (
    None
):
    # Arrange
    expected_column_names = [
        EnergyResultColumnNames.calculation_id,
        EnergyResultColumnNames.calculation_execution_time_start,
        EnergyResultColumnNames.calculation_type,
        EnergyResultColumnNames.grid_area,
        EnergyResultColumnNames.time_series_type,
        EnergyResultColumnNames.aggregation_level,
        EnergyResultColumnNames.from_grid_area,
        EnergyResultColumnNames.balance_responsible_id,
        EnergyResultColumnNames.energy_supplier_id,
    ]

    # Act
    actual = _get_column_group_for_calculation_result_id()

    # Assert
    assert actual == expected_column_names


def test__get_column_group_for_calculation_result_id__excludes_exepected_other_column_names(
    contracts_path: str,
) -> None:
    # This class is a guard against adding new columns without considering how the column affects the generation of calculation result IDs

    # Arrange
    expected_other_columns = [
        EnergyResultColumnNames.time,
        EnergyResultColumnNames.quantity_quality,
        EnergyResultColumnNames.quantity,
        EnergyResultColumnNames.calculation_result_id,
    ]
    contract_path = f"{contracts_path}/energy-result-table-column-names.json"
    all_columns = get_column_names_from_contract(contract_path)

    # Act
    included_columns = _get_column_group_for_calculation_result_id()
    actual_other_columns = set(all_columns) - set(included_columns)

    # Assert
    assert set(actual_other_columns) == set(expected_other_columns)
