from datetime import datetime, timedelta
from decimal import Decimal

from settlement_report_job.infrastructure.wholesale.data_values import (
    MeteringPointTypeDataProductValue,
    ChargeTypeDataProductValue,
    ChargeResolutionDataProductValue,
    MeteringPointResolutionDataProductValue,
)
from settlement_report_job.infrastructure.wholesale.data_values.calculation_type import (
    CalculationTypeDataProductValue,
)
from settlement_report_job.infrastructure.wholesale.data_values.settlement_method import (
    SettlementMethodDataProductValue,
)
from test_factories.charge_price_information_periods_factory import (
    ChargePriceInformationPeriodsRow,
)
from test_factories.latest_calculations_factory import LatestCalculationsPerDayRow
from test_factories.metering_point_periods_factory import MeteringPointPeriodsRow
from test_factories.metering_point_time_series_factory import (
    MeteringPointTimeSeriesTestDataSpec,
)

from test_factories.charge_price_points_factory import ChargePricePointsRow
from test_factories.monthly_amounts_per_charge_factory import MonthlyAmountsPerChargeRow
from test_factories.total_monthly_amounts_factory import TotalMonthlyAmountsRow
from test_factories.charge_link_periods_factory import ChargeLinkPeriodsRow
from test_factories.energy_factory import EnergyTestDataSpec
from test_factories.amounts_per_charge_factory import AmountsPerChargeRow

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
DEFAULT_FROM_GRID_AREA_CODE = None
DEFAULT_TO_GRID_AREA_CODE = None
DEFAULT_ENERGY_SUPPLIER_ID = "1234567890123"
DEFAULT_CHARGE_CODE = "41000"
DEFAULT_CHARGE_TYPE = ChargeTypeDataProductValue.TARIFF
DEFAULT_CHARGE_OWNER_ID = "3333333333333"
DEFAULT_CHARGE_PRICE = Decimal("10.000")

# For energy results
DEFAULT_RESULT_ID = "12345678-4e15-434c-9d93-b03a6dd272a5"
DEFAULT_SETTLEMENT_METHOD = None
DEFAULT_QUANTITY_UNIT = "kwh"
DEFAULT_QUANTITY_QUALITIES = ["measured"]
DEFAULT_BALANCE_RESPONSIBLE_PARTY_ID = "1234567890123"


