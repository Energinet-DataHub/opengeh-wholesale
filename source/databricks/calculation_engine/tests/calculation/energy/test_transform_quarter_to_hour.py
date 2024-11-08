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

import pytest
from pyspark.sql import SparkSession

import tests.calculation.preparation.transformations.prepared_metering_point_time_series_factory as factory
from package.constants import Colname
from package.codelists import MeteringPointResolution, QuantityQuality
from package.calculation.energy.quarter_to_hour import (
    transform_quarter_to_hour,
)

DEFAULT_QUANTITY = Decimal("4.444000")


def test__transform_quarter_to_hour__when_valid_input__merge_basis_data_time_series(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [
        factory.create_row(
            resolution=MeteringPointResolution.HOUR, quantity=DEFAULT_QUANTITY
        ),
        factory.create_row(
            resolution=MeteringPointResolution.QUARTER,
            observation_time=datetime(2020, 1, 1, 0, 0),
            quantity=DEFAULT_QUANTITY,
        ),
        factory.create_row(
            resolution=MeteringPointResolution.QUARTER,
            observation_time=datetime(2020, 1, 1, 0, 15),
            quantity=DEFAULT_QUANTITY,
        ),
        factory.create_row(
            resolution=MeteringPointResolution.QUARTER,
            observation_time=datetime(2020, 1, 1, 0, 30),
            quantity=DEFAULT_QUANTITY,
        ),
        factory.create_row(
            resolution=MeteringPointResolution.QUARTER,
            observation_time=datetime(2020, 1, 1, 0, 45),
            quantity=DEFAULT_QUANTITY,
        ),
        factory.create_row(
            resolution=MeteringPointResolution.QUARTER,
            observation_time=datetime(2020, 1, 1, 1, 00),
            quantity=DEFAULT_QUANTITY,
        ),
        factory.create_row(
            resolution=MeteringPointResolution.QUARTER,
            observation_time=datetime(2020, 1, 1, 1, 15),
            quantity=DEFAULT_QUANTITY,
        ),
        factory.create_row(
            resolution=MeteringPointResolution.QUARTER,
            observation_time=datetime(2020, 1, 1, 1, 30),
            quantity=DEFAULT_QUANTITY,
        ),
        factory.create_row(
            resolution=MeteringPointResolution.QUARTER,
            observation_time=datetime(2020, 1, 1, 1, 45),
            quantity=DEFAULT_QUANTITY,
        ),
    ]

    prepared_metering_point_time_series = factory.create(spark, rows)
    # Act
    actual = transform_quarter_to_hour(prepared_metering_point_time_series)

    # Assert
    assert actual.df.count() == 3
    assert actual.df.collect()[0][Colname.quantity] == DEFAULT_QUANTITY
    assert actual.df.collect()[1][Colname.quantity] == 4 * DEFAULT_QUANTITY
    assert actual.df.collect()[2][Colname.quantity] == 4 * DEFAULT_QUANTITY


@pytest.mark.parametrize(
    "quality_1, quality_2, quality_3, quality_4, expected_quality",
    [
        (
            QuantityQuality.MISSING,
            QuantityQuality.MISSING,
            QuantityQuality.MISSING,
            QuantityQuality.MISSING,
            QuantityQuality.MISSING,
        ),
        (
            QuantityQuality.MEASURED,
            QuantityQuality.MEASURED,
            QuantityQuality.MEASURED,
            QuantityQuality.MEASURED,
            QuantityQuality.MEASURED,
        ),
        (
            QuantityQuality.MISSING,
            QuantityQuality.MISSING,
            QuantityQuality.MISSING,
            QuantityQuality.ESTIMATED,
            QuantityQuality.ESTIMATED,
        ),
        (
            QuantityQuality.MISSING,
            QuantityQuality.MISSING,
            QuantityQuality.MISSING,
            QuantityQuality.MEASURED,
            QuantityQuality.ESTIMATED,
        ),
        (
            QuantityQuality.MISSING,
            QuantityQuality.MISSING,
            QuantityQuality.ESTIMATED,
            QuantityQuality.MEASURED,
            QuantityQuality.ESTIMATED,
        ),
        (
            QuantityQuality.MISSING,
            QuantityQuality.ESTIMATED,
            QuantityQuality.MEASURED,
            QuantityQuality.CALCULATED,
            QuantityQuality.CALCULATED,
        ),
    ],
)
def test__transform_quarter_to_hour__when_different_qualities__uses_correct_quality(
    spark: SparkSession,
    quality_1: QuantityQuality,
    quality_2: QuantityQuality,
    quality_3: QuantityQuality,
    quality_4: QuantityQuality,
    expected_quality: QuantityQuality,
) -> None:
    # Arrange
    rows = [
        factory.create_row(
            resolution=MeteringPointResolution.QUARTER,
            observation_time=datetime(2020, 1, 1, 1, 00),
            quantity=DEFAULT_QUANTITY,
            quality=quality_1,
        ),
        factory.create_row(
            resolution=MeteringPointResolution.QUARTER,
            observation_time=datetime(2020, 1, 1, 1, 15),
            quantity=DEFAULT_QUANTITY,
            quality=quality_2,
        ),
        factory.create_row(
            resolution=MeteringPointResolution.QUARTER,
            observation_time=datetime(2020, 1, 1, 1, 30),
            quantity=DEFAULT_QUANTITY,
            quality=quality_3,
        ),
        factory.create_row(
            resolution=MeteringPointResolution.QUARTER,
            observation_time=datetime(2020, 1, 1, 1, 45),
            quantity=DEFAULT_QUANTITY,
            quality=quality_4,
        ),
    ]

    prepared_metering_point_time_series = factory.create(spark, rows)

    # Act
    actual = transform_quarter_to_hour(prepared_metering_point_time_series)

    # Assert
    assert actual.df.collect()[0][Colname.quality] == expected_quality.value
