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


from decimal import Decimal
from datetime import datetime
from typing import Callable
import pytest
from pyspark.sql import DataFrame, Row, SparkSession
from pyspark.sql.functions import lit
from pyspark.sql.types import DecimalType

from package.calculation.energy.energy_results import (
    EnergyResults,
    energy_results_schema,
)
from package.common import assert_schema
from package.constants import Colname

from tests.calculation.dataframe_defaults import DataframeDefaults


@pytest.fixture(scope="module")
def dataframe_with_energy_result_schema_factory(
    spark: SparkSession,
) -> Callable[..., DataFrame]:
    def factory(
        grid_area: str = DataframeDefaults.default_grid_area,
        to_grid_area: str | None = None,
        from_grid_area: str | None = None,
        balance_responsible_id: str | None = None,
        energy_supplier_id: str | None = None,
        time_window_start: datetime = DataframeDefaults.default_time_window_start,
        time_window_end: datetime = DataframeDefaults.default_time_window_end,
        sum_quantity: Decimal = DataframeDefaults.default_sum_quantity,
        quality: str = DataframeDefaults.default_quality,
        metering_point_type: str = DataframeDefaults.default_metering_point_type,
        settlement_method: str | None = None,
    ) -> DataFrame:
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
            Colname.sum_quantity: sum_quantity,
            Colname.qualities: [quality],
            Colname.metering_point_type: metering_point_type,
            Colname.settlement_method: settlement_method,
        }

        return spark.createDataFrame([Row(**row)], schema=energy_results_schema)

    return factory


class TestCtor:
    class TestWhenNullableColumnsAreMissingInInputDataframe:
        def test_returns_dataframe_that_includes_missing_column(
            self,
            dataframe_with_energy_result_schema_factory,
        ) -> None:
            # Arrange
            df = dataframe_with_energy_result_schema_factory()
            nullable_column = Colname.energy_supplier_id
            df_with_missing_columns = df.drop(nullable_column)

            # Act
            actual = EnergyResults(df_with_missing_columns)

            # Assert
            assert energy_results_schema[nullable_column].nullable is True
            assert nullable_column in actual.df.schema.fieldNames()

    class TestWhenMismatchInNullability:
        def test_respects_nullability_of_input_dataframe(
            self,
            dataframe_with_energy_result_schema_factory,
        ) -> None:
            # Arrange
            df = dataframe_with_energy_result_schema_factory()
            df = df.withColumn(Colname.sum_quantity, lit(None).cast(DecimalType(18, 6)))

            # Act
            actual = EnergyResults(df)

            # Assert
            assert energy_results_schema[Colname.sum_quantity].nullable is False
            assert actual.df.schema[Colname.sum_quantity].nullable is True

    class TestWhenValidInput:
        def test_returns_expected_dataframe(
            self,
            dataframe_with_energy_result_schema_factory: Callable[..., DataFrame],
        ) -> None:
            # Arrange
            df = dataframe_with_energy_result_schema_factory()

            # Act
            actual = EnergyResults(df)

            # Assert
            assert actual.df.collect() == df.collect()

    class TestWhenInputContainsIrrelevantColumn:
        def test_returns_schema_without_irrelevant_column(
            self,
            dataframe_with_energy_result_schema_factory,
        ) -> None:
            # Arrange
            df = dataframe_with_energy_result_schema_factory()
            df.withColumn("irrelevant_column", lit("test"))

            # Act
            actual = EnergyResults(df)

            # Assert
            assert_schema(
                actual.df.schema,
                energy_results_schema,
                ignore_nullability=True,
                ignore_decimal_precision=True,
                ignore_decimal_scale=True,
            )
