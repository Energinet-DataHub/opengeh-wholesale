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
from pyspark.sql import SparkSession, Row
from pyspark.sql.functions import col

from calculation.energy import grid_loss_responsible_factories
from package.calculation.energy.aggregators.grid_loss_aggregators import (
    calculate_positive_grid_loss,
)
from package.calculation.energy.data_structures.energy_results import (
    EnergyResults,
)
from package.codelists import (
    MeteringPointType,
    QuantityQuality,
)
from package.constants import Colname
from tests.calculation.energy import energy_results_factories


@pytest.fixture(scope="module")
def actual_positive_grid_loss(spark: SparkSession) -> EnergyResults:
    rows = [
        energy_results_factories.create_grid_loss_row(
            grid_area="001",
            quantity=Decimal(-12.567),
        ),
        energy_results_factories.create_grid_loss_row(
            grid_area="002",
            quantity=Decimal(34.32),
        ),
        energy_results_factories.create_grid_loss_row(
            grid_area="003",
            quantity=Decimal(0.0),
        ),
    ]

    grid_loss = energy_results_factories.create(spark, rows)

    responsible_rows = [
        grid_loss_responsible_factories.create_row(
            grid_area="001",
            metering_point_type=MeteringPointType.CONSUMPTION,
        ),
        grid_loss_responsible_factories.create_row(
            grid_area="002",
            metering_point_type=MeteringPointType.CONSUMPTION,
        ),
        grid_loss_responsible_factories.create_row(
            grid_area="003",
            metering_point_type=MeteringPointType.CONSUMPTION,
        ),
    ]
    grid_loss_responsible = grid_loss_responsible_factories.create(
        spark, responsible_rows
    )

    return calculate_positive_grid_loss(grid_loss, grid_loss_responsible)


class TestWhenValidInput:
    def test__has_no_values_below_zero(
        self,
        actual_positive_grid_loss: EnergyResults,
    ) -> None:
        assert (
            actual_positive_grid_loss.df.where(col(Colname.quantity) < 0).count() == 0
        )

    def test__changes_negative_values_to_zero(
        self,
        actual_positive_grid_loss: EnergyResults,
    ) -> None:
        assert actual_positive_grid_loss.df.collect()[0][Colname.quantity] == Decimal(
            "0.00000"
        )

    def test__positive_values_will_not_change(
        self,
        actual_positive_grid_loss: EnergyResults,
    ) -> None:
        assert actual_positive_grid_loss.df.collect()[1][Colname.quantity] == Decimal(
            "34.32000"
        )

    def test__values_that_are_zero_stay_zero(
        self,
        actual_positive_grid_loss: EnergyResults,
    ) -> None:
        assert actual_positive_grid_loss.df.collect()[2][Colname.quantity] == Decimal(
            "0.00000"
        )

    def test__has_expected_values(
        self,
        actual_positive_grid_loss: EnergyResults,
    ) -> None:
        actual_row = actual_positive_grid_loss.df.collect()[1]

        expected = {
            Colname.grid_area: "002",
            Colname.to_grid_area: None,
            Colname.from_grid_area: None,
            Colname.balance_responsible_id: None,
            Colname.energy_supplier_id: grid_loss_responsible_factories.DEFAULT_ENERGY_SUPPLIER_ID,
            Colname.observation_time: energy_results_factories.DEFAULT_OBSERVATION_TIME,
            Colname.quantity: Decimal("34.320000"),
            Colname.qualities: [QuantityQuality.CALCULATED.value],
            Colname.metering_point_id: grid_loss_responsible_factories.DEFAULT_METERING_POINT_ID,
        }
        expected_row = Row(**expected)

        assert actual_row == expected_row
