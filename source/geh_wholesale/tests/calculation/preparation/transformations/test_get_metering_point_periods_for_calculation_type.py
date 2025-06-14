from datetime import datetime

import pytest
from pyspark.sql import SparkSession

import tests.calculation.preparation.transformations.metering_point_periods_factory as factory
from geh_wholesale.calculation.preparation.transformations.metering_point_periods_for_calculation_type import (
    _get_child_metering_points_with_energy_suppliers,
)
from geh_wholesale.codelists import MeteringPointType
from geh_wholesale.constants import Colname


class TestWhenMeteringPointPeriodsHasMeteringPointType:
    @pytest.mark.parametrize(
        "metering_point_type",
        [
            t
            for t in MeteringPointType
            if t != MeteringPointType.EXCHANGE
            and t != MeteringPointType.PRODUCTION
            and t != MeteringPointType.CONSUMPTION
        ],
    )
    def test__returns_child_metering_points(
        self,
        metering_point_type: MeteringPointType,
        spark: SparkSession,
    ) -> None:
        # Arrange
        rows = [
            factory.create_row(
                metering_point_type=metering_point_type,
                parent_metering_point_id="parent_metering_point_id",
            ),
            factory.create_row(metering_point_id="parent_metering_point_id"),
        ]
        metering_point_periods = factory.create(spark, rows)

        # Act
        actual = _get_child_metering_points_with_energy_suppliers(
            metering_point_periods,
        )

        # Assert
        assert actual.count() == 1


class TestWhenParentMeteringPointChangesEnergySupplierWithinChildMeteringPointPeriod:
    def test__returns_two_child_metering_points_with_the_same_period_as_the_parent_metering_points(
        self,
        spark: SparkSession,
    ):
        """
        input metering points:
        parent:  |-----------------|
        parent:                    |-----------------|
        child:   |-----------------------------------|
        output metering points (only child):
        child:   |-----------------|
        child:                     |-----------------|
        """
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
        actual = _get_child_metering_points_with_energy_suppliers(
            metering_point_periods,
        )

        # Assert
        assert actual.count() == 2
        actual_metering_points_sorted = actual.sort(Colname.from_date).collect()

        assert actual_metering_points_sorted[0][Colname.energy_supplier_id] == "es_parent_1"
        assert actual_metering_points_sorted[1][Colname.energy_supplier_id] == "es_parent_2"
        assert actual_metering_points_sorted[0][Colname.from_date] == datetime(2019, 12, 31, 23)
        assert actual_metering_points_sorted[1][Colname.from_date] == datetime(2020, 1, 15, 23)
        assert actual_metering_points_sorted[0][Colname.to_date] == datetime(2020, 1, 15, 23)
        assert actual_metering_points_sorted[1][Colname.to_date] == datetime(2020, 1, 31, 23)

    def test__returns_two_child_metering_points_with_the_same_from_and_to_date(
        self,
        spark: SparkSession,
    ):
        """
        input metering points:
        parent:  |-----------------|
        parent:                    |-----------------|
        child:       |--------------------------|
        output metering points (only child):
        child:       |-------------|
        child:                     |------------|
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
        actual = _get_child_metering_points_with_energy_suppliers(
            metering_point_periods,
        )

        # Assert
        assert actual.count() == 2
        actual_metering_points_sorted = actual.sort(Colname.from_date).collect()

        assert actual_metering_points_sorted[0][Colname.energy_supplier_id] == "es_parent_1"
        assert actual_metering_points_sorted[1][Colname.energy_supplier_id] == "es_parent_2"
        assert actual_metering_points_sorted[0][Colname.from_date] == datetime(2019, 12, 31, 23)
        assert actual_metering_points_sorted[1][Colname.from_date] == datetime(2020, 1, 15, 23)
        assert actual_metering_points_sorted[0][Colname.to_date] == datetime(2020, 1, 15, 23)
        assert actual_metering_points_sorted[1][Colname.to_date] == datetime(2020, 1, 31, 23)

    def test__returns_two_child_metering_points_with_parent_metering_points_from_and_to_date(
        self,
        spark: SparkSession,
    ):
        """
        input metering points:
        parent:       |------------|
        parent:                    |------------|
        child:   |-----------------------------------|
        output metering points (only child):
        child:        |------------|
        child:                     |------------|
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
        actual = _get_child_metering_points_with_energy_suppliers(
            metering_point_periods,
        )

        # Assert
        assert actual.count() == 2
        actual_metering_points_sorted = actual.sort(Colname.from_date).collect()

        assert actual_metering_points_sorted[0][Colname.energy_supplier_id] == "es_parent_1"
        assert actual_metering_points_sorted[1][Colname.energy_supplier_id] == "es_parent_2"
        assert actual_metering_points_sorted[0][Colname.from_date] == datetime(2020, 1, 5, 23)
        assert actual_metering_points_sorted[1][Colname.from_date] == datetime(2020, 1, 15, 23)
        assert actual_metering_points_sorted[0][Colname.to_date] == datetime(2020, 1, 15, 23)
        assert actual_metering_points_sorted[1][Colname.to_date] == datetime(2020, 1, 25, 23)

    def test__returns_two_child_metering_points_with_the_same_period_as_parent_metering_points_in_child_period(
        self,
        spark: SparkSession,
    ):
        """
        input metering points:
        parent:           |--------|
        parent:                    |--------|
        parent:  |--------|
        parent:                             |--------|
        child:            |-----------------|
        output metering points (only child):
        child:            |--------|
        child:                     |--------|
        """
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
        actual = _get_child_metering_points_with_energy_suppliers(
            metering_point_periods,
        )

        # Assert
        assert actual.count() == 2
        actual_metering_points_sorted = actual.sort(Colname.from_date).collect()

        assert actual_metering_points_sorted[0][Colname.energy_supplier_id] == "es_parent_1"
        assert actual_metering_points_sorted[1][Colname.energy_supplier_id] == "es_parent_2"

        assert actual_metering_points_sorted[0][Colname.from_date] == datetime(2019, 12, 31, 23)
        assert actual_metering_points_sorted[1][Colname.from_date] == datetime(2020, 1, 15, 23)

        assert actual_metering_points_sorted[0][Colname.to_date] == datetime(2020, 1, 15, 23)
        assert actual_metering_points_sorted[1][Colname.to_date] == datetime(2020, 1, 31, 23)


class TestWhenNoMeteringPointIdMatchingParentMeteringPointId:
    def test__returns_no_child_metering_points(
        self,
        spark: SparkSession,
    ):
        # Arrange
        rows = [
            factory.create_row(
                metering_point_type=MeteringPointType.CONSUMPTION_FROM_GRID,
                parent_metering_point_id="parent_metering_point_id",
            )
        ]
        metering_point_periods = factory.create(spark, rows)

        # Act
        actual = _get_child_metering_points_with_energy_suppliers(
            metering_point_periods,
        )

        # Assert
        assert actual.count() == 0
