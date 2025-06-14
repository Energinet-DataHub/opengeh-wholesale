from datetime import datetime
from decimal import Decimal

import pytest
from pyspark.sql import SparkSession

import tests.calculation.preparation.transformations.prepared_metering_point_time_series_factory as factory
from geh_wholesale.calculation.energy.quarter_to_hour import (
    transform_quarter_to_hour,
)
from geh_wholesale.codelists import MeteringPointResolution, QuantityQuality
from geh_wholesale.constants import Colname

DEFAULT_QUANTITY = Decimal("4.444000")


def test__transform_quarter_to_hour__when_valid_input__merge_basis_data_time_series(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [
        factory.create_row(resolution=MeteringPointResolution.HOUR, quantity=DEFAULT_QUANTITY),
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
    ("quality_1", "quality_2", "quality_3", "quality_4", "expected_quality"),
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
