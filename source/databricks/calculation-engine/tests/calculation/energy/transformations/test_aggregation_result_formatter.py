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
    QuantityQuality,
)
from package.calculation.energy.transformations import (
    create_dataframe_from_aggregation_result_schema,
)
from package.calculation.energy.schemas import aggregation_result_schema
import pytest
import pandas as pd
from tests.calculation.dataframe_defaults import DataframeDefaults
from package.constants import Colname
from typing import Callable
from pyspark.sql import DataFrame, SparkSession


@pytest.fixture(scope="module")
def aggregation_result_factory(spark: SparkSession) -> Callable[..., DataFrame]:
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
        position: int | None = None,
    ) -> DataFrame:
        pandas_df = pd.DataFrame().append(
            [
                {
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
                    Colname.quality: quality,
                    Colname.metering_point_type: metering_point_type,
                    Colname.settlement_method: settlement_method,
                    Colname.position: position,
                }
            ],
            ignore_index=True,
        )

        return spark.createDataFrame(pandas_df, schema=aggregation_result_schema)

    return factory


@pytest.fixture(scope="module")
def agg_result_factory(spark: SparkSession) -> Callable[..., DataFrame]:
    def factory(
        grid_area: str = "A",
        start: datetime = datetime(2020, 1, 1, 0, 0),
        end: datetime = datetime(2020, 1, 1, 1, 0),
        resolution: str = MeteringPointResolution.HOUR.value,
        sum_quantity: Decimal = Decimal("1.234"),
        quality: str = QuantityQuality.ESTIMATED.value,
        metering_point_type: str = MeteringPointType.CONSUMPTION.value,
    ) -> DataFrame:
        return spark.createDataFrame(
            pd.DataFrame().append(
                [
                    {
                        Colname.grid_area: grid_area,
                        Colname.time_window: {Colname.start: start, Colname.end: end},
                        Colname.resolution: resolution,
                        Colname.sum_quantity: sum_quantity,
                        Colname.quality: quality,
                        Colname.metering_point_type: metering_point_type,
                    }
                ],
                ignore_index=True,
            )
        )

    return factory


def test__create_dataframe_from_aggregation_result_schema__can_create_a_dataframe_that_match_aggregation_result_schema(
    agg_result_factory: Callable[..., DataFrame],
) -> None:
    # Arrange
    result = agg_result_factory()
    # Act
    actual = create_dataframe_from_aggregation_result_schema(result)
    # Assert
    assert actual.schema == aggregation_result_schema


def test__create_dataframe_from_aggregation_result_schema__match_expected_dataframe(
    agg_result_factory: Callable[..., DataFrame],
    aggregation_result_factory: Callable[..., DataFrame],
) -> None:
    # Arrange
    result = agg_result_factory()
    expected = aggregation_result_factory(
        grid_area="A",
        time_window_start=datetime(2020, 1, 1, 0, 0),
        time_window_end=datetime(2020, 1, 1, 1, 0),
        sum_quantity=Decimal("1.234"),
        quality=QuantityQuality.ESTIMATED.value,
        metering_point_type=MeteringPointType.CONSUMPTION.value,
    )
    # Act
    actual = create_dataframe_from_aggregation_result_schema(result)
    # Assert
    assert actual.collect() == expected.collect()
