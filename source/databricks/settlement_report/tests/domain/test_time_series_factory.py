import uuid
from datetime import datetime, timedelta
from decimal import Decimal
from unittest.mock import Mock

import pytest
from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.functions import monotonically_increasing_id
import pyspark.sql.functions as F
from pyspark.sql.types import DecimalType

import metering_point_time_series_factory as factory
from settlement_report_job.domain.metering_point_resolution import (
    DataProductMeteringPointResolution,
)
from settlement_report_job.domain.time_series_factory import create_time_series
from settlement_report_job.infrastructure.column_names import (
    DataProductColumnNames,
    TimeSeriesPointCsvColumnNames,
)

DEFAULT_PERIOD_START = datetime(2024, 1, 2, 22)
DEFAULT_PERIOD_END = datetime(2024, 5, 31, 2)
DEFAULT_TIME_ZONE = "Europe/Copenhagen"
DEFAULT_CALCULATION_BY_GRID_AREA = {
    "804": uuid.UUID("11111111-1111-1111-1111-111111111111"),
    "805": uuid.UUID("22222222-2222-2222-2222-222222222222"),
}


def _create_time_series_with_increasing_quantity(
    spark: SparkSession,
    from_date: datetime,
    to_date: datetime,
    resolution: DataProductMeteringPointResolution,
) -> DataFrame:
    spec = factory.MeteringPointTimeSeriesTestDataSpec(
        from_date=from_date, to_date=to_date, resolution=resolution
    )
    df = factory.create(spark, spec)
    return df.withColumn(  # just set quantity equal to its row number
        DataProductColumnNames.quantity,
        monotonically_increasing_id().cast(DecimalType(18, 3)),
    )


@pytest.mark.parametrize(
    "resolution",
    [
        DataProductMeteringPointResolution.HOUR,
        DataProductMeteringPointResolution.QUARTER,
    ],
)
def test_create_time_series__when_two_days_of_data__returns_two_rows(
    spark: SparkSession, resolution: DataProductMeteringPointResolution
) -> None:
    # Arrange
    from_date = datetime(2024, 1, 1, 23)
    to_date = from_date + timedelta(days=1)
    expected_rows = to_date.day - from_date.day
    spec = factory.MeteringPointTimeSeriesTestDataSpec(
        from_date=from_date, to_date=to_date, resolution=resolution
    )
    df = factory.create(spark, spec)
    mock_repository = Mock()
    mock_repository.read_metering_point_time_series.return_value = df

    # Act
    result_df = create_time_series(
        period_start=from_date,
        period_end=to_date,
        calculation_id_by_grid_area={
            factory.DEFAULT_GRID_AREA_CODE: uuid.UUID(factory.DEFAULT_CALCULATION_ID)
        },
        resolution=resolution,
        time_zone=DEFAULT_TIME_ZONE,
        repository=mock_repository,
    )

    # Assert
    assert result_df.count() == expected_rows


@pytest.mark.parametrize(
    "resolution, energy_quantity_column_count",
    [
        (DataProductMeteringPointResolution.HOUR, 25),
        (DataProductMeteringPointResolution.QUARTER, 100),
    ],
)
def test_create_time_series__returns_expected_energy_quantity_columns(
    spark: SparkSession,
    resolution: DataProductMeteringPointResolution,
    energy_quantity_column_count: int,
) -> None:
    # Arrange
    expected_columns = [
        f"ENERGYQUANTITY{i}" for i in range(1, energy_quantity_column_count + 1)
    ]
    from_date = datetime(2024, 1, 1, 23)
    to_date = from_date + timedelta(days=1)
    spec = factory.MeteringPointTimeSeriesTestDataSpec(
        from_date=from_date, to_date=to_date, resolution=resolution
    )
    df = factory.create(spark, spec)
    mock_repository = Mock()
    mock_repository.read_metering_point_time_series.return_value = df

    # Act
    actual_df = create_time_series(
        period_start=from_date,
        period_end=to_date,
        calculation_id_by_grid_area={
            factory.DEFAULT_GRID_AREA_CODE: uuid.UUID(factory.DEFAULT_CALCULATION_ID)
        },
        resolution=resolution,
        time_zone=DEFAULT_TIME_ZONE,
        repository=mock_repository,
    )

    # Assert
    actual_columns = [
        col for col in actual_df.columns if col.startswith("ENERGYQUANTITY")
    ]
    assert set(actual_columns) == set(expected_columns)


# @pytest.mark.parametrize(
#     "from_date, to_date",
#     [
#         (DataProductMeteringPointResolution.QUARTER, 100),
#     ],
# )
def test_create_time_series__when_entering_daylight_saving_time__returns_expected_energy_quantities(
    spark: SparkSession,
    # resolution: DataProductMeteringPointResolution,
    # energy_quantity_column_count,
) -> None:
    # Arrange
    resolution = DataProductMeteringPointResolution.HOUR
    from_date = datetime(2023, 3, 25, 23)
    to_date = datetime(2023, 3, 27, 22)
    df = _create_time_series_with_increasing_quantity(
        spark=spark,
        from_date=from_date,
        to_date=to_date,
        resolution=resolution,
    )

    mock_repository = Mock()
    mock_repository.read_metering_point_time_series.return_value = df

    # Act
    actual_df = create_time_series(
        period_start=from_date,
        period_end=to_date,
        calculation_id_by_grid_area={
            factory.DEFAULT_GRID_AREA_CODE: uuid.UUID(factory.DEFAULT_CALCULATION_ID)
        },
        resolution=resolution,
        time_zone=DEFAULT_TIME_ZONE,
        repository=mock_repository,
    )

    # Assert
    assert actual_df.count() == 2
    dst_day = actual_df.where(
        F.col(TimeSeriesPointCsvColumnNames.start_of_day) == from_date
    ).collect()[0]
    for i in range(1, 25):
        expected_value = None if i == 24 else Decimal(i - 1)
        assert dst_day[f"ENERGYQUANTITY{i}"] == expected_value


# def test_create_time_series__when_input_has_both_resoltution__returns_only_data_with_expected_resolution() -> None:
