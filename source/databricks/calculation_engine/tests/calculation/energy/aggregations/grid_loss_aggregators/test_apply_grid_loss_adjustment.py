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
import pytest

from pyspark.sql import SparkSession
from datetime import datetime, timedelta

from package.calculation.energy.aggregators.grid_loss_aggregators import (
    apply_grid_loss_adjustment,
)
from package.codelists import (
    MeteringPointType,
    QuantityQuality,
)
from package.constants import Colname
import tests.calculation.energy.energy_results_factories as energy_results_factories
import tests.calculation.energy.grid_loss_responsible_factories as grid_loss_responsible_factories

# This time should be within the time window of the grid loss responsible
DEFAULT_OBSERVATION_TIME = datetime.strptime(
    "2020-01-01T00:00:00+0000", "%Y-%m-%dT%H:%M:%S%z"
)
DEFAULT_FROM_DATE = datetime.strptime("2020-01-01T00:00:00+0000", "%Y-%m-%dT%H:%M:%S%z")
DEFAULT_TO_DATE = datetime.strptime("2020-01-02T00:00:00+0000", "%Y-%m-%dT%H:%M:%S%z")


class TestWhenValidInput:
    @pytest.mark.parametrize(
        "metering_point_type",
        [
            MeteringPointType.CONSUMPTION,
            MeteringPointType.PRODUCTION,
        ],
    )
    def test_returns_qualities_from_result_and_grid_loss(
        self,
        spark: SparkSession,
        metering_point_type: MeteringPointType,
    ) -> None:
        # Arrange
        expected_qualities = [
            QuantityQuality.CALCULATED.value,
            QuantityQuality.ESTIMATED.value,
        ]

        result_row = energy_results_factories.create_row(
            observation_time=DEFAULT_OBSERVATION_TIME,
            energy_supplier_id="energy_supplier_id",
            qualities=[QuantityQuality.CALCULATED],
        )
        result = energy_results_factories.create(spark, [result_row])

        grid_loss_row = energy_results_factories.create_row(
            observation_time=DEFAULT_OBSERVATION_TIME,
            qualities=[QuantityQuality.ESTIMATED],
        )
        grid_loss = energy_results_factories.create(spark, [grid_loss_row])

        grid_loss_responsible_row = grid_loss_responsible_factories.create_row(
            energy_supplier_id="energy_supplier_id",
            metering_point_type=metering_point_type,
        )
        grid_loss_responsible = grid_loss_responsible_factories.create(
            spark, [grid_loss_responsible_row]
        )

        # Act
        actual = apply_grid_loss_adjustment(
            result,
            grid_loss,
            grid_loss_responsible,
            metering_point_type,
        )

        # Assert
        actual_row = actual.df.collect()[0]
        actual_qualities = actual_row[Colname.qualities]
        assert set(actual_qualities) == set(expected_qualities)

    @pytest.mark.parametrize(
        "metering_point_type",
        [
            MeteringPointType.CONSUMPTION,
            MeteringPointType.PRODUCTION,
        ],
    )
    def test_returns_sum_quantity_from_result_and_grid_loss(
        self,
        spark: SparkSession,
        metering_point_type: MeteringPointType,
    ) -> None:
        # Arrange
        result_row = energy_results_factories.create_row(
            observation_time=DEFAULT_OBSERVATION_TIME,
            energy_supplier_id="energy_supplier_id",
            sum_quantity=20,
        )
        result = energy_results_factories.create(spark, [result_row])

        grid_loss_row = energy_results_factories.create_row(
            observation_time=DEFAULT_OBSERVATION_TIME,
            sum_quantity=10,
        )
        grid_loss = energy_results_factories.create(spark, [grid_loss_row])

        grid_loss_responsible_row = grid_loss_responsible_factories.create_row(
            energy_supplier_id="energy_supplier_id",
            metering_point_type=metering_point_type,
        )
        grid_loss_responsible = grid_loss_responsible_factories.create(
            spark, [grid_loss_responsible_row]
        )

        # Act
        actual = apply_grid_loss_adjustment(
            result,
            grid_loss,
            grid_loss_responsible,
            metering_point_type,
        )

        # Assert
        actual_row = actual.df.collect()[0]
        actual_sum_quantity = actual_row[Colname.sum_quantity]
        assert actual_sum_quantity == 30