def create_charge_link_periods_row(
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
) -> ChargeLinkPeriodsRow:
    charge_key = f"{charge_code}-{charge_type}-{charge_owner_id}"
    return ChargeLinkPeriodsRow(
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


def create_charge_price_points_row(
    calculation_id: str = DEFAULT_CALCULATION_ID,
    calculation_type: CalculationTypeDataProductValue = CalculationTypeDataProductValue.WHOLESALE_FIXING,
    calculation_version: int = DEFAULT_CALCULATION_VERSION,
    charge_code: str = DEFAULT_CHARGE_CODE,
    charge_type: ChargeTypeDataProductValue = DEFAULT_CHARGE_TYPE,
    charge_owner_id: str = DEFAULT_CHARGE_OWNER_ID,
    charge_price: Decimal = DEFAULT_CHARGE_PRICE,
    charge_time: datetime = DEFAULT_PERIOD_START,
) -> ChargePricePointsRow:
    charge_key = f"{charge_code}-{charge_type}-{charge_owner_id}"
    return ChargePricePointsRow(
        calculation_id=calculation_id,
        calculation_type=calculation_type,
        calculation_version=calculation_version,
        charge_key=charge_key,
        charge_code=charge_code,
        charge_type=charge_type,
        charge_owner_id=charge_owner_id,
        charge_price=charge_price,
        charge_time=charge_time,
    )


def create_charge_price_information_periods_row(
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
) -> ChargePriceInformationPeriodsRow:
    charge_key = f"{charge_code}-{charge_type}-{charge_owner_id}"

    return ChargePriceInformationPeriodsRow(
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


def create_metering_point_periods_row(
    calculation_id: str = DEFAULT_CALCULATION_ID,
    calculation_type: CalculationTypeDataProductValue = CalculationTypeDataProductValue.WHOLESALE_FIXING,
    calculation_version: int = DEFAULT_CALCULATION_VERSION,
    metering_point_id: str = DEFAULT_METERING_POINT_ID,
    metering_point_type: MeteringPointTypeDataProductValue = DEFAULT_METERING_TYPE,
    settlement_method: SettlementMethodDataProductValue = DEFAULT_SETTLEMENT_METHOD,
    grid_area_code: str = DEFAULT_GRID_AREA_CODE,
    resolution: MeteringPointResolutionDataProductValue = DEFAULT_RESOLUTION,
    from_grid_area_code: str = DEFAULT_FROM_GRID_AREA_CODE,
    to_grid_area_code: str = DEFAULT_TO_GRID_AREA_CODE,
    parent_metering_point_id: str | None = None,
    energy_supplier_id: str = DEFAULT_ENERGY_SUPPLIER_ID,
    balance_responsible_party_id: str = DEFAULT_BALANCE_RESPONSIBLE_PARTY_ID,
    from_date: datetime = DEFAULT_PERIOD_START,
    to_date: datetime = DEFAULT_PERIOD_END,
) -> MeteringPointPeriodsRow:
    return MeteringPointPeriodsRow(
        calculation_id=calculation_id,
        calculation_type=calculation_type,
        calculation_version=calculation_version,
        metering_point_id=metering_point_id,
        metering_point_type=metering_point_type,
        settlement_method=settlement_method,
        grid_area_code=grid_area_code,
        resolution=resolution,
        from_grid_area_code=from_grid_area_code,
        to_grid_area_code=to_grid_area_code,
        parent_metering_point_id=parent_metering_point_id,
        energy_supplier_id=energy_supplier_id,
        balance_responsible_party_id=balance_responsible_party_id,
        from_date=from_date,
        to_date=to_date,
    )


def create_time_series_points_data_spec(
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


def create_amounts_per_charge_row(
    calculation_id: str = DEFAULT_CALCULATION_ID,
    calculation_type: CalculationTypeDataProductValue = CalculationTypeDataProductValue.WHOLESALE_FIXING,
    calculation_version: int = DEFAULT_CALCULATION_VERSION,
    grid_area_code: str = DEFAULT_GRID_AREA_CODE,
    energy_supplier_id: str = DEFAULT_ENERGY_SUPPLIER_ID,
    charge_code: str = DEFAULT_CHARGE_CODE,
    charge_type: ChargeTypeDataProductValue = DEFAULT_CHARGE_TYPE,
    charge_owner_id: str = DEFAULT_CHARGE_OWNER_ID,
    resolution: ChargeResolutionDataProductValue = ChargeResolutionDataProductValue.HOUR,
    quantity_unit: str = "kWh",
    metering_point_type: MeteringPointTypeDataProductValue = DEFAULT_METERING_TYPE,
    settlement_method: SettlementMethodDataProductValue = DEFAULT_SETTLEMENT_METHOD,
    is_tax: bool = False,
    currency: str = "DKK",
    time: datetime = DEFAULT_PERIOD_START,
    quantity: Decimal = Decimal("1.005"),
    quantity_qualities: list[str] = ["measured"],
    price: Decimal = Decimal("0.005"),
    amount: Decimal = Decimal("0.005"),
) -> AmountsPerChargeRow:
    return AmountsPerChargeRow(
        calculation_id=calculation_id,
        calculation_type=calculation_type,
        calculation_version=calculation_version,
        result_id="result_id_placeholder",  # Add appropriate value
        grid_area_code=grid_area_code,
        energy_supplier_id=energy_supplier_id,
        charge_code=charge_code,
        charge_type=charge_type,
        charge_owner_id=charge_owner_id,
        resolution=resolution,
        quantity_unit=quantity_unit,
        metering_point_type=metering_point_type,
        settlement_method=settlement_method,
        is_tax=is_tax,
        currency=currency,
        time=time,
        quantity=quantity,
        quantity_qualities=quantity_qualities,
        price=price,
        amount=amount,
    )


def create_monthly_amounts_per_charge_row(
    calculation_id: str = DEFAULT_CALCULATION_ID,
    calculation_type: CalculationTypeDataProductValue = CalculationTypeDataProductValue.WHOLESALE_FIXING,
    calculation_version: int = DEFAULT_CALCULATION_VERSION,
    grid_area_code: str = DEFAULT_GRID_AREA_CODE,
    energy_supplier_id: str = DEFAULT_ENERGY_SUPPLIER_ID,
    charge_code: str = DEFAULT_CHARGE_CODE,
    charge_type: ChargeTypeDataProductValue = DEFAULT_CHARGE_TYPE,
    charge_owner_id: str = DEFAULT_CHARGE_OWNER_ID,
    quantity_unit: str = "kWh",
    is_tax: bool = False,
    currency: str = "DKK",
    time: datetime = DEFAULT_PERIOD_START,
    amount: Decimal = Decimal("0.005"),
) -> MonthlyAmountsPerChargeRow:
    return MonthlyAmountsPerChargeRow(
        calculation_id=calculation_id,
        calculation_type=calculation_type,
        calculation_version=calculation_version,
        result_id="result_id_placeholder",  # Add appropriate value
        grid_area_code=grid_area_code,
        energy_supplier_id=energy_supplier_id,
        charge_code=charge_code,
        charge_type=charge_type,
        charge_owner_id=charge_owner_id,
        quantity_unit=quantity_unit,
        is_tax=is_tax,
        currency=currency,
        time=time,
        amount=amount,
    )


def create_total_monthly_amounts_row(
    calculation_id: str = DEFAULT_CALCULATION_ID,
    calculation_type: CalculationTypeDataProductValue = CalculationTypeDataProductValue.WHOLESALE_FIXING,
    calculation_version: int = DEFAULT_CALCULATION_VERSION,
    grid_area_code: str = DEFAULT_GRID_AREA_CODE,
    energy_supplier_id: str = DEFAULT_ENERGY_SUPPLIER_ID,
    charge_owner_id: str = DEFAULT_CHARGE_OWNER_ID,
    currency: str = "DKK",
    time: datetime = DEFAULT_PERIOD_START,
    amount: Decimal = Decimal("0.005"),
) -> TotalMonthlyAmountsRow:
    return TotalMonthlyAmountsRow(
        calculation_id=calculation_id,
        calculation_type=calculation_type,
        calculation_version=calculation_version,
        result_id="result_id_placeholder",  # Add appropriate value
        grid_area_code=grid_area_code,
        energy_supplier_id=energy_supplier_id,
        charge_owner_id=charge_owner_id,
        currency=currency,
        time=time,
        amount=amount,
    )


def create_latest_calculations_per_day_row(
    calculation_id: str = DEFAULT_CALCULATION_ID,
    calculation_type: CalculationTypeDataProductValue = CalculationTypeDataProductValue.BALANCE_FIXING,
    calculation_version: int = DEFAULT_CALCULATION_VERSION,
    grid_area_code: str = DEFAULT_GRID_AREA_CODE,
    start_of_day: datetime = DEFAULT_PERIOD_START,
) -> LatestCalculationsPerDayRow:

    return LatestCalculationsPerDayRow(
        calculation_id=calculation_id,
        calculation_type=calculation_type,
        calculation_version=calculation_version,
        grid_area_code=grid_area_code,
        start_of_day=start_of_day,
    )


def create_energy_results_data_spec(
    calculation_id: str = DEFAULT_CALCULATION_ID,
    calculation_type: CalculationTypeDataProductValue = CalculationTypeDataProductValue.WHOLESALE_FIXING,
    calculation_period_start: datetime = DEFAULT_PERIOD_START,
    calculation_period_end: datetime = DEFAULT_PERIOD_END,
    calculation_version: int = DEFAULT_CALCULATION_VERSION,
    result_id: str = DEFAULT_RESULT_ID,
    grid_area_code: str = DEFAULT_GRID_AREA_CODE,
    metering_point_type: MeteringPointTypeDataProductValue = DEFAULT_METERING_TYPE,
    settlement_method: str = DEFAULT_SETTLEMENT_METHOD,
    resolution: MeteringPointResolutionDataProductValue = DEFAULT_RESOLUTION,
    quantity: Decimal = Decimal("1.005"),
    quantity_unit: str = DEFAULT_QUANTITY_UNIT,
    quantity_qualities: list[str] = DEFAULT_QUANTITY_QUALITIES,
    from_date: datetime = DEFAULT_PERIOD_START,
    to_date: datetime = DEFAULT_PERIOD_END,
    energy_supplier_id: str = DEFAULT_ENERGY_SUPPLIER_ID,
    balance_responsible_party_id: str = DEFAULT_BALANCE_RESPONSIBLE_PARTY_ID,
) -> EnergyTestDataSpec:
    return EnergyTestDataSpec(
        calculation_id=calculation_id,
        calculation_type=calculation_type,
        calculation_period_start=calculation_period_start,
        calculation_period_end=calculation_period_end,
        calculation_version=calculation_version,
        result_id=result_id,
        grid_area_code=grid_area_code,
        metering_point_type=metering_point_type,
        settlement_method=settlement_method,
        resolution=resolution,
        quantity=quantity,
        quantity_unit=quantity_unit,
        quantity_qualities=quantity_qualities,
        from_date=from_date,
        to_date=to_date,
        energy_supplier_id=energy_supplier_id,
        balance_responsible_party_id=balance_responsible_party_id,
    )
