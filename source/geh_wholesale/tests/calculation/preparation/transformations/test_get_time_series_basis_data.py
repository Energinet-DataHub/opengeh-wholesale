from geh_common.testing.dataframes.assert_schemas import assert_schema
from pyspark.sql import SparkSession

from geh_wholesale.databases.wholesale_basis_data_internal import (
    get_time_series_points_basis_data,
)
from geh_wholesale.databases.wholesale_basis_data_internal.schemas import (
    time_series_points_schema,
)
from tests.calculation.preparation.transformations import (
    prepared_metering_point_time_series_factory,
)


def test__when_valid_input__returns_df_with_expected_schema(
    spark: SparkSession,
) -> None:
    # Arrange
    metering_point_time_series = prepared_metering_point_time_series_factory.create(spark)

    # Act
    actual = get_time_series_points_basis_data("some-calculation-id", metering_point_time_series)

    # Assert
    assert_schema(
        actual.schema,
        time_series_points_schema,
        ignore_decimal_scale=True,
        ignore_nullability=True,  # TODO JVM: This should be False / remove when time_series_points_schema is updated
    )
