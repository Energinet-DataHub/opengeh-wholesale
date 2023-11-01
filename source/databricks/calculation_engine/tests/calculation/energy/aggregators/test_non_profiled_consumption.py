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

from package.calculation.energy.aggregators import (
    aggregate_non_profiled_consumption_ga_es,
    aggregate_non_profiled_consumption_ga_brp,
    aggregate_non_profiled_consumption_ga,
)
from package.calculation.energy.energy_results import (
    EnergyResults,
    energy_results_schema,
)
from package.codelists import MeteringPointType, QuantityQuality, SettlementMethod
from package.constants import Colname

date_time_formatting_string = "%Y-%m-%dT%H:%M:%S%z"
default_obs_time = datetime.strptime(
    "2020-01-01T00:00:00+0000", date_time_formatting_string
)


@pytest.fixture(scope="module")
def agg_result_factory(spark: SparkSession) -> Callable[..., EnergyResults]:
    def factory() -> EnergyResults:
        pandas_df = pd.DataFrame(
            {
                Colname.grid_area: ["1", "1", "1", "1", "1", "2"],
                Colname.to_grid_area: ["1", "1", "1", "1", "1", "2"],
                Colname.from_grid_area: ["1", "1", "1", "1", "1", "2"],
                Colname.balance_responsible_id: ["1", "2", "1", "2", "1", "1"],
                Colname.energy_supplier_id: ["1", "2", "3", "4", "5", "6"],
                Colname.time_window: [
                    {
                        Colname.start: datetime(2020, 1, 1, 0, 0),
                        Colname.end: datetime(2020, 1, 1, 1, 0),
                    },
                    {
                        Colname.start: datetime(2020, 1, 1, 0, 0),
                        Colname.end: datetime(2020, 1, 1, 1, 0),
                    },
                    {
                        Colname.start: datetime(2020, 1, 1, 0, 0),
                        Colname.end: datetime(2020, 1, 1, 1, 0),
                    },
                    {
                        Colname.start: datetime(2020, 1, 1, 0, 0),
                        Colname.end: datetime(2020, 1, 1, 1, 0),
                    },
                    {
                        Colname.start: datetime(2020, 1, 1, 1, 0),
                        Colname.end: datetime(2020, 1, 1, 2, 0),
                    },
                    {
                        Colname.start: datetime(2020, 1, 1, 0, 0),
                        Colname.end: datetime(2020, 1, 1, 1, 0),
                    },
                ],
                Colname.sum_quantity: [
                    Decimal(1.0),
                    Decimal(1.0),
                    Decimal(1.0),
                    Decimal(1.0),
                    Decimal(1.0),
                    Decimal(1.0),
                ],
                Colname.qualities: [
                    [QuantityQuality.ESTIMATED.value],
                    [QuantityQuality.ESTIMATED.value],
                    [QuantityQuality.ESTIMATED.value],
                    [QuantityQuality.ESTIMATED.value],
                    [QuantityQuality.ESTIMATED.value],
                    [QuantityQuality.ESTIMATED.value],
                ],
                Colname.metering_point_type: [
                    MeteringPointType.CONSUMPTION.value,
                    MeteringPointType.CONSUMPTION.value,
                    MeteringPointType.CONSUMPTION.value,
                    MeteringPointType.CONSUMPTION.value,
                    MeteringPointType.CONSUMPTION.value,
                    MeteringPointType.CONSUMPTION.value,
                ],
                Colname.settlement_method: [
                    SettlementMethod.NON_PROFILED.value,
                    SettlementMethod.NON_PROFILED.value,
                    SettlementMethod.NON_PROFILED.value,
                    SettlementMethod.NON_PROFILED.value,
                    SettlementMethod.NON_PROFILED.value,
                    SettlementMethod.NON_PROFILED.value,
                ],
            }
        )

        df = spark.createDataFrame(pandas_df, schema=energy_results_schema)
        return EnergyResults(df)

    return factory


def test_non_profiled_consumption_summarizes_correctly_on_grid_area_within_same_time_window(
    agg_result_factory: Callable[..., EnergyResults],
) -> None:
    # Arrange
    consumption = agg_result_factory()

    # Act
    aggregated_df = aggregate_non_profiled_consumption_ga(consumption)

    # Assert
    aggregated_df_collect = aggregated_df.df.sort(
        Colname.grid_area, Colname.time_window
    ).collect()
    assert (
        aggregated_df_collect[0][Colname.sum_quantity] == Decimal("4.0")
        and aggregated_df_collect[0][Colname.grid_area] == "1"
        and aggregated_df_collect[0][Colname.time_window]["start"]
        == datetime(2020, 1, 1, 0, 0)
        and aggregated_df_collect[0][Colname.time_window]["end"]
        == datetime(2020, 1, 1, 1, 0)
    )


