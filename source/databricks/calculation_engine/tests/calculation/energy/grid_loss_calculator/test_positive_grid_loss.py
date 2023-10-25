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
from pyspark.sql import DataFrame, SparkSession
from pyspark.sql.types import (
    StructType,
    StringType,
    DecimalType,
    TimestampType,
    ArrayType,
)
from pyspark.sql.functions import col
import pytest
import pandas as pd
from typing import Callable

from package.calculation.energy.energy_results import (
    energy_results_schema,
    EnergyResults,
)
from package.codelists import (
    MeteringPointType,
    QuantityQuality,
)
from package.common import assert_schema
from package.constants import Colname
from package.calculation.energy.grid_loss_calculator import calculate_positive_grid_loss


@pytest.fixture(scope="module")
def grid_loss_schema() -> StructType:
    return (
        StructType()
        .add(Colname.grid_area, StringType(), False)
        .add(
            Colname.time_window,
            StructType()
            .add(Colname.start, TimestampType())
            .add(Colname.end, TimestampType()),
            False,
        )
        .add(Colname.sum_quantity, DecimalType(18, 3))
        .add(Colname.qualities, ArrayType(StringType(), False), False)
        .add(Colname.metering_point_type, StringType())
    )


@pytest.fixture(scope="module")
def agg_result_factory(
    spark: SparkSession, grid_loss_schema: StructType
) -> Callable[[], DataFrame]:
    """
    Factory to generate a single row of time series data, with default parameters as specified above.
    """

    def factory() -> DataFrame:
        pandas_df = pd.DataFrame(
            {
                Colname.grid_area: [],
                Colname.time_window: [],
                Colname.sum_quantity: [],
                Colname.qualities: [],
                Colname.metering_point_type: [],
            }
        )
        pandas_df = pandas_df.append(
            [
                {
                    Colname.grid_area: str(1),
                    Colname.time_window: {
                        Colname.start: datetime(2020, 1, 1, 0, 0),
                        Colname.end: datetime(2020, 1, 1, 1, 0),
                    },
                    Colname.sum_quantity: Decimal(-12.567),
                    Colname.qualities: [QuantityQuality.ESTIMATED.value],
                    Colname.metering_point_type: MeteringPointType.EXCHANGE.value,
                },
                {
                    Colname.grid_area: str(2),
                    Colname.time_window: {
                        Colname.start: datetime(2020, 1, 1, 0, 0),
                        Colname.end: datetime(2020, 1, 1, 1, 0),
                    },
                    Colname.sum_quantity: Decimal(34.32),
                    Colname.qualities: [QuantityQuality.ESTIMATED.value],
                    Colname.metering_point_type: MeteringPointType.EXCHANGE.value,
                },
                {
                    Colname.grid_area: str(3),
                    Colname.time_window: {
                        Colname.start: datetime(2020, 1, 1, 0, 0),
                        Colname.end: datetime(2020, 1, 1, 1, 0),
                    },
                    Colname.sum_quantity: Decimal(0.0),
                    Colname.qualities: [QuantityQuality.ESTIMATED.value],
                    Colname.metering_point_type: MeteringPointType.EXCHANGE.value,
                },
            ],
            ignore_index=True,
        )

        return spark.createDataFrame(pandas_df, schema=grid_loss_schema)

    return factory


def call_calculate_grid_loss(
    agg_result_factory: Callable[[], EnergyResults]
) -> EnergyResults:
    df = agg_result_factory()
    return calculate_positive_grid_loss(df)


def test_grid_area_grid_loss_has_no_values_below_zero(
    agg_result_factory: Callable[[], EnergyResults]
) -> None:
    result = call_calculate_grid_loss(agg_result_factory)

    assert result.df.where(col(Colname.sum_quantity) < 0).count() == 0


def test_grid_area_grid_loss_changes_negative_values_to_zero(
    agg_result_factory: Callable[[], EnergyResults]
) -> None:
    result = call_calculate_grid_loss(agg_result_factory)

    assert result.df.collect()[0][Colname.sum_quantity] == Decimal("0.00000")


def test_grid_area_grid_loss_positive_values_will_not_change(
    agg_result_factory: Callable[[], EnergyResults]
) -> None:
    result = call_calculate_grid_loss(agg_result_factory)

    assert result.df.collect()[1][Colname.sum_quantity] == Decimal("34.32000")


def test_grid_area_grid_loss_values_that_are_zero_stay_zero(
    agg_result_factory: Callable[[], EnergyResults]
) -> None:
    result = call_calculate_grid_loss(agg_result_factory)

    assert result.df.collect()[2][Colname.sum_quantity] == Decimal("0.00000")


def test_returns_correct_schema(
    agg_result_factory: Callable[[], EnergyResults]
) -> None:
    """
    Aggregator should return the correct schema, including the proper fields for the aggregated quantity values
    and time window (from the single-hour resolution specified in the aggregator).
    """
    result = call_calculate_grid_loss(agg_result_factory)
    assert_schema(result.df.schema, energy_results_schema, ignore_nullability=True)