class TestWhenEnergySupplierIdIsNotGridLossResponsible:
    @pytest.mark.parametrize(
        "metering_point_type",
        [
            MeteringPointType.CONSUMPTION,
            MeteringPointType.PRODUCTION,
        ],
    )
    def test_returns_result_sum_quantity_equal_to_correct_adjusted_grid_loss(
        self,
        spark: SparkSession,
        metering_point_type: MeteringPointType,
    ) -> None:
        # Arrange
        result_rows = [
            energy_results_factories.create_row(
                grid_area="1",
                energy_supplier_id="not_grid_loss_responsible",
                observation_time=DEFAULT_OBSERVATION_TIME,
                sum_quantity=10,
            )
        ]
        result = energy_results_factories.create(spark, result_rows)

        grid_loss_rows = [
            energy_results_factories.create_row(
                grid_area="1",
                from_grid_area=None,
                to_grid_area=None,
                balance_responsible_id=None,
                energy_supplier_id=None,
                observation_time=DEFAULT_OBSERVATION_TIME,
                sum_quantity=20,
                qualities=[QuantityQuality.MEASURED],
            )
        ]
        grid_loss = energy_results_factories.create(spark, grid_loss_rows)

        grid_loss_responsible_rows = [
            grid_loss_responsible_factories.create_row(
                grid_area="1",
                metering_point_type=metering_point_type,
                energy_supplier_id="grid_loss_responsible_1",
                from_date=DEFAULT_FROM_DATE,
                to_date=DEFAULT_TO_DATE,
            )
        ]
        grid_loss_responsible = grid_loss_responsible_factories.create(
            spark, grid_loss_responsible_rows
        )

        # Act
        actual = apply_grid_loss_adjustment(
            result,
            grid_loss,
            grid_loss_responsible,
            metering_point_type,
        )

        # Assert
        assert actual.df.count() == 2
        assert actual.df.collect()[0][Colname.sum_quantity] == 20
        assert actual.df.collect()[1][Colname.sum_quantity] == 10


class TestWhenGridLossResponsibleIsChangedWithinPeriod:
    @pytest.mark.parametrize(
        "metering_point_type",
        [
            MeteringPointType.CONSUMPTION,
            MeteringPointType.PRODUCTION,
        ],
    )
    def test_returns_correct_energy_supplier_within_grid_loss_responsible_period(
        self,
        spark: SparkSession,
        metering_point_type: MeteringPointType,
    ) -> None:
        # Arrange
        from_date_1 = DEFAULT_FROM_DATE
        to_date_1 = DEFAULT_TO_DATE
        from_date_2 = from_date_1 + timedelta(days=1)
        to_date_2 = to_date_1 + timedelta(days=1)

        result_rows = [
            energy_results_factories.create_row(
                grid_area="1",
                energy_supplier_id="grid_loss_responsible_2",
                observation_time=DEFAULT_OBSERVATION_TIME,
                sum_quantity=10,
            )
        ]
        result = energy_results_factories.create(spark, result_rows)

        grid_loss_rows = [
            energy_results_factories.create_row(
                grid_area="1",
                from_grid_area=None,
                to_grid_area=None,
                balance_responsible_id=None,
                energy_supplier_id=None,
                observation_time=DEFAULT_OBSERVATION_TIME,
                sum_quantity=20,
                qualities=[QuantityQuality.MEASURED],
            ),
            energy_results_factories.create_row(
                grid_area="1",
                from_grid_area=None,
                to_grid_area=None,
                balance_responsible_id=None,
                energy_supplier_id=None,
                observation_time=from_date_2,
                sum_quantity=30,
                qualities=[QuantityQuality.MEASURED],
            ),
        ]
        grid_loss = energy_results_factories.create(spark, grid_loss_rows)

        grid_loss_responsible_rows = [
            grid_loss_responsible_factories.create_row(
                grid_area="1",
                metering_point_type=metering_point_type,
                energy_supplier_id="grid_loss_responsible_1",
                from_date=from_date_1,
                to_date=to_date_1,
            ),
            grid_loss_responsible_factories.create_row(
                grid_area="1",
                metering_point_type=metering_point_type,
                energy_supplier_id="grid_loss_responsible_2",
                from_date=from_date_2,
                to_date=to_date_2,
            ),
        ]
        grid_loss_responsible = grid_loss_responsible_factories.create(
            spark, grid_loss_responsible_rows
        )

        # Act
        actual = apply_grid_loss_adjustment(
            result,
            grid_loss,
            grid_loss_responsible,
            metering_point_type,
        )

        # Assert
        assert actual.df.count() == 3
        assert actual.df.collect()[0][Colname.sum_quantity] == 20
        assert (
            actual.df.collect()[0][Colname.energy_supplier_id]
            == "grid_loss_responsible_1"
        )
        assert actual.df.collect()[1][Colname.sum_quantity] == 30
        assert (
            actual.df.collect()[1][Colname.energy_supplier_id]
            == "grid_loss_responsible_2"
        )
        assert actual.df.collect()[2][Colname.sum_quantity] == 10
        assert (
            actual.df.collect()[2][Colname.energy_supplier_id]
            == "grid_loss_responsible_2"
        )
