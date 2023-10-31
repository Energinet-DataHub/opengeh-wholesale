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

import datetime
from decimal import Decimal

import pytest
from pyspark.sql import Row, SparkSession

from package.calculation.energy.energy_results import (
    EnergyResults,
    energy_results_schema,
)
from package.calculation.energy.grid_loss_calculator import (
    calculate_total_consumption as sut,
)
from package.codelists import MeteringPointType, QuantityQuality
from package.constants import Colname

DEFAULT_GRID_AREA = "100"
DEFAULT_FROM_GRID_AREA = "200"
DEFAULT_TO_GRID_AREA = "300"
DEFAULT_OBSERVATION_TIME = datetime.datetime.now()
DEFAULT_SUM_QUANTITY = Decimal("999.123456")
DEFAULT_QUALITIES = [QuantityQuality.MEASURED]
DEFAULT_METERING_POINT_TYPE = MeteringPointType.CONSUMPTION


def create_energy_result_point(
    grid_area: str = DEFAULT_GRID_AREA,
    from_grid_area: str = DEFAULT_FROM_GRID_AREA,
    to_grid_area: str = DEFAULT_TO_GRID_AREA,
    observation_time: datetime = DEFAULT_OBSERVATION_TIME,
    sum_quantity: Decimal = DEFAULT_SUM_QUANTITY,
    qualities: None | list[QuantityQuality] = None,
    metering_point_type: MeteringPointType = DEFAULT_METERING_POINT_TYPE,
) -> Row:
    if qualities is None:
        qualities = DEFAULT_QUALITIES
    qualities = [q.value for q in qualities]

    row = {
        Colname.grid_area: grid_area,
        Colname.from_grid_area: from_grid_area,
        Colname.to_grid_area: to_grid_area,
        Colname.balance_responsible_id: None,
        Colname.energy_supplier_id: None,
        Colname.time_window: {
            Colname.start: observation_time,
            Colname.end: observation_time + datetime.timedelta(minutes=15),
        },
        Colname.sum_quantity: sum_quantity,
        Colname.qualities: qualities,
        Colname.metering_point_type: metering_point_type.value,
        Colname.settlement_method: None,
    }
    if type(qualities) == str:
        row[Colname.qualities] = [qualities]

    return Row(**row)


def create_energy_results(
    spark: SparkSession, data: None | Row | list[Row] = None
) -> EnergyResults:
    if type(data) == Row:
        data = [data]
    elif type(data) is None:
        data = [create_energy_result_point()]
    df = spark.createDataFrame(data, schema=energy_results_schema)
    return EnergyResults(df)


@pytest.mark.parametrize(
    "prod_qualities, exchange_qualities, expected_qualities",
    [
        (
            [QuantityQuality.MEASURED],
            [QuantityQuality.CALCULATED],
            [QuantityQuality.MEASURED.value, QuantityQuality.CALCULATED.value],
        ),
        (
            [QuantityQuality.MEASURED, QuantityQuality.CALCULATED],
            [QuantityQuality.MEASURED, QuantityQuality.CALCULATED],
            [QuantityQuality.MEASURED.value, QuantityQuality.CALCULATED.value],
        ),
    ],
)
def test__when_valid_input__returns_expected_qualities(
    spark: SparkSession,
    prod_qualities: list[QuantityQuality],
    exchange_qualities: list[QuantityQuality],
    expected_qualities: list[str],
) -> None:
    """
    Test that qualities from both production and net exchange is aggregated.
    """
    # Arrange
    production = create_energy_result_point(qualities=prod_qualities)
    net_exchange = create_energy_result_point(qualities=exchange_qualities)
    production_per_ga = create_energy_results(spark, production)
    net_exchange_per_ga = create_energy_results(spark, net_exchange)

    # Act
    actual = sut(production_per_ga, net_exchange_per_ga)

    # Assert
    actual.df.show()
    actual_row = actual.df.collect()[0]
    assert sorted(actual_row[Colname.qualities]) == sorted(expected_qualities)


def test__when_valid_input__includes_only_qualities_from_neighbour_ga(
    spark: SparkSession,
) -> None:
    """
    Test that qualities from both production and net exchange is aggregated.
    """
    # Arrange
    production = create_energy_result_point(qualities=[QuantityQuality.MEASURED])
    net_exchange = create_energy_result_point(qualities=[QuantityQuality.CALCULATED])
    net_exchange_other_ga = create_energy_result_point(
        grid_area="some-other-grid-area", qualities=[QuantityQuality.ESTIMATED]
    )
    production_per_ga = create_energy_results(spark, production)
    net_exchange_per_ga = create_energy_results(
        spark, [net_exchange, net_exchange_other_ga]
    )

    # Act
    actual = sut(production_per_ga, net_exchange_per_ga)

    # Assert
    actual_row = actual.df.collect()[0]
    assert sorted(actual_row[Colname.qualities]) == sorted(
        [QuantityQuality.MEASURED.value, QuantityQuality.CALCULATED.value]
    )


def test_grid_area_total_consumption(spark: SparkSession):
    # Arrange
    production = create_energy_result_point()
    net_exchange = create_energy_result_point()
    net_exchange_other_ga = create_energy_result_point(grid_area="some-other-grid-area")
    production_per_ga = create_energy_results(spark, production)
    net_exchange_per_ga = create_energy_results(
        spark, [net_exchange, net_exchange_other_ga]
    )

    # Act
    actual = sut(production_per_ga, net_exchange_per_ga)

    # Assert
    assert False
