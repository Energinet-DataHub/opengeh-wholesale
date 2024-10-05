import uuid
from datetime import datetime, timedelta
from unittest.mock import Mock

import pytest
from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.functions import monotonically_increasing_id
from pyspark.sql.types import DecimalType

import test_factories.metering_point_time_series_factory as time_series_factory
import test_factories.charge_link_periods_factory as charge_link_periods_factory
import test_factories.charge_price_information_periods_factory as charge_price_information_periods_factory

from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.domain.metering_point_resolution import (
    DataProductMeteringPointResolution,
)
from settlement_report_job.domain.system_operator_filter import (
    filter_by_charge_owner_on_metering_point,
)
from settlement_report_job.infrastructure.column_names import (
    DataProductColumnNames,
)

DEFAULT_TIME_ZONE = "Europe/Copenhagen"
DEFAULT_FROM_DATE = datetime(2024, 1, 1, 23)
DEFAULT_TO_DATE = DEFAULT_FROM_DATE + timedelta(days=1)
DATAHUB_ADMINISTRATOR_ID = "1234567890123"


# def _create_time_series_with_increasing_quantity(
#     spark: SparkSession,
#     from_date: datetime,
#     to_date: datetime,
#     resolution: DataProductMeteringPointResolution,
# ) -> DataFrame:
#     spec = time_series_factory.MeteringPointTimeSeriesTestDataSpec(
#         from_date=from_date, to_date=to_date, resolution=resolution
#     )
#     df = time_series_factory.create(spark, spec)
#     return df.withColumn(  # just set quantity equal to its row number
#         DataProductColumnNames.quantity,
#         monotonically_increasing_id().cast(DecimalType(18, 3)),
#     )


def test_create_time_series__when_two_days_of_data__returns_two_rows(
    spark: SparkSession, resolution: DataProductMeteringPointResolution
) -> None:
    # Arrange
    time_series_factory.create(
        spark, data_spec=time_series_factory.MeteringPointTimeSeriesTestDataSpec()
    )
    charge_link_periods_factory.create(
        spark, charge_link_periods_factory.ChargeLinkPeriodsTestDataSpec()
    )
    charge_price_information_periods = charge_price_information_periods_factory.create(
        spark,
        charge_price_information_periods_factory.ChargePriceInformationPeriodsTestDataSpec(),
    )

    # expected_rows = DEFAULT_TO_DATE.day - DEFAULT_FROM_DATE.day
    # spec = time_series_factory.MeteringPointTimeSeriesTestDataSpec(
    #     from_date=DEFAULT_FROM_DATE, to_date=DEFAULT_TO_DATE, resolution=resolution
    # )
    charge_price_information_periods = charge_price_information_periods_factory.create(
        spark
    )
    mock_repository = Mock()
    mock_repository.read_metering_point_time_series.return_value = time_series_df
    mock_repository.read_charge_link_periods.return_value = charge_link_periods_df
    mock_repository.read_charge_price_information_periods.return_value = (
        charge_price_information_periods
    )

    # Act

    result_df = filter_by_charge_owner_on_metering_point(
        df=df,
        system_operator_id=system_operator_id,
        repository=mock_repository,
    )

    # Assert
    assert result_df.count() == expected_rows
