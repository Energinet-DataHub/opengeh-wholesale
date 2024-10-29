from dataclasses import dataclass
from datetime import datetime, timedelta
from decimal import Decimal

from pyspark.sql import SparkSession, DataFrame

from settlement_report_job.wholesale.data_values import (
    CalculationTypeDataProductValue,
    ChargeTypeDataProductValue,
    ChargeResolutionDataProductValue,
    MeteringPointResolutionDataProductValue,
    MeteringPointTypeDataProductValue,
    SettlementMethodDataProductValue,
)
from test_factories.default_test_data_spec import (
    create_energy_results_data_spec,
    create_amounts_per_charge_row,
)
from test_factories import (
    metering_point_periods_factory,
    metering_point_time_series_factory,
    charge_link_periods_factory,
    charge_price_information_periods_factory,
    energy_factory,
    amounts_per_charge_factory,
)

GRID_AREAS = ["804", "805"]
CALCULATION_ID = "12345678-6f20-40c5-9a95-f419a1245d7e"
CALCULATION_TYPE = CalculationTypeDataProductValue.WHOLESALE_FIXING
ENERGY_SUPPLIER_IDS = ["1000000000000", "2000000000000"]
FROM_DATE = datetime(2024, 1, 1, 23)
TO_DATE = FROM_DATE + timedelta(days=1)
"""TO_DATE is exclusive"""
# CHARGE_CODE = "4000"
# CHARGE_TYPE = ChargeTypeDataProductValue.TARIFF
CHARGE_OWNER_ID = "5790001330552"
# CHARGE_KEY = f"{CHARGE_CODE}_{CHARGE_TYPE}_{CHARGE_OWNER_ID}"
IS_TAX = False
METERING_POINT_TYPES = [
    MeteringPointTypeDataProductValue.CONSUMPTION,
    MeteringPointTypeDataProductValue.EXCHANGE,
]
RESULT_ID = "12345678-4e15-434c-9d93-b03a6dd272a5"
CALCULATION_PERIOD_START = FROM_DATE
CALCULATION_PERIOD_END = TO_DATE
QUANTITY_UNIT = "kwh"
QUANTITY_QUALITIES = ["measured"]
BALANCE_RESPONSIBLE_PARTY_ID = "1234567890123"


@dataclass
class MeteringPointSpec:
    metering_point_id: str
    metering_point_type: MeteringPointTypeDataProductValue
    grid_area_code: str
    energy_supplier_id: str
    resolution: MeteringPointResolutionDataProductValue


@dataclass
class Charge:
    charge_key: str
    charge_code: str
    charge_type: ChargeTypeDataProductValue
    charge_owner_id: str
    is_tax: bool


def create_metering_point_periods(spark: SparkSession) -> DataFrame:
    """
    Creates a DataFrame with metering point periods for testing purposes.
    """

    rows = []
    for metering_point in _get_all_metering_points():
        rows.append(
            metering_point_periods_factory.MeteringPointPeriodsRow(
                calculation_id=CALCULATION_ID,
                calculation_type=CALCULATION_TYPE,
                calculation_version=1,
                metering_point_id=metering_point.metering_point_id,
                metering_point_type=MeteringPointTypeDataProductValue.CONSUMPTION,
                settlement_method=SettlementMethodDataProductValue.FLEX,
                grid_area_code=metering_point.grid_area_code,
                resolution=metering_point.resolution,
                from_grid_area_code=None,
                parent_metering_point_id=None,
                energy_supplier_id=metering_point.energy_supplier_id,
                balance_responsible_party_id=BALANCE_RESPONSIBLE_PARTY_ID,
                from_date=FROM_DATE,
                to_date=TO_DATE,
            )
        )
    return metering_point_periods_factory.create(spark, rows)


def create_metering_point_time_series(spark: SparkSession) -> DataFrame:
    """
    Creates a DataFrame with metering point time series data for testing purposes.
    There is one row for each combination of resolution, grid area code, and energy supplier id.
    There is one calculation with two grid areas, and each grid area has two energy suppliers and each energy supplier
    has one metering point in the grid area
    """
    df = None
    for metering_point in _get_all_metering_points():
        data_spec = (
            metering_point_time_series_factory.MeteringPointTimeSeriesTestDataSpec(
                calculation_id=CALCULATION_ID,
                calculation_type=CALCULATION_TYPE,
                calculation_version=1,
                metering_point_id=metering_point.metering_point_id,
                metering_point_type=MeteringPointTypeDataProductValue.CONSUMPTION,
                resolution=metering_point.resolution,
                grid_area_code=metering_point.grid_area_code,
                energy_supplier_id=metering_point.energy_supplier_id,
                from_date=FROM_DATE,
                to_date=TO_DATE,
                quantity=Decimal("1.005"),
            )
        )
        next_df = metering_point_time_series_factory.create(spark, data_spec)
        if df is None:
            df = next_df
        else:
            df = df.union(next_df)

    return df


