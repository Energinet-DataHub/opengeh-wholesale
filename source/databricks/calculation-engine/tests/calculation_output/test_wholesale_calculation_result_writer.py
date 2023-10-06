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

from datetime import datetime
from decimal import Decimal
from typing import List

from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.functions import col
import pytest

from package.codelists import (
    ChargeQuality,
    ChargeResolution,
    ChargeType,
    ChargeUnit,
    MeteringPointType,
    ProcessType,
    SettlementMethod,
)
from package.constants import Colname, WholesaleResultColumnNames
from package.infrastructure.paths import (
    OUTPUT_DATABASE_NAME,
    WHOLESALE_RESULT_TABLE_NAME,
)
from package.calculation_output import WholesaleCalculationResultWriter
from typing import Any


TABLE_NAME = f"{OUTPUT_DATABASE_NAME}.{WHOLESALE_RESULT_TABLE_NAME}"

# Writer constructor parameters
DEFAULT_BATCH_ID = "0b15a420-9fc8-409a-a169-fbd49479d718"
DEFAULT_PROCESS_TYPE = ProcessType.FIRST_CORRECTION_SETTLEMENT
DEFAULT_BATCH_EXECUTION_START = datetime(2022, 6, 10, 13, 15)

# Input dataframe parameters
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
DEFAULT_RESOLUTION = ChargeResolution.HOUR
DEFAULT_CHARGE_PRICE = Decimal("0.756998")
DEFAULT_TOTAL_QUANTITY = Decimal("1.1")
DEFAULT_CHARGE_COUNT = "3"
DEFAULT_TOTAL_AMOUNT = Decimal("123.456")
DEFAULT_UNIT = ChargeUnit.KWH
DEFAULT_QUALITY = ChargeQuality.CALCULATED


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
    resolution: ChargeResolution = DEFAULT_RESOLUTION,
    charge_price: Decimal = DEFAULT_CHARGE_PRICE,
    total_quantity: Decimal = DEFAULT_TOTAL_QUANTITY,
    charge_count: str = DEFAULT_CHARGE_COUNT,
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
        Colname.charge_resolution: resolution.value,
        Colname.charge_price: charge_price,
        Colname.total_quantity: total_quantity,
        Colname.charge_count: charge_count,
        Colname.total_amount: total_amount,
        Colname.unit: unit.value,
        Colname.qualities: [quality.value],
    }

    return row


def _create_result_df(spark: SparkSession, row: List[dict]) -> DataFrame:
    return spark.createDataFrame(data=row)


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


@pytest.fixture(scope="session")
def sut() -> WholesaleCalculationResultWriter:
    return WholesaleCalculationResultWriter(
        DEFAULT_BATCH_ID,
        DEFAULT_PROCESS_TYPE,
        DEFAULT_BATCH_EXECUTION_START,
    )


@pytest.mark.parametrize(
    "column_name, column_value",
    [
        (WholesaleResultColumnNames.calculation_id, DEFAULT_BATCH_ID),
        (WholesaleResultColumnNames.calculation_type, DEFAULT_PROCESS_TYPE.value),
        (
            WholesaleResultColumnNames.calculation_execution_time_start,
            DEFAULT_BATCH_EXECUTION_START,
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
def test__write__writes_column(
    sut: WholesaleCalculationResultWriter,
    spark: SparkSession,
    column_name: str,
    column_value: Any,
    migrations_executed: None,
) -> None:
    """Test all columns except calculation_result_id. It is tested separatedly in another test."""

    # Arrange
    row = [_create_result_row()]
    result_df = _create_result_df(spark, row)

    # Act
    sut.write(result_df)

    # Assert
    actual_df = spark.read.table(TABLE_NAME).where(
        col(WholesaleResultColumnNames.calculation_id) == DEFAULT_BATCH_ID
    )
    actual_row = actual_df.collect()[0]

    assert actual_row[column_name] == column_value


def test__write__writes_calculation_result_id(
    sut: WholesaleCalculationResultWriter,
    spark: SparkSession,
    migrations_executed_per_test: None,
) -> None:
    # Arrange
    result_df = _create_result_df_corresponding_to_multiple_calculation_results(spark)
    expected_number_of_calculation_result_ids = 3

    # Act
    sut.write(result_df)

    # Assert
    actual_df = spark.read.table(TABLE_NAME).select(
        col(WholesaleResultColumnNames.calculation_result_id)
    )

    assert actual_df.distinct().count() == expected_number_of_calculation_result_ids


def test__get_column_group_for_calculation_result_id__returns_expected_column_names(
    sut: WholesaleCalculationResultWriter,
) -> None:
    # Arrange
    expected_column_names = [
        Colname.batch_id,
        Colname.charge_resolution,
        Colname.charge_type,
        Colname.grid_area,
        Colname.charge_owner,
        Colname.energy_supplier_id,
    ]

    # Act
    actual = sut._get_column_group_for_calculation_result_id()

    # Assert
    assert actual == expected_column_names


def test__get_column_group_for_calculation_result_id__excludes_expected_other_column_names(
    sut: WholesaleCalculationResultWriter,
) -> None:
    # This class is a guard against adding new columns without considering how the column affects the generation of
    # calculation result IDs

    # Arrange
    expected_excluded_columns = [
        WholesaleResultColumnNames.calculation_id,
        WholesaleResultColumnNames.calculation_type,
        WholesaleResultColumnNames.calculation_execution_time_start,
        WholesaleResultColumnNames.calculation_result_id,
        WholesaleResultColumnNames.grid_area,
        WholesaleResultColumnNames.quantity,
        WholesaleResultColumnNames.quantity_unit,
        WholesaleResultColumnNames.quantity_qualities,
        WholesaleResultColumnNames.time,
        WholesaleResultColumnNames.metering_point_type,
        WholesaleResultColumnNames.settlement_method,
        WholesaleResultColumnNames.price,
        WholesaleResultColumnNames.amount,
        WholesaleResultColumnNames.is_tax,
        WholesaleResultColumnNames.charge_code,
    ]
    all_columns = [
        attr for attr in dir(WholesaleResultColumnNames) if not attr.startswith("__")
    ]

    # Act
    included_columns = sut._get_column_group_for_calculation_result_id()

    # Assert
    excluded_columns = set(all_columns) - set(included_columns)
    assert set(excluded_columns) == set(expected_excluded_columns)
