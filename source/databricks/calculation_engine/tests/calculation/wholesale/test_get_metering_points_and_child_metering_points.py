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

from pyspark.sql import SparkSession
import pytest
import tests.calculation.preparation.transformations.metering_point_periods_factory as factory
from package.calculation.wholesale.get_metering_points_and_child_metering_points import (
    get_metering_points_and_child_metering_points,
)

from package.codelists import MeteringPointType
from package.constants import Colname


class TestWhenMeteringPointPeriodsHasMeteringPointTypesThatIsNotExchange:
    @pytest.mark.parametrize(
        "metering_point_type",
        [t for t in MeteringPointType if t != MeteringPointType.EXCHANGE],
    )
    def test__then_return_metering_points(
        self,
        metering_point_type: MeteringPointType,
        spark: SparkSession,
    ):
        # Arrange
        row = factory.create_row(
            metering_point_type=metering_point_type,
        )
        metering_point_periods = factory.create(spark, row)

        # Act
        actual = get_metering_points_and_child_metering_points(
            metering_point_periods,
        )

        # Assert
        assert actual.count() == 1


class TestWhenMeteringPointPeriodsHasMeteringPointTypesThatIsExchange:
    def test__then_return_result_without_the_metering_point(
        self,
        spark: SparkSession,
    ):
        # Arrange
        row = factory.create_row(
            metering_point_type=MeteringPointType.EXCHANGE,
        )
        metering_point_periods = factory.create(spark, row)

        # Act
        actual = get_metering_points_and_child_metering_points(
            metering_point_periods,
        )

        # Assert
        assert actual.count() == 0


