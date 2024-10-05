from datetime import datetime, timedelta

import pytest
from pyspark.sql import SparkSession, DataFrame
import test_factories.charge_link_periods_factory as charge_link_periods_factory
import test_factories.charge_price_information_periods_factory as charge_price_information_periods_factory
from settlement_report_job.infrastructure.column_names import DataProductColumnNames
from test_factories.charge_price_information_periods_factory import (
    ChargePriceInformationPeriodsTestDataSpec,
)
from test_factories.charge_link_periods_factory import ChargeLinkPeriodsTestDataSpec

from settlement_report_job.domain.DataProductValues.charge_resolution import (
    ChargeResolution,
)
from settlement_report_job.domain.DataProductValues.charge_type import ChargeType
from settlement_report_job.domain.DataProductValues.metering_point_type import (
    MeteringPointType,
)
from settlement_report_job.domain.calculation_type import CalculationType

from settlement_report_job.domain.system_operator_filter import (
    filter_time_series_on_charge_owner,
)

DEFAULT_TIME_ZONE = "Europe/Copenhagen"
DEFAULT_FROM_DATE = datetime(2024, 1, 1, 23)
DEFAULT_TO_DATE = DEFAULT_FROM_DATE + timedelta(days=1)
DATAHUB_ADMINISTRATOR_ID = "1234567890123"
DEFAULT_PERIOD_START = datetime(2024, 1, 1, 22)
DEFAULT_PERIOD_END = datetime(2024, 1, 2, 22)
DEFAULT_CALCULATION_ID = "11111111-1111-1111-1111-111111111111"
DEFAULT_CALCULATION_VERSION = 1
DEFAULT_METERING_POINT_ID = "12345678-1111-1111-1111-111111111111"
DEFAULT_METERING_TYPE = MeteringPointType.CONSUMPTION
DEFAULT_GRID_AREA_CODE = "804"
DEFAULT_ENERGY_SUPPLIER_ID = "1234567890123"
DEFAULT_CHARGE_CODE = "41000"
DEFAULT_CHARGE_TYPE = ChargeType.TARIFF
DEFAULT_CHARGE_OWNER_ID = "3333333333333"
DEFAULT_CHARGE_KEY = "41000-tariff-3333333333333"


@pytest.fixture
def default_charge_link_periods_test_data_spec() -> (
    charge_link_periods_factory.ChargeLinkPeriodsTestDataSpec
):
    return ChargeLinkPeriodsTestDataSpec(
        calculation_id=DEFAULT_CALCULATION_ID,
        calculation_type=CalculationType.WHOLESALE_FIXING,
        calculation_version=DEFAULT_CALCULATION_VERSION,
        charge_key=DEFAULT_CHARGE_KEY,
        charge_code=DEFAULT_CHARGE_CODE,
        charge_type=DEFAULT_CHARGE_TYPE,
        charge_owner_id=DEFAULT_CHARGE_OWNER_ID,
        metering_point_id=DEFAULT_METERING_POINT_ID,
        from_date=DEFAULT_PERIOD_START,
        to_date=DEFAULT_PERIOD_END,
        quantity=1,
    )


@pytest.fixture
def default_charge_price_information_periods_test_data_spec() -> (
    ChargePriceInformationPeriodsTestDataSpec
):
    return ChargePriceInformationPeriodsTestDataSpec(
        calculation_id=DEFAULT_CALCULATION_ID,
        calculation_type=CalculationType.WHOLESALE_FIXING,
        calculation_version=DEFAULT_CALCULATION_VERSION,
        charge_key=DEFAULT_CHARGE_KEY,
        charge_code=DEFAULT_CHARGE_CODE,
        charge_type=DEFAULT_CHARGE_TYPE,
        charge_owner_id=DEFAULT_CHARGE_OWNER_ID,
        resolution=ChargeResolution.HOUR,
        is_tax=False,
        from_date=DEFAULT_PERIOD_START,
        to_date=DEFAULT_PERIOD_END,
    )


def _create_data_frame_with_metering_point_id(
    spark: SparkSession, calculation_id: str, metering_point_ids: str | list[str]
) -> DataFrame:
    if not isinstance(metering_point_ids, list):
        metering_point_ids = [metering_point_ids]

    return spark.createDataFrame(
        [(calculation_id, mp_id, i) for i, mp_id in enumerate(metering_point_ids)],
        [
            DataProductColumnNames.calculation_id,
            DataProductColumnNames.metering_point_id,
            "other_column",
        ],
    )


def test_(
    spark: SparkSession,
    default_charge_price_information_periods_test_data_spec: ChargePriceInformationPeriodsTestDataSpec,
    default_charge_link_periods_test_data_spec: ChargeLinkPeriodsTestDataSpec,
) -> None:
    # Arrange
    df_with_metering_point_id = _create_data_frame_with_metering_point_id(
        spark, DEFAULT_CALCULATION_ID, DEFAULT_METERING_POINT_ID
    )

    charge_link_periods_df = charge_link_periods_factory.create(
        spark, default_charge_link_periods_test_data_spec
    )
    charge_price_information_periods_df = (
        charge_price_information_periods_factory.create(
            spark, default_charge_price_information_periods_test_data_spec
        )
    )

    # Act
    actual = filter_time_series_on_charge_owner(
        df=df_with_metering_point_id,
        system_operator_id=DEFAULT_CHARGE_OWNER_ID,
        charge_link_periods=charge_link_periods_df,
        charge_price_information_periods=charge_price_information_periods_df,
    )

    # Assert
    assert actual.count() == 1
