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

# TODO BJM: Refactor to match what it now tests

from decimal import Decimal
from datetime import datetime
from typing import Callable
import pytest
import pandas as pd
from pyspark.sql import DataFrame, SparkSession
from pyspark.sql.functions import col, struct

from package.calculation.energy.energy_results import (
    EnergyResults,
    energy_results_schema,
)
from package.codelists import (
    MeteringPointType,
    QuantityQuality,
)
from package.common import assert_schema
from package.constants import Colname

from tests.calculation.dataframe_defaults import DataframeDefaults


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
                    Colname.qualities: [quality],
                    Colname.metering_point_type: metering_point_type,
                    Colname.settlement_method: settlement_method,
                }
            ],
            ignore_index=True,
        )

        return spark.createDataFrame(pandas_df, schema=energy_results_schema)

    return factory


@pytest.fixture(scope="module")
def input_agg_result_factory(spark: SparkSession) -> Callable[..., DataFrame]:
    def factory(
        grid_area: str = "A",
        start: datetime = datetime(2020, 1, 1, 0, 0),
        end: datetime = datetime(2020, 1, 1, 1, 0),
        sum_quantity: Decimal = Decimal("1.234"),
        quality: str = QuantityQuality.ESTIMATED.value,
        metering_point_type: str = MeteringPointType.CONSUMPTION.value,
    ) -> DataFrame:
        return spark.createDataFrame(
            pd.DataFrame().append(
                [
                    {
                        Colname.grid_area: grid_area,
                        Colname.to_grid_area: None,
                        Colname.from_grid_area: None,
                        Colname.balance_responsible_id: None,
                        Colname.energy_supplier_id: None,
                        Colname.time_window: {
                            Colname.start: start,
                            Colname.end: end,
                        },
                        Colname.sum_quantity: sum_quantity,
                        Colname.qualities: [quality],
                        Colname.metering_point_type: metering_point_type,
                        Colname.settlement_method: None,
                    }
                ],
                ignore_index=True,
            ),
            schema=energy_results_schema,
        ).withColumn(
            Colname.time_window,
            struct(
                col(Colname.time_window).getItem(Colname.start).alias(Colname.start),
                col(Colname.time_window).getItem(Colname.end).alias(Colname.end),
            ),
        )

    return factory


def test__create_dataframe_from_aggregation_result_schema__can_create_a_dataframe_that_match_aggregation_result_schema(
    input_agg_result_factory,
) -> None:
    # Arrange
    result = input_agg_result_factory()

    # Act
    actual = EnergyResults(result)

    # Assert
    assert_schema(
        actual.df.schema,
        energy_results_schema,
        ignore_nullability=True,
        ignore_decimal_precision=True,
        ignore_decimal_scale=True,
    )


def test__create_dataframe_from_aggregation_result_schema__match_expected_dataframe(
    input_agg_result_factory,
    aggregation_result_factory: Callable[..., DataFrame],
) -> None:
    # Arrange
    result = input_agg_result_factory()
    expected = aggregation_result_factory(
        grid_area="A",
        time_window_start=datetime(2020, 1, 1, 0, 0),
        time_window_end=datetime(2020, 1, 1, 1, 0),
        sum_quantity=Decimal("1.234"),
        quality=QuantityQuality.ESTIMATED.value,
        metering_point_type=MeteringPointType.CONSUMPTION.value,
    )
    # Act
    actual = EnergyResults(result)
    # Assert
    assert actual.df.collect() == expected.collect()
