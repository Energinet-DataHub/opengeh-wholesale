from dataclasses import dataclass
from datetime import datetime, timedelta
from decimal import Decimal

from pyspark.sql import SparkSession, DataFrame

from settlement_report_job.infrastructure.wholesale.data_values import (
    CalculationTypeDataProductValue,
    MeteringPointResolutionDataProductValue,
    MeteringPointTypeDataProductValue,
    SettlementMethodDataProductValue,
)
from test_factories.default_test_data_spec import create_energy_results_data_spec
from test_factories import (
    metering_point_time_series_factory,
    metering_point_periods_factory,
    latest_calculations_factory,
    energy_factory,
)

GRID_AREAS = ["804", "805"]
CALCULATION_ID = "ba6a4ce2-b549-494b-ad4b-80a35a05a925"
CALCULATION_TYPE = CalculationTypeDataProductValue.BALANCE_FIXING
ENERGY_SUPPLIER_IDS = ["1000000000000", "2000000000000"]
FROM_DATE = datetime(2024, 1, 1, 23)
TO_DATE = FROM_DATE + timedelta(days=1)
"""TO_DATE is exclusive"""
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
                metering_point_type=metering_point.metering_point_type,
                settlement_method=SettlementMethodDataProductValue.FLEX,
                resolution=metering_point.resolution,
                grid_area_code=metering_point.grid_area_code,
                energy_supplier_id=metering_point.energy_supplier_id,
                balance_responsible_party_id=BALANCE_RESPONSIBLE_PARTY_ID,
                from_grid_area_code=None,
                to_grid_area_code=None,
                parent_metering_point_id=None,
                from_date=FROM_DATE,
                to_date=TO_DATE,
            )
        )

    return metering_point_periods_factory.create(spark, rows)


def create_latest_calculations(spark: SparkSession) -> DataFrame:
    """
    Creates a DataFrame with latest calculations data for testing purposes.
    """

    data_specs = []
    for grid_area_code in GRID_AREAS:
        current_date = FROM_DATE
        while current_date < TO_DATE:
            data_specs.append(
                latest_calculations_factory.LatestCalculationsPerDayRow(
                    calculation_id=CALCULATION_ID,
                    calculation_type=CALCULATION_TYPE,
                    calculation_version=1,
                    grid_area_code=grid_area_code,
                    start_of_day=current_date,
                )
            )
            current_date += timedelta(days=1)

    return latest_calculations_factory.create(spark, data_specs)


def create_energy(spark: SparkSession) -> DataFrame:
    """
    Creates a DataFrame with energy data for testing purposes.
    Mimics the wholesale_results.energy_v1 view.
    """

    df = None
    for metering_point in _get_all_metering_points():
        data_spec = create_energy_results_data_spec(
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


def create_energy_per_es(spark: SparkSession) -> DataFrame:
    """
    Creates a DataFrame with energy data for testing purposes.
    Mimics the wholesale_results.energy_v1 view.
    """

    df = None
    for metering_point in _get_all_metering_points():
        data_spec = create_energy_results_data_spec(
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
