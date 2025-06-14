from datetime import datetime, timedelta

import pytest
from pyspark.sql import SparkSession

import tests.calculation.energy.energy_results_factories as energy_results_factories
from geh_wholesale.calculation.energy.aggregators.grid_loss_aggregators import (
    apply_grid_loss_adjustment,
)
from geh_wholesale.codelists import (
    MeteringPointType,
    QuantityQuality,
)
from geh_wholesale.constants import Colname
from tests.calculation.energy import grid_loss_metering_point_periods_factories

# This time should be within the time window of the grid loss responsible
DEFAULT_OBSERVATION_TIME = datetime.strptime("2020-01-01T00:00:00+0000", "%Y-%m-%dT%H:%M:%S%z")
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
            balance_responsible_id="balance_responsible_id",
            qualities=[QuantityQuality.CALCULATED],
        )
        result = energy_results_factories.create(spark, [result_row])

        grid_loss_row = energy_results_factories.create_row(
            observation_time=DEFAULT_OBSERVATION_TIME,
            qualities=[QuantityQuality.ESTIMATED],
        )
        grid_loss = energy_results_factories.create(spark, [grid_loss_row])

        grid_loss_metering_point_periods = grid_loss_metering_point_periods_factories.create_row(
            energy_supplier_id="energy_supplier_id",
            balance_responsible_id="balance_responsible_id",
            metering_point_type=metering_point_type,
        )
        grid_loss_metering_point_periods = grid_loss_metering_point_periods_factories.create(
            spark, [grid_loss_metering_point_periods]
        )

        # Act
        actual = apply_grid_loss_adjustment(
            result,
            grid_loss,
            grid_loss_metering_point_periods,
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
    def test_returns_quantity_from_result_and_grid_loss(
        self,
        spark: SparkSession,
        metering_point_type: MeteringPointType,
    ) -> None:
        # Arrange
        result_row = energy_results_factories.create_row(
            observation_time=DEFAULT_OBSERVATION_TIME,
            energy_supplier_id="energy_supplier_id",
            balance_responsible_id="balance_responsible_id",
            quantity=20,
        )
        result = energy_results_factories.create(spark, [result_row])

        grid_loss_row = energy_results_factories.create_row(
            observation_time=DEFAULT_OBSERVATION_TIME,
            quantity=10,
        )
        grid_loss = energy_results_factories.create(spark, [grid_loss_row])

        grid_loss_metering_point_periods_row = grid_loss_metering_point_periods_factories.create_row(
            energy_supplier_id="energy_supplier_id",
            balance_responsible_id="balance_responsible_id",
            metering_point_type=metering_point_type,
        )
        grid_loss_metering_point_periods = grid_loss_metering_point_periods_factories.create(
            spark, [grid_loss_metering_point_periods_row]
        )

        # Act
        actual = apply_grid_loss_adjustment(
            result,
            grid_loss,
            grid_loss_metering_point_periods,
            metering_point_type,
        )

        # Assert
        actual_row = actual.df.collect()[0]
        actual_quantity = actual_row[Colname.quantity]
        assert actual_quantity == 30


class TestWhenEnergySupplierIdIsNotGridLossResponsible:
    @pytest.mark.parametrize(
        "metering_point_type",
        [
            MeteringPointType.CONSUMPTION,
            MeteringPointType.PRODUCTION,
        ],
    )
    def test_returns_result_quantity_equal_to_correct_adjusted_grid_loss(
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
                quantity=10,
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
                quantity=20,
                qualities=[QuantityQuality.MEASURED],
            )
        ]
        grid_loss = energy_results_factories.create(spark, grid_loss_rows)

        grid_loss_metering_point_periods_rows = [
            grid_loss_metering_point_periods_factories.create_row(
                grid_area="1",
                metering_point_type=metering_point_type,
                energy_supplier_id="grid_loss_responsible_1",
                from_date=DEFAULT_FROM_DATE,
                to_date=DEFAULT_TO_DATE,
            )
        ]
        grid_loss_metering_point_periods = grid_loss_metering_point_periods_factories.create(
            spark, grid_loss_metering_point_periods_rows
        )

        # Act
        actual = apply_grid_loss_adjustment(
            result,
            grid_loss,
            grid_loss_metering_point_periods,
            metering_point_type,
        )

        # Assert
        assert actual.df.count() == 2
        assert actual.df.collect()[0][Colname.quantity] == 20
        assert actual.df.collect()[1][Colname.quantity] == 10


