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
from package.codelists import (
    MeteringPointType,
    MeteringPointResolution,
    TimeSeriesQuality,
)
from package.calculation.energy.grid_loss_calculator import calculate_negative_grid_loss
from package.calculation.energy.schemas import aggregation_result_schema
from package.calculation.energy.transformations import (
    create_dataframe_from_aggregation_result_schema,
)
from pyspark.sql import DataFrame, SparkSession
from pyspark.sql.types import StructType, StringType, DecimalType, TimestampType
from pyspark.sql.functions import col
import pytest
import pandas as pd
from package.constants import Colname
from typing import Callable


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
        .add(Colname.quality, StringType())
        .add(Colname.resolution, StringType())
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
                Colname.resolution: [],
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
                    Colname.quality: TimeSeriesQuality.ESTIMATED.value,
                    Colname.resolution: MeteringPointResolution.HOUR.value,
                    Colname.metering_point_type: MeteringPointType.EXCHANGE.value,
                },
                {
                    Colname.grid_area: str(2),
                    Colname.time_window: {
                        Colname.start: datetime(2020, 1, 1, 0, 0),
                        Colname.end: datetime(2020, 1, 1, 1, 0),
                    },
                    Colname.sum_quantity: Decimal(34.32),
                    Colname.quality: TimeSeriesQuality.ESTIMATED.value,
                    Colname.resolution: MeteringPointResolution.HOUR.value,
                    Colname.metering_point_type: MeteringPointType.EXCHANGE.value,
                },
                {
                    Colname.grid_area: str(3),
                    Colname.time_window: {
                        Colname.start: datetime(2020, 1, 1, 0, 0),
                        Colname.end: datetime(2020, 1, 1, 1, 0),
                    },
                    Colname.sum_quantity: Decimal(0.0),
                    Colname.quality: TimeSeriesQuality.ESTIMATED.value,
                    Colname.resolution: MeteringPointResolution.HOUR.value,
                    Colname.metering_point_type: MeteringPointType.EXCHANGE.value,
                },
            ],
            ignore_index=True,
        )

        return spark.createDataFrame(pandas_df, schema=grid_loss_schema)

    return factory


def call_calculate_negative_grid_loss(
    agg_result_factory: Callable[[], DataFrame]
) -> DataFrame:
    df = create_dataframe_from_aggregation_result_schema(agg_result_factory())
    return calculate_negative_grid_loss(df)


def test_negative_grid_loss_has_no_values_below_zero(
    agg_result_factory: Callable[[], DataFrame]
) -> None:
    result = call_calculate_negative_grid_loss(agg_result_factory)

    assert result.filter(col(Colname.sum_quantity) < 0).count() == 0


def test_negative_grid_loss_change_negative_value_to_positive(
    agg_result_factory: Callable[[], DataFrame]
) -> None:
    result = call_calculate_negative_grid_loss(agg_result_factory)

    assert result.collect()[0][Colname.sum_quantity] == Decimal("12.56700")


def test_negative_grid_loss_change_positive_value_to_zero(
    agg_result_factory: Callable[[], DataFrame]
) -> None:
    result = call_calculate_negative_grid_loss(agg_result_factory)

    assert result.collect()[1][Colname.sum_quantity] == Decimal("0.00000")


def test_negative_grid_loss_values_that_are_zero_stay_zero(
    agg_result_factory: Callable[[], DataFrame]
) -> None:
    result = call_calculate_negative_grid_loss(agg_result_factory)

    assert result.collect()[2][Colname.sum_quantity] == Decimal("0.00000")


def test_returns_correct_schema(agg_result_factory: Callable[[], DataFrame]) -> None:
    result = call_calculate_negative_grid_loss(agg_result_factory)
    assert result.schema == aggregation_result_schema