def test_non_profiled_consumption_summarizes_correctly_on_grid_area_with_different_time_window(
    agg_result_factory: Callable[..., EnergyResults],
) -> None:
    # Arrange
    consumption = agg_result_factory()

    # Act
    aggregated_df = aggregate_non_profiled_consumption_ga(consumption)

    # Assert
    aggregated_df_collect = aggregated_df.df.sort(
        Colname.grid_area, Colname.time_window
    ).collect()
    assert (
        aggregated_df_collect[1][Colname.sum_quantity] == Decimal("1.0")
        and aggregated_df_collect[1][Colname.grid_area] == "1"
        and aggregated_df_collect[1][Colname.time_window]["start"]
        == datetime(2020, 1, 1, 1, 0)
        and aggregated_df_collect[1][Colname.time_window]["end"]
        == datetime(2020, 1, 1, 2, 0)
    )


def test_non_profiled_consumption_summarizes_correctly_on_grid_area_with_same_time_window_as_other_grid_area(
    agg_result_factory: Callable[..., EnergyResults],
) -> None:
    # Arrange
    consumption = agg_result_factory()

    # Act
    aggregated_df = aggregate_non_profiled_consumption_ga(consumption)

    # Assert
    aggregated_df_collect = aggregated_df.df.sort(
        Colname.grid_area, Colname.time_window
    ).collect()
    assert (
        aggregated_df_collect[2][Colname.sum_quantity] == Decimal("1.0")
        and aggregated_df_collect[2][Colname.grid_area] == "2"
        and aggregated_df_collect[2][Colname.time_window]["start"]
        == datetime(2020, 1, 1, 0, 0)
        and aggregated_df_collect[2][Colname.time_window]["end"]
        == datetime(2020, 1, 1, 1, 0)
    )


def test_non_profiled_consumption_calculation_per_ga_and_es(
    agg_result_factory: Callable[..., EnergyResults]
) -> None:
    # Arrange
    consumption = agg_result_factory()

    # Act
    aggregated_df = aggregate_non_profiled_consumption_ga_es(consumption)

    # Assert
    aggregated_df_collect = aggregated_df.df.sort(
        Colname.grid_area, Colname.energy_supplier_id, Colname.time_window
    ).collect()
    assert aggregated_df_collect[0][Colname.balance_responsible_id] is None
    assert aggregated_df_collect[0][Colname.grid_area] == "1"
    assert aggregated_df_collect[0][Colname.energy_supplier_id] == "1"
    assert aggregated_df_collect[0][Colname.sum_quantity] == Decimal(1)
    assert aggregated_df_collect[1][Colname.sum_quantity] == Decimal(1)
    assert aggregated_df_collect[2][Colname.sum_quantity] == Decimal(1)
    assert aggregated_df_collect[3][Colname.sum_quantity] == Decimal(1)
    assert aggregated_df_collect[4][Colname.sum_quantity] == Decimal(1)
    assert aggregated_df_collect[5][Colname.sum_quantity] == Decimal(1)


def test_non_profiled_consumption_calculation_per_ga_and_brp(
    agg_result_factory: Callable[..., EnergyResults],
) -> None:
    # Arrange
    non_profiled_consumption = agg_result_factory()

    # Act
    aggregated_df = aggregate_non_profiled_consumption_ga_brp(non_profiled_consumption)

    # Assert
    aggregated_df_collect = aggregated_df.df.sort(
        Colname.grid_area, Colname.balance_responsible_id, Colname.time_window
    ).collect()
    assert aggregated_df_collect[0][Colname.energy_supplier_id] is None
    assert aggregated_df_collect[0][Colname.grid_area] == "1"
    assert aggregated_df_collect[0][Colname.balance_responsible_id] == "1"
    assert aggregated_df_collect[0][Colname.sum_quantity] == Decimal(2)
    assert aggregated_df_collect[1][Colname.sum_quantity] == Decimal(1)
    assert aggregated_df_collect[2][Colname.sum_quantity] == Decimal(2)
    assert aggregated_df_collect[3][Colname.sum_quantity] == Decimal(1)


def test_non_profiled_consumption_calculation_per_ga(
    agg_result_factory: Callable[..., EnergyResults]
) -> None:
    # Arrange
    consumption = agg_result_factory()

    # Act
    aggregated_df = aggregate_non_profiled_consumption_ga(consumption)

    # Assert
    aggregated_df_collect = aggregated_df.df.sort(
        Colname.grid_area, Colname.time_window
    ).collect()
    assert aggregated_df_collect[0][Colname.balance_responsible_id] is None
    assert aggregated_df_collect[0][Colname.energy_supplier_id] is None
    assert aggregated_df_collect[0][Colname.grid_area] == "1"
    assert aggregated_df_collect[0][Colname.sum_quantity] == Decimal(4)
    assert aggregated_df_collect[1][Colname.sum_quantity] == Decimal(1)
    assert aggregated_df_collect[2][Colname.sum_quantity] == Decimal(1)