class TestWhenEnergySupplierOnlyHasGridLossMeteringPoints:
    @pytest.mark.parametrize(
        "metering_point_type",
        [
            MeteringPointType.CONSUMPTION,
            MeteringPointType.PRODUCTION,
        ],
    )
    def test_appends_grid_loss_to_energy_result(
        self,
        spark: SparkSession,
        metering_point_type: MeteringPointType,
    ) -> None:
        # Arrange
        result = energy_results_factories.create(spark, [])

        grid_loss_rows = [
            energy_results_factories.create_row(
                grid_area="1",
                observation_time=DEFAULT_OBSERVATION_TIME,
                quantity=20,
            )
        ]
        grid_loss = energy_results_factories.create(spark, grid_loss_rows)

        grid_loss_metering_point_periods_rows = [
            grid_loss_metering_point_periods_factories.create_row(
                grid_area="1",
                metering_point_type=metering_point_type,
                energy_supplier_id="energy_supplier_id",
                balance_responsible_id="balance_responsible_id",
                from_date=DEFAULT_FROM_DATE,
                to_date=DEFAULT_TO_DATE,
            )
        ]
        grid_loss_metering_point_periods = grid_loss_metering_point_periods_factories.create(
            spark, grid_loss_metering_point_periods_rows
        )

        # Act
        actual = apply_grid_loss_adjustment(
            result,
            grid_loss,
            grid_loss_metering_point_periods,
            metering_point_type,
        )

        # Assert
        assert actual.df.count() == 1
        assert actual.df.collect()[0][Colname.quantity] == 20
        assert actual.df.collect()[0][Colname.energy_supplier_id] == "energy_supplier_id"
        assert actual.df.collect()[0][Colname.balance_responsible_party_id] == "balance_responsible_id"


class TestWhenGridLossResponsibleIsChangedWithinPeriod:
    @pytest.mark.parametrize(
        "metering_point_type",
        [
            MeteringPointType.CONSUMPTION,
            MeteringPointType.PRODUCTION,
        ],
    )
    def test_returns_correct_energy_supplier_within_grid_loss_metering_point_period(
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
                quantity=10,
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
                quantity=20,
                qualities=[QuantityQuality.MEASURED],
            ),
            energy_results_factories.create_row(
                grid_area="1",
                from_grid_area=None,
                to_grid_area=None,
                balance_responsible_id=None,
                energy_supplier_id=None,
                observation_time=from_date_2,
                quantity=30,
                qualities=[QuantityQuality.MEASURED],
            ),
        ]
        grid_loss = energy_results_factories.create(spark, grid_loss_rows)

        grid_loss_metering_point_periods_rows = [
            grid_loss_metering_point_periods_factories.create_row(
                grid_area="1",
                metering_point_type=metering_point_type,
                energy_supplier_id="grid_loss_responsible_1",
                from_date=from_date_1,
                to_date=to_date_1,
            ),
            grid_loss_metering_point_periods_factories.create_row(
                grid_area="1",
                metering_point_type=metering_point_type,
                energy_supplier_id="grid_loss_responsible_2",
                from_date=from_date_2,
                to_date=to_date_2,
            ),
        ]
        grid_loss_metering_point_periods = grid_loss_metering_point_periods_factories.create(
            spark, grid_loss_metering_point_periods_rows
        )

        # Act
        actual = apply_grid_loss_adjustment(
            result,
            grid_loss,
            grid_loss_metering_point_periods,
            metering_point_type,
        )

        # Assert
        assert actual.df.count() == 3
        assert actual.df.collect()[0][Colname.quantity] == 20
        assert actual.df.collect()[0][Colname.energy_supplier_id] == "grid_loss_responsible_1"
        assert actual.df.collect()[1][Colname.quantity] == 30
        assert actual.df.collect()[1][Colname.energy_supplier_id] == "grid_loss_responsible_2"
        assert actual.df.collect()[2][Colname.quantity] == 10
        assert actual.df.collect()[2][Colname.energy_supplier_id] == "grid_loss_responsible_2"
