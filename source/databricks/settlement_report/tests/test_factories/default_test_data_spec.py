from datetime import datetime, timedelta
from decimal import Decimal

from settlement_report_job.infrastructure.calculation_type import CalculationType
from settlement_report_job.wholesale.data_values import (
    MeteringPointTypeDataProductValue,
    ChargeTypeDataProductValue,
    ChargeResolutionDataProductValue,
    MeteringPointResolutionDataProductValue,
)
from settlement_report_job.wholesale.data_values.calculation_type import (
    CalculationTypeDataProductValue,
)
from test_factories.charge_link_periods_factory import ChargeLinkPeriodsTestDataSpec
from test_factories.charge_price_information_periods_factory import (
    ChargePriceInformationPeriodsTestDataSpec,
)
from test_factories.latest_calculations_factory import LatestCalculationsTestDataSpec
from test_factories.metering_point_time_series_factory import (
    MeteringPointTimeSeriesTestDataSpec,
)

DEFAULT_FROM_DATE = datetime(2024, 1, 1, 23)
DEFAULT_TO_DATE = DEFAULT_FROM_DATE + timedelta(days=1)
DATAHUB_ADMINISTRATOR_ID = "1234567890123"
DEFAULT_PERIOD_START = DEFAULT_FROM_DATE
DEFAULT_PERIOD_END = DEFAULT_TO_DATE
DEFAULT_CALCULATION_ID = "11111111-1111-1111-1111-111111111111"
DEFAULT_CALCULATION_VERSION = 1
DEFAULT_METERING_POINT_ID = "3456789012345"
DEFAULT_METERING_TYPE = MeteringPointTypeDataProductValue.CONSUMPTION
DEFAULT_RESOLUTION = MeteringPointResolutionDataProductValue.HOUR
DEFAULT_GRID_AREA_CODE = "804"
DEFAULT_ENERGY_SUPPLIER_ID = "1234567890123"
DEFAULT_CHARGE_CODE = "41000"
DEFAULT_CHARGE_TYPE = ChargeTypeDataProductValue.TARIFF
DEFAULT_CHARGE_OWNER_ID = "3333333333333"


def create_charge_link_periods_data_spec(
    calculation_id: str = DEFAULT_CALCULATION_ID,
    calculation_type: CalculationTypeDataProductValue = CalculationTypeDataProductValue.WHOLESALE_FIXING,
    calculation_version: int = DEFAULT_CALCULATION_VERSION,
    charge_code: str = DEFAULT_CHARGE_CODE,
    charge_type: ChargeTypeDataProductValue = DEFAULT_CHARGE_TYPE,
    charge_owner_id: str = DEFAULT_CHARGE_OWNER_ID,
    metering_point_id: str = DEFAULT_METERING_POINT_ID,
    from_date: datetime = DEFAULT_PERIOD_START,
    to_date: datetime = DEFAULT_PERIOD_END,
    quantity: int = 1,
) -> ChargeLinkPeriodsTestDataSpec:
    charge_key = f"{charge_code}-{charge_type}-{charge_owner_id}"
    return ChargeLinkPeriodsTestDataSpec(
        calculation_id=calculation_id,
        calculation_type=calculation_type,
        calculation_version=calculation_version,
        charge_key=charge_key,
        charge_code=charge_code,
        charge_type=charge_type,
        charge_owner_id=charge_owner_id,
        metering_point_id=metering_point_id,
        from_date=from_date,
        to_date=to_date,
        quantity=quantity,
    )


def create_charge_price_information_periods_data_spec(
    calculation_id: str = DEFAULT_CALCULATION_ID,
    calculation_type: CalculationTypeDataProductValue = CalculationTypeDataProductValue.WHOLESALE_FIXING,
    calculation_version: int = DEFAULT_CALCULATION_VERSION,
    charge_code: str = DEFAULT_CHARGE_CODE,
    charge_type: ChargeTypeDataProductValue = DEFAULT_CHARGE_TYPE,
    charge_owner_id: str = DEFAULT_CHARGE_OWNER_ID,
    resolution: ChargeResolutionDataProductValue = ChargeResolutionDataProductValue.HOUR,
    is_tax: bool = False,
    from_date: datetime = DEFAULT_PERIOD_START,
    to_date: datetime = DEFAULT_PERIOD_END,
) -> ChargePriceInformationPeriodsTestDataSpec:
    charge_key = f"{charge_code}-{charge_type}-{charge_owner_id}"

    return ChargePriceInformationPeriodsTestDataSpec(
        calculation_id=calculation_id,
        calculation_type=calculation_type,
        calculation_version=calculation_version,
        charge_key=charge_key,
        charge_code=charge_code,
        charge_type=charge_type,
        charge_owner_id=charge_owner_id,
        resolution=resolution,
        is_tax=is_tax,
        from_date=from_date,
        to_date=to_date,
    )


def create_time_series_data_spec(
    calculation_id: str = DEFAULT_CALCULATION_ID,
    calculation_type: CalculationTypeDataProductValue = CalculationTypeDataProductValue.WHOLESALE_FIXING,
    calculation_version: int = DEFAULT_CALCULATION_VERSION,
    metering_point_id: str = DEFAULT_METERING_POINT_ID,
    metering_point_type: MeteringPointTypeDataProductValue = DEFAULT_METERING_TYPE,
    resolution: MeteringPointResolutionDataProductValue = DEFAULT_RESOLUTION,
    grid_area_code: str = DEFAULT_GRID_AREA_CODE,
    energy_supplier_id: str = DEFAULT_ENERGY_SUPPLIER_ID,
    from_date: datetime = DEFAULT_PERIOD_START,
    to_date: datetime = DEFAULT_PERIOD_END,
    quantity: Decimal = Decimal("1.005"),
) -> MeteringPointTimeSeriesTestDataSpec:
    return MeteringPointTimeSeriesTestDataSpec(
        calculation_id=calculation_id,
        calculation_type=calculation_type,
        calculation_version=calculation_version,
        metering_point_id=metering_point_id,
        metering_point_type=metering_point_type,
        resolution=resolution,
        grid_area_code=grid_area_code,
        energy_supplier_id=energy_supplier_id,
        from_date=from_date,
        to_date=to_date,
        quantity=quantity,
    )


def create_latest_calculations_data_spec(
    calculation_id: str = DEFAULT_CALCULATION_ID,
    calculation_type: CalculationTypeDataProductValue = CalculationTypeDataProductValue.WHOLESALE_FIXING,
    calculation_version: int = DEFAULT_CALCULATION_VERSION,
    grid_area_code: str = DEFAULT_GRID_AREA_CODE,
    start_of_day: datetime = DEFAULT_PERIOD_START,
) -> LatestCalculationsTestDataSpec:

    return LatestCalculationsTestDataSpec(
        calculation_id=calculation_id,
        calculation_type=calculation_type,
        calculation_version=calculation_version,
        grid_area_code=grid_area_code,
        start_of_day=start_of_day,
    )
