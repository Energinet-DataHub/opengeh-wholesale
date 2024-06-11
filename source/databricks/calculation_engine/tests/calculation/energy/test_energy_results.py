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

import pytest
from pyspark.sql import SparkSession
from pyspark.sql.functions import lit
from pyspark.sql.types import DecimalType

import tests.calculation.energy.energy_results_factories as factory
from package.calculation.energy.data_structures.energy_results import (
    EnergyResults,
    energy_results_schema,
)
from package.constants import Colname


class TestCtor:
    class TestWhenNullableColumnsAreMissingInInputDataframe:
        def test_returns_dataframe_that_includes_missing_column(
            self, spark: SparkSession
        ) -> None:
            # Arrange
            df = factory.create(spark).df
            nullable_columns = [
                Colname.to_grid_area_code,
                Colname.from_grid_area_code,
                Colname.balance_responsible_id,
                Colname.energy_supplier_id,
                Colname.metering_point_id,
            ]
            df_with_missing_columns = df.drop(*nullable_columns)

            # Act
            actual = EnergyResults(df_with_missing_columns)

            # Assert
            assert set(nullable_columns).issubset(set(actual.df.schema.fieldNames()))

    class TestWhenMismatchInNullability:
        def test_accepts_nullability_of_input_dataframe(
            self, spark: SparkSession
        ) -> None:
            # Arrange
            df = factory.create(spark).df
            df = df.withColumn(Colname.quantity, lit(None).cast(DecimalType(18, 3)))

            # Act
            actual = EnergyResults(df)

            # Assert
            assert energy_results_schema[Colname.quantity].nullable is False
            assert actual.df.schema[Colname.quantity].nullable is True

    class TestWhenValidInput:
        def test_returns_expected_dataframe(self, spark: SparkSession) -> None:
            df = factory.create(spark).df

            actual = EnergyResults(df)

            assert actual.df.collect() == df.collect()

    class TestWhenInputContainsIrrelevantColumn:
        def test_returns_schema_without_irrelevant_column(
            self, spark: SparkSession
        ) -> None:
            # Arrange
            df = factory.create(spark).df
            irrelevant_column = "irrelevant_column"
            df = df.withColumn(irrelevant_column, lit("test"))

            # Act
            actual = EnergyResults(df)

            # Assert
            assert irrelevant_column not in actual.df.schema.fieldNames()

    class TestWhenInputDecimalScaleIsHigherThanThree:
        def test_raise_exception_if_scale_does_no_match_schema(
            self, spark: SparkSession
        ) -> None:
            """
            The quantity column in EnergyResult can be represented by 3 decimals.
            Time series has 3 decimals and those with resolution PT1H is divided by four (quarters).
            See rounding.py for details on how we deal with rounding.
            The end result should be stored with 3 decimals.
            """

            # Arrange
            df = factory.create(spark).df
            df = df.withColumn(
                Colname.quantity,
                lit(Decimal("0.12345678")).cast(DecimalType(18, 8)),
            )

            # Act & Assert
            with pytest.raises(AssertionError) as exc_info:
                EnergyResults(df)

            # Assert
            assert "Decimal scale error" in str(exc_info.value)
