from datetime import datetime
from decimal import Decimal

import pytest
from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.functions import monotonically_increasing_id
import pyspark.sql.functions as F
from pyspark.sql.types import DecimalType

import test_factories.default_test_data_spec as default_data
import test_factories.metering_point_time_series_factory as time_series_factory

from settlement_report_job.domain.time_series.prepare_for_csv import prepare_for_csv
from settlement_report_job.domain.csv_column_names import (
    CsvColumnNames,
)
from settlement_report_job.wholesale.column_names import DataProductColumnNames
from settlement_report_job.wholesale.data_values import (
    MeteringPointResolutionDataProductValue,
)

DEFAULT_TIME_ZONE = "Europe/Copenhagen"
DEFAULT_FROM_DATE = default_data.DEFAULT_FROM_DATE
DEFAULT_TO_DATE = default_data.DEFAULT_TO_DATE
DATAHUB_ADMINISTRATOR_ID = "1234567890123"
SYSTEM_OPERATOR_ID = "3333333333333"
NOT_SYSTEM_OPERATOR_ID = "4444444444444"


def _create_time_series_with_increasing_quantity(
    spark: SparkSession,
    from_date: datetime,
    to_date: datetime,
    resolution: MeteringPointResolutionDataProductValue,
) -> DataFrame:
    spec = default_data.create_time_series_data_spec(
        from_date=from_date, to_date=to_date, resolution=resolution
    )
    df = time_series_factory.create(spark, spec)
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
    spec = default_data.create_time_series_data_spec(
        from_date=DEFAULT_FROM_DATE, to_date=DEFAULT_TO_DATE, resolution=resolution
    )
    df = time_series_factory.create(spark, spec)
    # Act
    result_df = prepare_for_csv(
        filtered_time_series_points=df,
        metering_point_resolution=resolution,
        time_zone=DEFAULT_TIME_ZONE,
    )

    # Assert
    assert result_df.count() == expected_rows
