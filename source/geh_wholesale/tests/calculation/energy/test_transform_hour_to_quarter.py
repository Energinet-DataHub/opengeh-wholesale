from decimal import Decimal

from pyspark.sql import SparkSession

import tests.calculation.preparation.transformations.prepared_metering_point_time_series_factory as factory
from geh_wholesale.calculation.energy.hour_to_quarter import (
    transform_hour_to_quarter,
)
from geh_wholesale.codelists import MeteringPointResolution
from geh_wholesale.constants import Colname

DEFAULT_QUANTITY = Decimal("4.444000")


def test__transform_hour_to_quarter__when_valid_input__split_basis_data_time_series(
    spark: SparkSession,
) -> None:
    # Arrange
    rows = [
        factory.create_row(resolution=MeteringPointResolution.HOUR, quantity=DEFAULT_QUANTITY),
        factory.create_row(resolution=MeteringPointResolution.QUARTER, quantity=DEFAULT_QUANTITY),
    ]

    prepared_metering_point_time_series = factory.create(spark, rows)

    # Act
    actual = transform_hour_to_quarter(prepared_metering_point_time_series)

    # Assert
    assert actual.df.count() == 5
    # Check that hourly quantity is divided by 4
    assert actual.df.collect()[0][Colname.quantity] == DEFAULT_QUANTITY / 4
    # Check that quarterly quantity is not divided by 4
    assert actual.df.collect()[4][Colname.quantity] == DEFAULT_QUANTITY
