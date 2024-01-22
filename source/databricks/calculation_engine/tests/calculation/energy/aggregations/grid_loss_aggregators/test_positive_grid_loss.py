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
from typing import Callable

import pandas as pd
import pytest
from pyspark.sql import SparkSession
from pyspark.sql.functions import col

from calculation.energy import grid_loss_responsible_factories
from package.calculation.energy.energy_results import (
    EnergyResults,
    energy_results_schema,
)
from package.calculation.energy.aggregators.grid_loss_aggregators import (
    calculate_positive_grid_loss,
)
from package.codelists import (
    QuantityQuality,
    MeteringPointType,
)
from package.constants import Colname


@pytest.fixture(scope="module")
def agg_result_factory(spark: SparkSession) -> Callable[[], EnergyResults]:
    """
    Factory to generate a single row of time series data, with default parameters as specified above.
    """

    def factory() -> EnergyResults:
        pandas_df = pd.DataFrame(
            {
                Colname.grid_area: [],
                Colname.to_grid_area: [],
                Colname.from_grid_area: [],
                Colname.balance_responsible_id: [],
                Colname.energy_supplier_id: [],
                Colname.time_window: [],
                Colname.sum_quantity: [],
                Colname.qualities: [],
                Colname.metering_point_id: [],
            }
        )
        pandas_df = pandas_df.append(
            [
                {
                    Colname.grid_area: str(1),
                    Colname.to_grid_area: None,
                    Colname.from_grid_area: None,
                    Colname.balance_responsible_id: "balance_responsible_id",
                    Colname.energy_supplier_id: "energy_supplier_id",
                    Colname.time_window: {
                        Colname.start: datetime(2020, 1, 1, 0, 0),
                        Colname.end: datetime(2020, 1, 1, 1, 0),
                    },
                    Colname.sum_quantity: Decimal(-12.567),
                    Colname.qualities: [QuantityQuality.ESTIMATED.value],
                },
                {
                    Colname.grid_area: str(2),
                    Colname.to_grid_area: None,
                    Colname.from_grid_area: None,
                    Colname.balance_responsible_id: "balance_responsible_id",
                    Colname.energy_supplier_id: "energy_supplier_id",
                    Colname.time_window: {
                        Colname.start: datetime(2020, 1, 1, 0, 0),
                        Colname.end: datetime(2020, 1, 1, 1, 0),
                    },
                    Colname.sum_quantity: Decimal(34.32),
                    Colname.qualities: [QuantityQuality.ESTIMATED.value],
                },
                {
                    Colname.grid_area: str(3),
                    Colname.to_grid_area: None,
                    Colname.from_grid_area: None,
                    Colname.balance_responsible_id: "balance_responsible_id",
                    Colname.energy_supplier_id: "energy_supplier_id",
                    Colname.time_window: {
                        Colname.start: datetime(2020, 1, 1, 0, 0),
                        Colname.end: datetime(2020, 1, 1, 1, 0),
                    },
                    Colname.sum_quantity: Decimal(0.0),
                    Colname.qualities: [QuantityQuality.ESTIMATED.value],
                    Colname.metering_point_id: None,
                },
            ],
            ignore_index=True,
        )

        df = spark.createDataFrame(pandas_df, schema=energy_results_schema)
        return EnergyResults(df)

    return factory


def call_calculate_grid_loss(
    agg_result_factory: Callable[[], EnergyResults]
) -> EnergyResults:
    spark = SparkSession.builder.getOrCreate()
    df = agg_result_factory()
    grid_loss_responsible_row = grid_loss_responsible_factories.create_row(
        metering_point_type=MeteringPointType.CONSUMPTION,
    )
    grid_loss_responsible = grid_loss_responsible_factories.create(
        spark, [grid_loss_responsible_row]
    )
    return calculate_positive_grid_loss(df, grid_loss_responsible)


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
