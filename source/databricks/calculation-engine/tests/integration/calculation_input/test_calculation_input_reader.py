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
import pytest
from pyspark.sql import SparkSession
from package.codelists import (
    InputMeteringPointType,
    InputSettlementMethod,
    MeteringPointType,
    SettlementMethod,
)
from package.calculation_input import CalculationInputReader
from package.calculation_input.schemas import metering_point_period_schema
from package.constants import Colname
from package.infrastructure import paths


METERING_POINT_PERIODS_TABLE_NAME = f"{paths.INPUT_DATABASE_NAME}.{paths.METERING_POINT_PERIODS_TABLE_NAME}"


def _overwrite_table_data_with_new_row(
    spark: SparkSession,
    metering_point_type: InputMeteringPointType = InputMeteringPointType.CONSUMPTION,
    settlement_method: InputSettlementMethod = InputSettlementMethod.FLEX,
) -> None:
    row = {
        Colname.metering_point_id: "foo",
        Colname.metering_point_type: metering_point_type.value,
        Colname.calculation_type: "foo",
        Colname.settlement_method: settlement_method.value,
        Colname.grid_area: "foo",
        Colname.resolution: "foo",
        Colname.from_grid_area: "foo",
        Colname.to_grid_area: "foo",
        Colname.parent_metering_point_id: "foo",
        Colname.energy_supplier_id: "foo",
        Colname.balance_responsible_id: "foo",
        Colname.from_date: datetime(2022, 6, 8, 22, 0, 0),
        Colname.to_date: datetime(2022, 6, 8, 22, 0, 0),
    }

    df = spark.createDataFrame(data=[row], schema=metering_point_period_schema)
    df.write.format("delta").mode("overwrite").insertInto(METERING_POINT_PERIODS_TABLE_NAME)


@pytest.mark.parametrize("metering_point_type,expected", [
    [InputMeteringPointType.CONSUMPTION, MeteringPointType.CONSUMPTION],
    [InputMeteringPointType.PRODUCTION, MeteringPointType.PRODUCTION],
    [InputMeteringPointType.EXCHANGE, MeteringPointType.EXCHANGE],
    [InputMeteringPointType.VE_PRODUCTION, MeteringPointType.VE_PRODUCTION],
    [InputMeteringPointType.NET_PRODUCTION, MeteringPointType.NET_PRODUCTION],
    [InputMeteringPointType.SUPPLY_TO_GRID, MeteringPointType.SUPPLY_TO_GRID],
    [InputMeteringPointType.CONSUMPTION_FROM_GRID, MeteringPointType.CONSUMPTION_FROM_GRID],
    [InputMeteringPointType.WHOLESALE_SERVICES_INFORMATION, MeteringPointType.WHOLESALE_SERVICES_INFORMATION],
    [InputMeteringPointType.OWN_PRODUCTION, MeteringPointType.OWN_PRODUCTION],
    [InputMeteringPointType.NET_FROM_GRID, MeteringPointType.NET_FROM_GRID],
    [InputMeteringPointType.NET_TO_GRID, MeteringPointType.NET_TO_GRID],
    [InputMeteringPointType.TOTAL_CONSUMPTION, MeteringPointType.TOTAL_CONSUMPTION],
    [InputMeteringPointType.ELECTRICAL_HEATING, MeteringPointType.ELECTRICAL_HEATING],
    [InputMeteringPointType.NET_CONSUMPTION, MeteringPointType.NET_CONSUMPTION],
    [InputMeteringPointType.EFFECT_SETTLEMENT, MeteringPointType.EFFECT_SETTLEMENT],
])
def test___read_metering_point_periods__returns_df_with_correct_metering_point_types(
        spark: SparkSession,
        energy_input_data_written_to_delta: None,  # The purpose is to have the input tables available
        metering_point_type: InputMeteringPointType,
        expected: MeteringPointType) -> None:
    # Arrange
    _overwrite_table_data_with_new_row(spark, metering_point_type=metering_point_type)

    # Act
    actual = CalculationInputReader(spark).read_metering_point_periods()

    # Assert
    assert actual.collect()[0][Colname.metering_point_type] == expected.value


@pytest.mark.parametrize("settlement_method,expected", [
    [InputSettlementMethod.FLEX, SettlementMethod.FLEX],
    [InputSettlementMethod.NON_PROFILED, SettlementMethod.NON_PROFILED],
])
def test___read_metering_point_periods__returns_df_with_correct_settlemet_methods(
        spark: SparkSession,
        energy_input_data_written_to_delta: None,  # The purpose is to have the input tables available
        settlement_method: InputSettlementMethod,
        expected: SettlementMethod) -> None:
    # Arrange
    _overwrite_table_data_with_new_row(spark, settlement_method=settlement_method)

    # Act
    actual = CalculationInputReader(spark).read_metering_point_periods()

    # Assert
    assert actual.collect()[0][Colname.settlement_method] == expected.value
