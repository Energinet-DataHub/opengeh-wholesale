import uuid
from datetime import datetime, timedelta
from decimal import Decimal
from functools import reduce
from unittest.mock import Mock

import pytest
from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.functions import monotonically_increasing_id
import pyspark.sql.functions as F
from pyspark.sql.types import DecimalType

import tests.test_factories.metering_point_time_series_factory as factory

from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.domain.metering_point_resolution import (
    DataProductMeteringPointResolution,
)
from settlement_report_job.domain.system_operator_filter import (
    filter_by_charge_owner_on_metering_point,
)
from settlement_report_job.domain.time_series_factory import create_time_series
from settlement_report_job.infrastructure.column_names import (
    DataProductColumnNames,
    TimeSeriesPointCsvColumnNames,
)

DEFAULT_TIME_ZONE = "Europe/Copenhagen"
DEFAULT_FROM_DATE = datetime(2024, 1, 1, 23)
DEFAULT_TO_DATE = DEFAULT_FROM_DATE + timedelta(days=1)
DATAHUB_ADMINISTRATOR_ID = "1234567890123"


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
    expected_rows = DEFAULT_TO_DATE.day - DEFAULT_FROM_DATE.day
    spec = factory.MeteringPointTimeSeriesTestDataSpec(
        from_date=DEFAULT_FROM_DATE, to_date=DEFAULT_TO_DATE, resolution=resolution
    )
    time_series_df = factory.create(spark, spec)
    charge_link_periods_df = factory.create_charge_link_periods(spark)
    mock_repository = Mock()
    mock_repository.read_metering_point_time_series.return_value = time_series_df

    # Act

    result_df = filter_by_charge_owner_on_metering_point(
        period_start=DEFAULT_FROM_DATE,
        period_end=DEFAULT_TO_DATE,
        calculation_id_by_grid_area={
            factory.DEFAULT_GRID_AREA_CODE: uuid.UUID(factory.DEFAULT_CALCULATION_ID)
        },
        energy_supplier_ids=None,
        resolution=resolution,
        requesting_actor_market_role=MarketRole.DATAHUB_ADMINISTRATOR,
        requesting_actor_id=DATAHUB_ADMINISTRATOR_ID,
        time_zone=DEFAULT_TIME_ZONE,
        repository=mock_repository,
    )

    # Assert
    assert result_df.count() == expected_rows
