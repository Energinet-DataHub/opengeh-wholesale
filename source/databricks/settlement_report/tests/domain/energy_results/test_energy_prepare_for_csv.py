from datetime import datetime
from decimal import Decimal

import pytest
from pyspark.sql import SparkSession, DataFrame
from pyspark.sql.functions import monotonically_increasing_id
import pyspark.sql.functions as F
from pyspark.sql.types import DecimalType

import test_factories.default_test_data_spec as default_data
import test_factories.metering_point_time_series_factory as time_series_factory
from settlement_report_job.domain.market_role import MarketRole

from settlement_report_job.domain.time_series.prepare_for_csv import prepare_for_csv
from settlement_report_job.domain.csv_column_names import (
    CsvColumnNames,
)
from settlement_report_job.wholesale.column_names import DataProductColumnNames
from settlement_report_job.wholesale.data_values import (
    MeteringPointResolutionDataProductValue,
    MeteringPointTypeDataProductValue,
)

#
# DEFAULT_TIME_ZONE = "Europe/Copenhagen"
# DEFAULT_FROM_DATE = default_data.DEFAULT_FROM_DATE
# DEFAULT_TO_DATE = default_data.DEFAULT_TO_DATE
# DATAHUB_ADMINISTRATOR_ID = "1234567890123"
# SYSTEM_OPERATOR_ID = "3333333333333"
# NOT_SYSTEM_OPERATOR_ID = "4444444444444"
# DEFAULT_MARKET_ROLE = MarketRole.GRID_ACCESS_PROVIDER


def _create_time_series_with_many_combinations(
    spark: SparkSession,
    from_date: datetime,
    to_date: datetime,
    resolution: MeteringPointResolutionDataProductValue,
) -> DataFrame:

    df = None
    for grid_area_code in ["804", "805"]:
        for mp_type in [
            MeteringPointTypeDataProductValue.CONSUMPTION,
            MeteringPointTypeDataProductValue.PRODUCTION,
        ]:
            for mp_id in ["1", "2"]:
                for energy_supplier_id in ["1000000000000", "2000000000000"]:
                    spec = default_data.create_energy_results_data_spec(
                        from_date=from_date,
                        to_date=to_date,
                        grid_area_code=grid_area_code,
                        metering_point_type=mp_type,
                        metering_point_id=mp_id,
                        energy_supplier_id=energy_supplier_id,
                        resolution=resolution,
                    )
                    current_df = time_series_factory.create(spark, spec)
                    if df is None:
                        df = current_df
                    else:
                        df = df.union(current_df)

    return df


EXPECTED_ORDERING_FOR_DATAHUB_ADMINISTRATOR_AND_SYSTEM_OPERATOR = [
    CsvColumnNames.energy_supplier_id,
    CsvColumnNames.type_of_mp,
    CsvColumnNames.metering_point_id,
    CsvColumnNames.start_date_time,
]

EXPECTED_ORDERING_FOR_ENERGY_SUPPLIER_AND_GRID_ACCESS_PROVIDER = [
    CsvColumnNames.type_of_mp,
    CsvColumnNames.metering_point_id,
    CsvColumnNames.start_date_time,
]


@pytest.mark.parametrize(
    "requesting_actor_market_role,expected_order",
    [
        (
            MarketRole.DATAHUB_ADMINISTRATOR,
            EXPECTED_ORDERING_FOR_DATAHUB_ADMINISTRATOR_AND_SYSTEM_OPERATOR,
        ),
        (
            MarketRole.SYSTEM_OPERATOR,
            EXPECTED_ORDERING_FOR_DATAHUB_ADMINISTRATOR_AND_SYSTEM_OPERATOR,
        ),
        (
            MarketRole.ENERGY_SUPPLIER,
            EXPECTED_ORDERING_FOR_ENERGY_SUPPLIER_AND_GRID_ACCESS_PROVIDER,
        ),
        (
            MarketRole.GRID_ACCESS_PROVIDER,
            EXPECTED_ORDERING_FOR_ENERGY_SUPPLIER_AND_GRID_ACCESS_PROVIDER,
        ),
    ],
)
def test_prepare_for_csv__when_daylight_saving_tim_transition__returns_dataframe_with_expected_ordering(
    spark: SparkSession,
    requesting_actor_market_role: MarketRole,
    expected_order: list[str],
) -> None:
    # Arrange
    from_date = datetime(2023, 3, 10, 22)
    to_date = datetime(2023, 3, 12, 22)
    df = _create_time_series_with_many_combinations(
        spark, from_date, to_date, MeteringPointResolutionDataProductValue.HOUR
    ).orderBy(F.rand())

    # Act
    actual_df = prepare_for_csv(
        filtered_time_series_points=df,
        metering_point_resolution=MeteringPointResolutionDataProductValue.HOUR,
        time_zone=DEFAULT_TIME_ZONE,
        requesting_actor_market_role=requesting_actor_market_role,
    )

    # Assert
    assert actual_df.collect() == actual_df.orderBy(expected_order).collect()
