from decimal import Decimal
from zoneinfo import ZoneInfo

import pytest
from pyspark.sql import SparkSession
from pyspark.sql.functions import col

from geh_wholesale.calculation.energy.aggregators.grid_loss_aggregators import (
    calculate_positive_grid_loss,
)
from geh_wholesale.calculation.energy.data_structures.energy_results import (
    EnergyResults,
)
from geh_wholesale.codelists import (
    MeteringPointType,
    QuantityQuality,
)
from geh_wholesale.constants import Colname
from tests.calculation.energy import energy_results_factories, grid_loss_metering_point_periods_factories


@pytest.fixture(scope="module")
def actual_positive_grid_loss(spark: SparkSession) -> EnergyResults:
    rows = [
        energy_results_factories.create_grid_loss_row(
            grid_area="001",
            quantity=Decimal(-12.567),
            observation_time=grid_loss_metering_point_periods_factories.DEFAULT_FROM_DATE,
        ),
        energy_results_factories.create_grid_loss_row(
            grid_area="002",
            quantity=Decimal(34.32),
            observation_time=grid_loss_metering_point_periods_factories.DEFAULT_FROM_DATE,
        ),
        energy_results_factories.create_grid_loss_row(
            grid_area="003",
            quantity=Decimal(0.0),
            observation_time=grid_loss_metering_point_periods_factories.DEFAULT_FROM_DATE,
        ),
    ]

    grid_loss = energy_results_factories.create(spark, rows)

    responsible_rows = [
        grid_loss_metering_point_periods_factories.create_row(
            grid_area="001",
            metering_point_id="a",
            metering_point_type=MeteringPointType.CONSUMPTION,
        ),
        grid_loss_metering_point_periods_factories.create_row(
            grid_area="002",
            metering_point_id="b",
            metering_point_type=MeteringPointType.CONSUMPTION,
        ),
        grid_loss_metering_point_periods_factories.create_row(
            grid_area="003",
            metering_point_id="c",
            metering_point_type=MeteringPointType.CONSUMPTION,
        ),
    ]
    grid_loss_metering_point_periods = grid_loss_metering_point_periods_factories.create(spark, responsible_rows)

    return calculate_positive_grid_loss(grid_loss, grid_loss_metering_point_periods)


class TestWhenValidInput:
    def test__has_no_values_below_zero(
        self,
        actual_positive_grid_loss: EnergyResults,
    ) -> None:
        assert actual_positive_grid_loss.df.where(col(Colname.quantity) < 0).count() == 0

    def test__changes_negative_values_to_zero(
        self,
        actual_positive_grid_loss: EnergyResults,
    ) -> None:
        assert actual_positive_grid_loss.df.collect()[0][Colname.quantity] == Decimal("0.00000")

    def test__positive_values_will_not_change(
        self,
        actual_positive_grid_loss: EnergyResults,
    ) -> None:
        assert actual_positive_grid_loss.df.collect()[1][Colname.quantity] == Decimal("34.32000")

    def test__values_that_are_zero_stay_zero(
        self,
        actual_positive_grid_loss: EnergyResults,
    ) -> None:
        assert actual_positive_grid_loss.df.collect()[2][Colname.quantity] == Decimal("0.00000")

    def test__has_expected_values(
        self,
        actual_positive_grid_loss: EnergyResults,
    ) -> None:
        actual_row = actual_positive_grid_loss.df.collect()[1].asDict()
        actual_row[Colname.observation_time] = actual_row[Colname.observation_time].astimezone(
            ZoneInfo("Europe/Copenhagen")
        )

        expected_row = {
            Colname.grid_area_code: "002",
            Colname.to_grid_area_code: None,
            Colname.from_grid_area_code: None,
            Colname.balance_responsible_party_id: grid_loss_metering_point_periods_factories.DEFAULT_BALANCE_RESPONSIBLE_ID,
            Colname.energy_supplier_id: grid_loss_metering_point_periods_factories.DEFAULT_ENERGY_SUPPLIER_ID,
            Colname.observation_time: grid_loss_metering_point_periods_factories.DEFAULT_FROM_DATE,
            Colname.quantity: Decimal("34.320000"),
            Colname.qualities: [QuantityQuality.CALCULATED.value],
            Colname.metering_point_id: "b",
        }

        assert actual_row == expected_row