def create_charge_link_periods(spark: SparkSession) -> DataFrame:
    """
    Creates a DataFrame with charge link periods data for testing purposes.
    """

    rows = []

    for metering_point in _get_all_metering_points():
        for charge in _get_all_charges():
            rows.append(
                charge_link_periods_factory.ChargeLinkPeriodsRow(
                    calculation_id=CALCULATION_ID,
                    calculation_type=CALCULATION_TYPE,
                    calculation_version=1,
                    charge_key=charge.charge_key,
                    charge_code=charge.charge_code,
                    charge_type=charge.charge_type,
                    charge_owner_id=CHARGE_OWNER_ID,
                    metering_point_id=metering_point.metering_point_id,
                    quantity=1,
                    from_date=FROM_DATE,
                    to_date=TO_DATE,
                )
            )

    return charge_link_periods_factory.create(spark, rows)


def create_charge_price_information_periods(spark: SparkSession) -> DataFrame:
    """
    Creates a DataFrame with charge price information periods data for testing purposes.
    """
    rows = []
    for charge in _get_all_charges():
        rows.append(
            charge_price_information_periods_factory.ChargePriceInformationPeriodsRow(
                calculation_id=CALCULATION_ID,
                calculation_type=CALCULATION_TYPE,
                calculation_version=1,
                charge_key=charge.charge_key,
                charge_code=charge.charge_code,
                charge_type=charge.charge_type,
                charge_owner_id=CHARGE_OWNER_ID,
                is_tax=IS_TAX,
                resolution=ChargeResolutionDataProductValue.HOUR,
                from_date=FROM_DATE,
                to_date=TO_DATE,
            )
        )
    return charge_price_information_periods_factory.create(spark, rows)


def create_energy(spark: SparkSession) -> DataFrame:
    """
    Creates a DataFrame with energy data for testing purposes.
    Mimics the wholesale_results.energy_v1 view.
    """

    df = None
    for metering_point in _get_all_metering_points():
        data_spec = create_energy_results_data_spec(
            calculation_id=CALCULATION_ID,
            calculation_type=CALCULATION_TYPE,
            calculation_period_start=FROM_DATE,
            calculation_period_end=TO_DATE,
            grid_area_code=metering_point.grid_area_code,
            metering_point_type=metering_point.metering_point_type,
            resolution=metering_point.resolution,
            energy_supplier_id=metering_point.energy_supplier_id,
        )
        next_df = energy_factory.create_energy_v1(spark, data_spec)
        if df is None:
            df = next_df
        else:
            df = df.union(next_df)

    return df


def create_amounts_per_charge(spark: SparkSession) -> DataFrame:
    """
    Creates a DataFrame with amounts per charge data for testing purposes.
    Mimics the wholesale_results.amounts_per_charge_v1 view.
    """

    df = None
    for metering_point in _get_all_metering_points():
        row = create_amounts_per_charge_row(
            calculation_id=CALCULATION_ID,
            calculation_type=CALCULATION_TYPE,
            time=FROM_DATE,
            grid_area_code=metering_point.grid_area_code,
            metering_point_type=metering_point.metering_point_type,
            resolution=ChargeResolutionDataProductValue.HOUR,
            energy_supplier_id=metering_point.energy_supplier_id,
        )
        next_df = amounts_per_charge_factory.create(spark, row)
        if df is None:
            df = next_df
        else:
            df = df.union(next_df)

    return df


def create_energy_per_es(spark: SparkSession) -> DataFrame:
    """
    Creates a DataFrame with energy data for testing purposes.
    Mimics the wholesale_results.energy_v1 view.
    """

    df = None
    for metering_point in _get_all_metering_points():
        data_spec = create_energy_results_data_spec(
            calculation_id=CALCULATION_ID,
            calculation_type=CALCULATION_TYPE,
            calculation_period_start=FROM_DATE,
            calculation_period_end=TO_DATE,
            grid_area_code=metering_point.grid_area_code,
            metering_point_type=metering_point.metering_point_type,
            resolution=metering_point.resolution,
            energy_supplier_id=metering_point.energy_supplier_id,
        )
        next_df = energy_factory.create_energy_per_es_v1(spark, data_spec)
        if df is None:
            df = next_df
        else:
            df = df.union(next_df)

    return df


def _get_all_metering_points() -> list[MeteringPointSpec]:
    metering_points = []
    count = 0
    for resolution in {
        MeteringPointResolutionDataProductValue.HOUR,
        MeteringPointResolutionDataProductValue.QUARTER,
    }:
        for grid_area_code in GRID_AREAS:
            for energy_supplier_id in ENERGY_SUPPLIER_IDS:
                for metering_point_type in METERING_POINT_TYPES:
                    metering_points.append(
                        MeteringPointSpec(
                            metering_point_id=str(1000000000000 + count),
                            metering_point_type=metering_point_type,
                            grid_area_code=grid_area_code,
                            energy_supplier_id=energy_supplier_id,
                            resolution=resolution,
                        )
                    )

    return metering_points


def _get_all_charges() -> list[Charge]:

    return [
        Charge(
            charge_key=f"4000_{ChargeTypeDataProductValue.TARIFF.value}_5790001330552",
            charge_code="4000",
            charge_type=ChargeTypeDataProductValue.TARIFF,
            charge_owner_id="5790001330552",
            is_tax=False,
        ),
        Charge(
            charge_key=f"4001_{ChargeTypeDataProductValue.TARIFF.value}_5790001330553",
            charge_code="4001",
            charge_type=ChargeTypeDataProductValue.TARIFF,
            charge_owner_id="5790001330553",
            is_tax=True,
        ),
    ]
