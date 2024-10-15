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
)
from test_factories import (
    metering_point_time_series_factory,
    charge_link_periods_factory,
    charge_price_information_periods_factory,
    latest_calculations_factory,
    energy_factory,
)

GRID_AREAS = ["804", "805"]
CALCULATION_ID = "12345678-6f20-40c5-9a95-f419a1245d7e"
CALCULATION_TYPE = CalculationTypeDataProductValue.WHOLESALE_FIXING
ENERGY_SUPPLIER_IDS = ["1000000000000", "2000000000000"]
FROM_DATE = datetime(2024, 1, 1, 23)
TO_DATE = FROM_DATE + timedelta(days=1)
"""TO_DATE is exclusive"""
CHARGE_CODE = "4000"
CHARGE_TYPE = ChargeTypeDataProductValue.TARIFF
CHARGE_OWNER_ID = "5790001330552"
CHARGE_KEY = f"{CHARGE_CODE}_{CHARGE_TYPE}_{CHARGE_OWNER_ID}"
IS_TAX = False
METERING_POINT_TYPES = ["consumption", "exchange"]  # TODO: Add the rest?
RESULT_ID = "12345678-4e15-434c-9d93-b03a6dd272a5"
CALCULATION_PERIOD_START = FROM_DATE
CALCULATION_PERIOD_END = TO_DATE
QUANTITY_UNIT = "kwh"
QUANTITY_QUALITIES = ["measured"]
BALANCE_RESPONSIBLE_PARTY_ID = "1234567890123"


@dataclass
class MeteringPointSpec:
    metering_point_id: str
    metering_point_type: str
    grid_area_code: str
    energy_supplier_id: str
    resolution: MeteringPointResolutionDataProductValue


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

    df = None

    for metering_point in _get_all_metering_points():
        data_spec = charge_link_periods_factory.ChargeLinkPeriodsTestDataSpec(
            calculation_id=CALCULATION_ID,
            calculation_type=CALCULATION_TYPE,
            calculation_version=1,
            charge_key=CHARGE_KEY,
            charge_code=CHARGE_CODE,
            charge_type=CHARGE_TYPE,
            charge_owner_id=CHARGE_OWNER_ID,
            metering_point_id=metering_point.metering_point_id,
            quantity=1,
            from_date=FROM_DATE,
            to_date=TO_DATE,
        )
        next_df = charge_link_periods_factory.create(spark, data_spec)
        if df is None:
            df = next_df
        else:
            df = df.union(next_df)

    return df


def create_charge_price_information_periods(spark: SparkSession) -> DataFrame:
    """
    Creates a DataFrame with charge price information periods data for testing purposes.
    """

    data_spec = charge_price_information_periods_factory.ChargePriceInformationPeriodsTestDataSpec(
        calculation_id=CALCULATION_ID,
        calculation_type=CALCULATION_TYPE,
        calculation_version=1,
        charge_key=CHARGE_KEY,
        charge_code=CHARGE_CODE,
        charge_type=CHARGE_TYPE,
        charge_owner_id=CHARGE_OWNER_ID,
        is_tax=IS_TAX,
        resolution=ChargeResolutionDataProductValue.HOUR,
        from_date=FROM_DATE,
        to_date=TO_DATE,
    )
    return charge_price_information_periods_factory.create(spark, data_spec)


def create_latest_calculations(spark: SparkSession) -> DataFrame:
    """
    Creates a DataFrame with latest calculations data for testing purposes.
    """

    data_specs = []
    for grid_area_code in GRID_AREAS:
        current_date = FROM_DATE
        while current_date < TO_DATE:
            data_specs.append(
                latest_calculations_factory.LatestCalculationsTestDataSpec(
                    calculation_id=CALCULATION_ID,
                    calculation_type=CALCULATION_TYPE,
                    calculation_version=1,
                    grid_area_code=grid_area_code,
                    start_of_day=current_date,
                )
            )
            current_date += timedelta(days=1)

    return latest_calculations_factory.create(spark, data_specs)


def _get_energy_test_spec(
    metering_point: MeteringPointSpec,
) -> energy_factory.EnergyTestDataSpec:
    return energy_factory.EnergyTestDataSpec(
        calculation_id=CALCULATION_ID,
        calculation_type=CALCULATION_TYPE,
        calculation_period_start=CALCULATION_PERIOD_START,
        calculation_period_end=CALCULATION_PERIOD_END,
        calculation_version=1,
        result_id=RESULT_ID,
        grid_area_code=metering_point.grid_area_code,
        metering_point_type=metering_point.metering_point_type,
        settlement_method=None,
        resolution=metering_point.resolution,
        quantity=Decimal("1.005"),
        quantity_unit=QUANTITY_UNIT,
        quantity_qualities=QUANTITY_QUALITIES,
        from_date=FROM_DATE,
        to_date=TO_DATE,
        energy_supplier_id=metering_point.energy_supplier_id,
        balance_responsible_party_id=BALANCE_RESPONSIBLE_PARTY_ID,
    )


def create_energy(spark: SparkSession) -> DataFrame:
    """
    Creates a DataFrame with energy data for testing purposes.
    Mimics the wholesale_results.energy_v1 view.
    """

    df = None
    for metering_point in _get_all_metering_points():
        data_spec = _get_energy_test_spec(metering_point)
        next_df = energy_factory.create_energy_v1(spark, data_spec)
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
        data_spec = _get_energy_test_spec(metering_point)
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
