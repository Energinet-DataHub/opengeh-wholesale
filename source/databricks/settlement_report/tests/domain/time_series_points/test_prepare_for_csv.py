from datetime import datetime
from decimal import Decimal

import pytest
from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.functions import monotonically_increasing_id
import pyspark.sql.functions as F
from pyspark.sql.types import DecimalType

import tests.test_factories.default_test_data_spec as default_data
import tests.test_factories.metering_point_time_series_factory as time_series_points_factory
from settlement_report_job.domain.utils.market_role import MarketRole

from settlement_report_job.domain.time_series_points.prepare_for_csv import (
    prepare_for_csv,
)
from settlement_report_job.domain.utils.csv_column_names import (
    CsvColumnNames,
)
from settlement_report_job.infrastructure.wholesale.column_names import (
    DataProductColumnNames,
)
from settlement_report_job.infrastructure.wholesale.data_values import (
    MeteringPointResolutionDataProductValue,
)

DEFAULT_TIME_ZONE = "Europe/Copenhagen"
DEFAULT_FROM_DATE = default_data.DEFAULT_FROM_DATE
DEFAULT_TO_DATE = default_data.DEFAULT_TO_DATE
DATAHUB_ADMINISTRATOR_ID = "1234567890123"
SYSTEM_OPERATOR_ID = "3333333333333"
NOT_SYSTEM_OPERATOR_ID = "4444444444444"
DEFAULT_MARKET_ROLE = MarketRole.GRID_ACCESS_PROVIDER


def _create_time_series_points_with_increasing_quantity(
    spark: SparkSession,
    from_date: datetime,
    to_date: datetime,
    resolution: MeteringPointResolutionDataProductValue,
) -> DataFrame:
    spec = default_data.create_time_series_points_data_spec(
        from_date=from_date, to_date=to_date, resolution=resolution
    )
    df = time_series_points_factory.create(spark, spec)
    return df.withColumn(  # just set quantity equal to its row number
        DataProductColumnNames.quantity,
        monotonically_increasing_id().cast(DecimalType(18, 3)),
    )


@pytest.mark.parametrize(
    "resolution",
    [
        MeteringPointResolutionDataProductValue.HOUR,
        MeteringPointResolutionDataProductValue.QUARTER,
    ],
)
def test_prepare_for_csv__when_two_days_of_data__returns_two_rows(
    spark: SparkSession, resolution: MeteringPointResolutionDataProductValue
) -> None:
    # Arrange
    expected_rows = DEFAULT_TO_DATE.day - DEFAULT_FROM_DATE.day
    spec = default_data.create_time_series_points_data_spec(
        from_date=DEFAULT_FROM_DATE, to_date=DEFAULT_TO_DATE, resolution=resolution
    )
    df = time_series_points_factory.create(spark, spec)

    # Act
    result_df = prepare_for_csv(
        filtered_time_series_points=df,
        metering_point_resolution=resolution,
        time_zone=DEFAULT_TIME_ZONE,
        requesting_actor_market_role=DEFAULT_MARKET_ROLE,
    )

    # Assert
    assert result_df.count() == expected_rows


@pytest.mark.parametrize(
    "resolution, energy_quantity_column_count",
    [
        (MeteringPointResolutionDataProductValue.HOUR, 25),
        (MeteringPointResolutionDataProductValue.QUARTER, 100),
    ],
)
def test_prepare_for_csv__returns_expected_energy_quantity_columns(
    spark: SparkSession,
    resolution: MeteringPointResolutionDataProductValue,
    energy_quantity_column_count: int,
) -> None:
    # Arrange
    expected_columns = [
        f"ENERGYQUANTITY{i}" for i in range(1, energy_quantity_column_count + 1)
    ]
    spec = default_data.create_time_series_points_data_spec(resolution=resolution)
    df = time_series_points_factory.create(spark, spec)

    # Act
    actual_df = prepare_for_csv(
        filtered_time_series_points=df,
        metering_point_resolution=resolution,
        time_zone=DEFAULT_TIME_ZONE,
        requesting_actor_market_role=DEFAULT_MARKET_ROLE,
    )

    # Assert
    actual_columns = [
        col for col in actual_df.columns if col.startswith("ENERGYQUANTITY")
    ]
    assert set(actual_columns) == set(expected_columns)


@pytest.mark.parametrize(
    "from_date,to_date,resolution,expected_columns_with_data",
    [
        (
            # Entering daylight saving time for hourly resolution
            datetime(2023, 3, 25, 23),
            datetime(2023, 3, 27, 22),
            MeteringPointResolutionDataProductValue.HOUR,
            23,
        ),
        (
            # Entering daylight saving time for quarterly resolution
            datetime(2023, 3, 25, 23),
            datetime(2023, 3, 27, 22),
            MeteringPointResolutionDataProductValue.QUARTER,
            92,
        ),
        (
            # Exiting daylight saving time for hourly resolution
            datetime(2023, 10, 28, 22),
            datetime(2023, 10, 30, 23),
            MeteringPointResolutionDataProductValue.HOUR,
            25,
        ),
        (
            # Exiting daylight saving time for quarterly resolution
            datetime(2023, 10, 28, 22),
            datetime(2023, 10, 30, 23),
            MeteringPointResolutionDataProductValue.QUARTER,
            100,
        ),
    ],
)
def test_prepare_for_csv__when_daylight_saving_tim_transition__returns_expected_energy_quantities(
    spark: SparkSession,
    from_date: datetime,
    to_date: datetime,
    resolution: MeteringPointResolutionDataProductValue,
    expected_columns_with_data: int,
) -> None:
    # Arrange
    df = _create_time_series_points_with_increasing_quantity(
        spark=spark,
        from_date=from_date,
        to_date=to_date,
        resolution=resolution,
    )
    total_columns = (
        25 if resolution == MeteringPointResolutionDataProductValue.HOUR else 100
    )

    # Act
    actual_df = prepare_for_csv(
        filtered_time_series_points=df,
        metering_point_resolution=resolution,
        time_zone=DEFAULT_TIME_ZONE,
        requesting_actor_market_role=DEFAULT_MARKET_ROLE,
    )

    # Assert
    assert actual_df.count() == 2
    dst_day = actual_df.where(F.col(CsvColumnNames.time) == from_date).collect()[0]
    for i in range(1, total_columns):
        expected_value = None if i > expected_columns_with_data else Decimal(i - 1)
        assert dst_day[f"ENERGYQUANTITY{i}"] == expected_value
