from datetime import datetime, timedelta
from decimal import Decimal

import pytest
from pyspark.sql import SparkSession
import test_factories.metering_point_time_series_factory as time_series_factory
import test_factories.charge_link_periods_factory as charge_link_periods_factory
import test_factories.charge_price_information_periods_factory as charge_price_information_periods_factory
from package.codelists import MeteringPointResolution
from settlement_report_job.domain.metering_point_resolution import (
    DataProductMeteringPointResolution,
)
from test_factories.charge_price_information_periods_factory import (
    ChargePriceInformationPeriodsTestDataSpec,
)
from test_factories.charge_link_periods_factory import ChargeLinkPeriodsTestDataSpec
from test_factories.metering_point_time_series_factory import (
    MeteringPointTimeSeriesTestDataSpec,
)

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
DEFAULT_PERIOD_START = DEFAULT_FROM_DATE
DEFAULT_PERIOD_END = DEFAULT_TO_DATE
DEFAULT_CALCULATION_ID = "11111111-1111-1111-1111-111111111111"
DEFAULT_CALCULATION_VERSION = 1
DEFAULT_METERING_POINT_ID = "3456789012345"
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


@pytest.fixture
def default_time_series_test_data_spec() -> MeteringPointTimeSeriesTestDataSpec:
    return MeteringPointTimeSeriesTestDataSpec(
        calculation_id=DEFAULT_CALCULATION_ID,
        calculation_type=CalculationType.WHOLESALE_FIXING,
        calculation_version=DEFAULT_CALCULATION_VERSION,
        metering_point_id=DEFAULT_METERING_POINT_ID,
        metering_point_type=DEFAULT_METERING_TYPE,
        resolution=DataProductMeteringPointResolution.HOUR,
        grid_area_code=DEFAULT_GRID_AREA_CODE,
        energy_supplier_id=DEFAULT_ENERGY_SUPPLIER_ID,
        from_date=DEFAULT_PERIOD_START,
        to_date=DEFAULT_PERIOD_END,
        quantity=Decimal("1.005"),
    )


def test_(
    spark: SparkSession,
    default_time_series_test_data_spec: MeteringPointTimeSeriesTestDataSpec,
    default_charge_price_information_periods_test_data_spec: ChargePriceInformationPeriodsTestDataSpec,
    default_charge_link_periods_test_data_spec: ChargeLinkPeriodsTestDataSpec,
) -> None:
    # Arrange
    time_series_df = time_series_factory.create(
        spark, default_time_series_test_data_spec
    )
    charge_link_periods_df = charge_link_periods_factory.create(
        spark, default_charge_link_periods_test_data_spec
    )
    charge_price_information_periods_df = (
        charge_price_information_periods_factory.create(
            spark, default_charge_price_information_periods_test_data_spec
        )
    )

    charge_link_periods_df.show()
    charge_price_information_periods_df.show()
    time_series_df.show()

    # Act
    actual = filter_time_series_on_charge_owner(
        df=time_series_df,
        system_operator_id=DEFAULT_CHARGE_OWNER_ID,
        charge_link_periods=charge_link_periods_df,
        charge_price_information_periods=charge_price_information_periods_df,
    )

    # Assert
    assert actual.count() == 1
