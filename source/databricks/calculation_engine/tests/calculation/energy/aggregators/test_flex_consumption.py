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

from datetime import datetime, timedelta
from decimal import Decimal
from typing import Callable

import pandas as pd
import pytest
from pyspark.sql import SparkSession

from package.calculation.energy.aggregators import (
    aggregate_flex_consumption_ga_es,
    aggregate_flex_consumption_ga_brp,
    aggregate_flex_consumption_ga,
)
from package.calculation.energy.energy_results import (
    EnergyResults,
    energy_results_schema,
)
from package.codelists import (
    QuantityQuality,
)
from package.constants import Colname

date_time_formatting_string = "%Y-%m-%dT%H:%M:%S%z"
default_obs_time = datetime.strptime(
    "2020-01-01T00:00:00+0000", date_time_formatting_string
)


@pytest.fixture(scope="module")
def test_data_factory(spark: SparkSession) -> Callable[..., EnergyResults]:
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
            }
        )
        for i in range(3):
            for j in range(5):
                for k in range(10):
                    pandas_df = pandas_df.append(
                        {
                            Colname.grid_area: str(i),
                            Colname.to_grid_area: None,
                            Colname.from_grid_area: None,
                            Colname.balance_responsible_id: str(j),
                            Colname.energy_supplier_id: str(k),
                            Colname.time_window: {
                                Colname.start: default_obs_time + timedelta(hours=i),
                                Colname.end: default_obs_time + timedelta(hours=i + 1),
                            },
                            Colname.sum_quantity: Decimal(i + j + k),
                            Colname.qualities: [QuantityQuality.ESTIMATED.value],
                        },
                        ignore_index=True,
                    )
        df = spark.createDataFrame(pandas_df, schema=energy_results_schema)
        return EnergyResults(df)

    return factory


def test_flex_consumption_calculation_per_ga_and_es(
    test_data_factory: Callable[..., EnergyResults]
) -> None:
    df = test_data_factory()
    result = aggregate_flex_consumption_ga_es(df).df.sort(
        Colname.grid_area, Colname.energy_supplier_id, Colname.time_window
    )
    result_collect = result.collect()
    assert result_collect[0][Colname.balance_responsible_id] is None
    assert result_collect[0][Colname.grid_area] == "0"
    assert result_collect[9][Colname.energy_supplier_id] == "9"
    assert result_collect[10][Colname.sum_quantity] == Decimal("15")
    assert result_collect[29][Colname.grid_area] == "2"
    assert result_collect[29][Colname.energy_supplier_id] == "9"
    assert result_collect[29][Colname.sum_quantity] == Decimal("65")


def test_flex_consumption_calculation_per_ga_and_brp(
    test_data_factory: Callable[..., EnergyResults]
) -> None:
    df = test_data_factory()
    result = aggregate_flex_consumption_ga_brp(df).df.sort(
        Colname.grid_area, Colname.balance_responsible_id, Colname.time_window
    )
    result_collect = result.collect()
    assert result_collect[0][Colname.energy_supplier_id] is None
    assert result_collect[0][Colname.sum_quantity] == Decimal("45")
    assert result_collect[4][Colname.grid_area] == "0"
    assert result_collect[5][Colname.balance_responsible_id] == "0"
    assert result_collect[14][Colname.grid_area] == "2"
    assert result_collect[14][Colname.balance_responsible_id] == "4"
    assert result_collect[14][Colname.sum_quantity] == Decimal("105")


def test_flex_consumption_calculation_per_ga(
    test_data_factory: Callable[..., EnergyResults]
) -> None:
    df = test_data_factory()
    result = aggregate_flex_consumption_ga(df).df.sort(
        Colname.grid_area, Colname.time_window
    )
    result_collect = result.collect()
    assert result_collect[0][Colname.balance_responsible_id] is None
    assert result_collect[0][Colname.energy_supplier_id] is None
    assert result_collect[0][Colname.grid_area] == "0"
    assert result_collect[1][Colname.sum_quantity] == Decimal("375")
    assert result_collect[2][Colname.grid_area] == "2"
    assert result_collect[2][Colname.sum_quantity] == Decimal("425")