class TestWhenParentMeteringPointChangesEnergySupplierWithinChildMeteringPointPeriod:
    def test__1(
        self,
        spark: SparkSession,
    ):
        # Arrange
        rows = [
            factory.create_row(
                metering_point_id="parent_metering_point_id",
                metering_point_type=MeteringPointType.CONSUMPTION,
                energy_supplier_id="es_parent_1",
                from_date=datetime(2019, 12, 31, 23),
                to_date=datetime(2020, 1, 15, 23),
            ),
            factory.create_row(
                metering_point_id="parent_metering_point_id",
                metering_point_type=MeteringPointType.CONSUMPTION,
                energy_supplier_id="es_parent_2",
                from_date=datetime(2020, 1, 15, 23),
                to_date=datetime(2020, 1, 31, 23),
            ),
            factory.create_row(
                parent_metering_point_id="parent_metering_point_id",
                metering_point_type=MeteringPointType.NET_CONSUMPTION,
                energy_supplier_id=None,
                from_date=datetime(2019, 12, 31, 23),
                to_date=datetime(2020, 1, 31, 23),
                settlement_method=None,
            ),
        ]
        metering_point_periods = factory.create(spark, rows)

        # Act
        actual = get_metering_points_and_child_metering_points(
            metering_point_periods,
        )

        # Assert
        assert actual.count() == 4
        actual_only_child_metering_points = (
            actual.filter(
                actual[Colname.metering_point_type]
                == MeteringPointType.NET_CONSUMPTION.value
            )
            .sort(Colname.from_date)
            .collect()
        )

        assert (
            actual_only_child_metering_points[0][Colname.energy_supplier_id]
            == "es_parent_1"
        )
        assert (
            actual_only_child_metering_points[1][Colname.energy_supplier_id]
            == "es_parent_2"
        )
        assert actual_only_child_metering_points[0][Colname.from_date] == datetime(
            2019, 12, 31, 23
        )
        assert actual_only_child_metering_points[1][Colname.from_date] == datetime(
            2020, 1, 15, 23
        )
        assert actual_only_child_metering_points[0][Colname.to_date] == datetime(
            2020, 1, 15, 23
        )
        assert actual_only_child_metering_points[1][Colname.to_date] == datetime(
            2020, 1, 31, 23
        )

    def test__2(
        self,
        spark: SparkSession,
    ):
        """
        When parent mp changes energy supplier within child mp period,
        but parent from date is before child from date and parent to date is after child to date,
        then returns two child and two parent metering points
        """
        # Arrange
        rows = [
            factory.create_row(
                metering_point_id="parent_metering_point_id",
                metering_point_type=MeteringPointType.CONSUMPTION,
                energy_supplier_id="es_parent_1",
                from_date=datetime(2019, 12, 15, 23),
                to_date=datetime(2020, 1, 15, 23),
            ),
            factory.create_row(
                metering_point_id="parent_metering_point_id",
                metering_point_type=MeteringPointType.CONSUMPTION,
                energy_supplier_id="es_parent_2",
                from_date=datetime(2020, 1, 15, 23),
                to_date=datetime(2020, 2, 15, 23),
            ),
            factory.create_row(
                parent_metering_point_id="parent_metering_point_id",
                metering_point_type=MeteringPointType.NET_CONSUMPTION,
                energy_supplier_id=None,
                from_date=datetime(2019, 12, 31, 23),
                to_date=datetime(2020, 1, 31, 23),
                settlement_method=None,
            ),
        ]
        metering_point_periods = factory.create(spark, rows)

        # Act
        actual = get_metering_points_and_child_metering_points(
            metering_point_periods,
        )

        # Assert
        assert actual.count() == 4
        actual_only_child_metering_points = (
            actual.filter(
                actual[Colname.metering_point_type]
                == MeteringPointType.NET_CONSUMPTION.value
            )
            .sort(Colname.from_date)
            .collect()
        )

        assert (
            actual_only_child_metering_points[0][Colname.energy_supplier_id]
            == "es_parent_1"
        )
        assert (
            actual_only_child_metering_points[1][Colname.energy_supplier_id]
            == "es_parent_2"
        )
        assert actual_only_child_metering_points[0][Colname.from_date] == datetime(
            2019, 12, 31, 23
        )
        assert actual_only_child_metering_points[1][Colname.from_date] == datetime(
            2020, 1, 15, 23
        )
        assert actual_only_child_metering_points[0][Colname.to_date] == datetime(
            2020, 1, 15, 23
        )
        assert actual_only_child_metering_points[1][Colname.to_date] == datetime(
            2020, 1, 31, 23
        )

    def test__3(
        self,
        spark: SparkSession,
    ):
        """
        When parent changes energy supplier within child period,
        but parent from date is before child from data and parent to date is after child to date,
        then returns two child and two parent metering points
        """
        # Arrange
        rows = [
            factory.create_row(
                metering_point_id="parent_metering_point_id",
                metering_point_type=MeteringPointType.CONSUMPTION,
                energy_supplier_id="es_parent_1",
                from_date=datetime(2020, 1, 5, 23),
                to_date=datetime(2020, 1, 15, 23),
            ),
            factory.create_row(
                metering_point_id="parent_metering_point_id",
                metering_point_type=MeteringPointType.CONSUMPTION,
                energy_supplier_id="es_parent_2",
                from_date=datetime(2020, 1, 15, 23),
                to_date=datetime(2020, 1, 25, 23),
            ),
            factory.create_row(
                parent_metering_point_id="parent_metering_point_id",
                metering_point_type=MeteringPointType.NET_CONSUMPTION,
                energy_supplier_id=None,
                from_date=datetime(2019, 12, 31, 23),
                to_date=datetime(2020, 1, 31, 23),
                settlement_method=None,
            ),
        ]
        metering_point_periods = factory.create(spark, rows)

        # Act
        actual = get_metering_points_and_child_metering_points(
            metering_point_periods,
        )

        # Assert
        assert actual.count() == 4
        actual_only_child_metering_points = (
            actual.filter(
                actual[Colname.metering_point_type]
                == MeteringPointType.NET_CONSUMPTION.value
            )
            .sort(Colname.from_date)
            .collect()
        )

        assert (
            actual_only_child_metering_points[0][Colname.energy_supplier_id]
            == "es_parent_1"
        )
        assert (
            actual_only_child_metering_points[1][Colname.energy_supplier_id]
            == "es_parent_2"
        )
        assert actual_only_child_metering_points[0][Colname.from_date] == datetime(
            2020, 1, 5, 23
        )
        assert actual_only_child_metering_points[1][Colname.from_date] == datetime(
            2020, 1, 15, 23
        )
        assert actual_only_child_metering_points[0][Colname.to_date] == datetime(
            2020, 1, 15, 23
        )
        assert actual_only_child_metering_points[1][Colname.to_date] == datetime(
            2020, 1, 25, 23
        )

    def test__4(
        self,
        spark: SparkSession,
    ):
        # Arrange
        rows = [
            factory.create_row(
                metering_point_id="parent_metering_point_id",
                metering_point_type=MeteringPointType.CONSUMPTION,
                energy_supplier_id="es_parent_1",
                from_date=datetime(2019, 12, 31, 23),
                to_date=datetime(2020, 1, 15, 23),
            ),
            factory.create_row(
                metering_point_id="parent_metering_point_id",
                metering_point_type=MeteringPointType.CONSUMPTION,
                energy_supplier_id="es_parent_2",
                from_date=datetime(2020, 1, 15, 23),
                to_date=datetime(2020, 1, 31, 23),
            ),
            factory.create_row(
                metering_point_id="parent_metering_point_id",
                metering_point_type=MeteringPointType.CONSUMPTION,
                energy_supplier_id="es_parent_3",
                from_date=datetime(2020, 1, 31, 23),
                to_date=datetime(2020, 2, 15, 23),
            ),
            factory.create_row(
                metering_point_id="parent_metering_point_id",
                metering_point_type=MeteringPointType.CONSUMPTION,
                energy_supplier_id="es_parent_4",
                from_date=datetime(2019, 12, 15, 23),
                to_date=datetime(2019, 12, 31, 23),
            ),
            factory.create_row(
                parent_metering_point_id="parent_metering_point_id",
                metering_point_type=MeteringPointType.NET_CONSUMPTION,
                energy_supplier_id=None,
                from_date=datetime(2019, 12, 31, 23),
                to_date=datetime(2020, 1, 31, 23),
                settlement_method=None,
            ),
        ]
        metering_point_periods = factory.create(spark, rows)

        # Act
        actual = get_metering_points_and_child_metering_points(
            metering_point_periods,
        )

        # Assert
        assert actual.count() == 6
        actual_only_child_metering_points = (
            actual.filter(
                actual[Colname.metering_point_type]
                == MeteringPointType.NET_CONSUMPTION.value
            )
            .sort(Colname.from_date)
            .collect()
        )

        assert (
            actual_only_child_metering_points[0][Colname.energy_supplier_id]
            == "es_parent_1"
        )
        assert (
            actual_only_child_metering_points[1][Colname.energy_supplier_id]
            == "es_parent_2"
        )

        assert actual_only_child_metering_points[0][Colname.from_date] == datetime(
            2019, 12, 31, 23
        )
        assert actual_only_child_metering_points[1][Colname.from_date] == datetime(
            2020, 1, 15, 23
        )

        assert actual_only_child_metering_points[0][Colname.to_date] == datetime(
            2020, 1, 15, 23
        )
        assert actual_only_child_metering_points[1][Colname.to_date] == datetime(
            2020, 1, 31, 23
        )
