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
from pyspark.sql.functions import col

from calculation.energy import grid_loss_responsible_factories
from package.calculation.energy.aggregators.grid_loss_aggregators import (
    calculate_positive_grid_loss,
)
from package.calculation.energy.energy_results import (
    EnergyResults,
)
from package.codelists import (
    MeteringPointType,
)
from package.constants import Colname
from tests.calculation.energy import energy_results_factories


@pytest.fixture(scope="module")
def actual_positive_grid_loss(spark: SparkSession) -> EnergyResults:
    rows = [
        energy_results_factories.create_row(
            grid_area="001",
            sum_quantity=Decimal(-12.567),
        ),
        energy_results_factories.create_row(
            grid_area="002",
            sum_quantity=Decimal(34.32),
        ),
        energy_results_factories.create_row(
            grid_area="003",
            sum_quantity=Decimal(0.0),
        ),
    ]

    df = energy_results_factories.create(spark, rows)

    grid_loss_responsible_row = grid_loss_responsible_factories.create_row(
        metering_point_type=MeteringPointType.CONSUMPTION,
    )
    grid_loss_responsible = grid_loss_responsible_factories.create(
        spark, [grid_loss_responsible_row]
    )

    return calculate_positive_grid_loss(df, grid_loss_responsible)


def test_grid_area_grid_loss_has_no_values_below_zero(
    actual_positive_grid_loss: EnergyResults,
) -> None:
    assert (
        actual_positive_grid_loss.df.where(col(Colname.sum_quantity) < 0).count() == 0
    )


def test_grid_area_grid_loss_changes_negative_values_to_zero(
    actual_positive_grid_loss: EnergyResults,
) -> None:
    assert actual_positive_grid_loss.df.collect()[0][Colname.sum_quantity] == Decimal(
        "0.00000"
    )


def test_grid_area_grid_loss_positive_values_will_not_change(
    actual_positive_grid_loss: EnergyResults,
) -> None:
    assert actual_positive_grid_loss.df.collect()[1][Colname.sum_quantity] == Decimal(
        "34.32000"
    )


def test_grid_area_grid_loss_values_that_are_zero_stay_zero(
    actual_positive_grid_loss: EnergyResults,
) -> None:
    assert actual_positive_grid_loss.df.collect()[2][Colname.sum_quantity] == Decimal(
        "0.00000"
    )
