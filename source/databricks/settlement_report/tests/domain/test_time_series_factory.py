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

DEFAULT_TIME_ZONE = "Europe/Copenhagen"


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


@pytest.mark.parametrize(
    "from_date,to_date,resolution,expected_columns_with_data",
    [
        (
            # Entering daylight saving time for hourly resolution
            datetime(2023, 3, 25, 23),
            datetime(2023, 3, 27, 22),
            DataProductMeteringPointResolution.HOUR,
            23,
        ),
        (
            # Entering daylight saving time for quarterly resolution
            datetime(2023, 3, 25, 23),
            datetime(2023, 3, 27, 22),
            DataProductMeteringPointResolution.QUARTER,
            92,
        ),
        (
            # Exiting daylight saving time for hourly resolution
            datetime(2023, 10, 28, 22),
            datetime(2023, 10, 30, 23),
            DataProductMeteringPointResolution.HOUR,
            25,
        ),
        (
            # Exiting daylight saving time for quarterly resolution
            datetime(2023, 10, 28, 22),
            datetime(2023, 10, 30, 23),
            DataProductMeteringPointResolution.QUARTER,
            100,
        ),
    ],
)
def test_create_time_series__when_daylight_saving_tim_transition__returns_expected_energy_quantities(
    spark: SparkSession,
    from_date: datetime,
    to_date: datetime,
    resolution: DataProductMeteringPointResolution,
    expected_columns_with_data: int,
) -> None:
    # Arrange
    df = _create_time_series_with_increasing_quantity(
        spark=spark,
        from_date=from_date,
        to_date=to_date,
        resolution=resolution,
    )
    total_columns = 25 if resolution == DataProductMeteringPointResolution.HOUR else 100

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
    for i in range(1, total_columns):
        expected_value = None if i > expected_columns_with_data else Decimal(i - 1)
        assert dst_day[f"ENERGYQUANTITY{i}"] == expected_value


@pytest.mark.parametrize(
    "resolution",
    [
        DataProductMeteringPointResolution.HOUR,
        DataProductMeteringPointResolution.QUARTER,
    ],
)
def test_create_time_series__when_input_has_both_resolution_types__returns_only_data_with_expected_resolution(
    spark: SparkSession,
    resolution: DataProductMeteringPointResolution,
) -> None:
    # Arrange
    from_date = datetime(2024, 1, 1, 23)
    to_date = from_date + timedelta(days=1)
    hourly_metering_point_id = "1111111111111"
    quarterly_metering_point_id = "1515151515115"
    expected_metering_point_id = (
        hourly_metering_point_id
        if resolution == DataProductMeteringPointResolution.HOUR
        else quarterly_metering_point_id
    )
    spec_hour = factory.MeteringPointTimeSeriesTestDataSpec(
        from_date=from_date,
        to_date=to_date,
        metering_point_id=hourly_metering_point_id,
        resolution=DataProductMeteringPointResolution.HOUR,
    )
    spec_quarter = factory.MeteringPointTimeSeriesTestDataSpec(
        from_date=from_date,
        to_date=to_date,
        metering_point_id=quarterly_metering_point_id,
        resolution=DataProductMeteringPointResolution.QUARTER,
    )
    df = factory.create(spark, spec_hour).union(factory.create(spark, spec_quarter))

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
    assert actual_df.count() == 1
    assert (
        actual_df.collect()[0][TimeSeriesPointCsvColumnNames.metering_point_id]
        == expected_metering_point_id
    )


def test_create_time_series__returns_only_days_within_selected_period(
    spark: SparkSession,
) -> None:
    # Arrange
    from_date = datetime(2024, 1, 2, 23)
    to_date = from_date + timedelta(days=1)

    df = factory.create(
        spark,
        factory.MeteringPointTimeSeriesTestDataSpec(
            from_date=from_date, to_date=to_date
        ),
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
        resolution=DataProductMeteringPointResolution.HOUR,
        time_zone=DEFAULT_TIME_ZONE,
        repository=mock_repository,
    )

    # Assert
    assert actual_df.count() == 1
    assert (
        actual_df.collect()[0][TimeSeriesPointCsvColumnNames.start_of_day] == from_date
    )


def test_create_time_series__returns_only_selected_grid_area(
    spark: SparkSession,
) -> None:
    # Arrange
    selected_grid_area_code = "805"
    not_selected_grid_area_code = "806"
    from_date = datetime(2024, 1, 2, 23)
    to_date = from_date + timedelta(days=1)
    df = factory.create(
        spark,
        factory.MeteringPointTimeSeriesTestDataSpec(
            from_date=from_date,
            to_date=to_date,
            grid_area_code=selected_grid_area_code,
        ),
    ).union(
        factory.create(
            spark,
            factory.MeteringPointTimeSeriesTestDataSpec(
                from_date=from_date,
                to_date=to_date,
                grid_area_code=not_selected_grid_area_code,
            ),
        )
    )
    mock_repository = Mock()
    mock_repository.read_metering_point_time_series.return_value = df

    # Act
    actual_df = create_time_series(
        period_start=from_date,
        period_end=to_date,
        calculation_id_by_grid_area={
            selected_grid_area_code: uuid.UUID(factory.DEFAULT_CALCULATION_ID)
        },
        resolution=DataProductMeteringPointResolution.HOUR,
        time_zone=DEFAULT_TIME_ZONE,
        repository=mock_repository,
    )

    # Assert
    assert actual_df.count() == 1
    assert (
        actual_df.collect()[0][DataProductColumnNames.grid_area_code]
        == selected_grid_area_code
    )


def test_create_time_series__returns_only_selected_calculation_id(
    spark: SparkSession,
) -> None:
    # Arrange
    selected_calculation_id = "11111111-9fc8-409a-a169-fbd49479d718"
    not_selected_calculation_id = "22222222-9fc8-409a-a169-fbd49479d718"
    expected_metering_point_id = "123456789012345678901234567"
    other_metering_point_id = "765432109876543210987654321"
    from_date = datetime(2024, 1, 2, 23)
    to_date = from_date + timedelta(days=1)
    df = factory.create(
        spark,
        factory.MeteringPointTimeSeriesTestDataSpec(
            from_date=from_date,
            to_date=to_date,
            calculation_id=selected_calculation_id,
            metering_point_id=expected_metering_point_id,
        ),
    ).union(
        factory.create(
            spark,
            factory.MeteringPointTimeSeriesTestDataSpec(
                from_date=from_date,
                to_date=to_date,
                calculation_id=not_selected_calculation_id,
                metering_point_id=other_metering_point_id,
            ),
        )
    )
    mock_repository = Mock()
    mock_repository.read_metering_point_time_series.return_value = df

    # Act
    actual_df = create_time_series(
        period_start=from_date,
        period_end=to_date,
        calculation_id_by_grid_area={
            factory.DEFAULT_GRID_AREA_CODE: uuid.UUID(selected_calculation_id)
        },
        resolution=DataProductMeteringPointResolution.HOUR,
        time_zone=DEFAULT_TIME_ZONE,
        repository=mock_repository,
    )

    # Assert
    assert actual_df.count() == 1
    assert (
        actual_df.collect()[0][TimeSeriesPointCsvColumnNames.metering_point_id]
        == expected_metering_point_id
    )
