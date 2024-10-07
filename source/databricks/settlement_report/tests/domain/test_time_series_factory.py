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

import test_factories.default_test_data_spec as default_data
import test_factories.metering_point_time_series_factory as time_series_factory
import test_factories.charge_link_periods_factory as charge_links_factory
import test_factories.charge_price_information_periods_factory as charge_price_information_periods


from settlement_report_job.domain.market_role import MarketRole
from settlement_report_job.domain.metering_point_resolution import (
    DataProductMeteringPointResolution,
)
from settlement_report_job.domain.time_series_factory import create_time_series
from settlement_report_job.infrastructure.column_names import (
    DataProductColumnNames,
    TimeSeriesPointCsvColumnNames,
)

DEFAULT_TIME_ZONE = "Europe/Copenhagen"
DEFAULT_FROM_DATE = default_data.DEFAULT_FROM_DATE
DEFAULT_TO_DATE = default_data.DEFAULT_TO_DATE
DATAHUB_ADMINISTRATOR_ID = "1234567890123"
SYSTEM_OPERATOR_ID = "3333333333333"
NOT_SYSTEM_OPERATOR_ID = "4444444444444"


def _create_time_series_with_increasing_quantity(
    spark: SparkSession,
    from_date: datetime,
    to_date: datetime,
    resolution: DataProductMeteringPointResolution,
) -> DataFrame:
    spec = default_data.create_time_series_data_spec(
        from_date=from_date, to_date=to_date, resolution=resolution
    )
    df = time_series_factory.create(spark, spec)
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
    spec = default_data.create_time_series_data_spec(
        from_date=DEFAULT_FROM_DATE, to_date=DEFAULT_TO_DATE, resolution=resolution
    )
    df = time_series_factory.create(spark, spec)
    mock_repository = Mock()
    mock_repository.read_metering_point_time_series.return_value = df

    # Act
    result_df = create_time_series(
        period_start=DEFAULT_FROM_DATE,
        period_end=DEFAULT_TO_DATE,
        calculation_id_by_grid_area={
            default_data.DEFAULT_GRID_AREA_CODE: uuid.UUID(
                default_data.DEFAULT_CALCULATION_ID
            )
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


@pytest.mark.parametrize(
    "resolution, energy_quantity_column_count",
    [
        (DataProductMeteringPointResolution.HOUR, 25),
        (DataProductMeteringPointResolution.QUARTER, 100),
    ],
)
def test_create_time_series__returns_expected_energy_quantity_columns(
    spark: SparkSession,
    resolution: DataProductMeteringPointResolution,
    energy_quantity_column_count: int,
) -> None:
    # Arrange
    expected_columns = [
        f"ENERGYQUANTITY{i}" for i in range(1, energy_quantity_column_count + 1)
    ]
    spec = default_data.create_time_series_data_spec(resolution=resolution)
    df = time_series_factory.create(spark, spec)
    mock_repository = Mock()
    mock_repository.read_metering_point_time_series.return_value = df

    # Act
    actual_df = create_time_series(
        period_start=DEFAULT_FROM_DATE,
        period_end=DEFAULT_TO_DATE,
        calculation_id_by_grid_area={
            default_data.DEFAULT_GRID_AREA_CODE: uuid.UUID(
                default_data.DEFAULT_CALCULATION_ID
            )
        },
        energy_supplier_ids=None,
        resolution=resolution,
        requesting_actor_market_role=MarketRole.DATAHUB_ADMINISTRATOR,
        requesting_actor_id=DATAHUB_ADMINISTRATOR_ID,
        time_zone=DEFAULT_TIME_ZONE,
        repository=mock_repository,
    )

    # Assert
    actual_columns = [
        col for col in actual_df.columns if col.startswith("ENERGYQUANTITY")
    ]
    assert set(actual_columns) == set(expected_columns)


@pytest.mark.parametrize(
    "from_date,to_date,resolution,expected_columns_with_data",
    [
        (
            # Entering daylight saving time for hourly resolution
            datetime(2023, 3, 25, 23),
            datetime(2023, 3, 27, 22),
            DataProductMeteringPointResolution.HOUR,
            23,
        ),
        (
            # Entering daylight saving time for quarterly resolution
            datetime(2023, 3, 25, 23),
            datetime(2023, 3, 27, 22),
            DataProductMeteringPointResolution.QUARTER,
            92,
        ),
        (
            # Exiting daylight saving time for hourly resolution
            datetime(2023, 10, 28, 22),
            datetime(2023, 10, 30, 23),
            DataProductMeteringPointResolution.HOUR,
            25,
        ),
        (
            # Exiting daylight saving time for quarterly resolution
            datetime(2023, 10, 28, 22),
            datetime(2023, 10, 30, 23),
            DataProductMeteringPointResolution.QUARTER,
            100,
        ),
    ],
)
def test_create_time_series__when_daylight_saving_tim_transition__returns_expected_energy_quantities(
    spark: SparkSession,
    from_date: datetime,
    to_date: datetime,
    resolution: DataProductMeteringPointResolution,
    expected_columns_with_data: int,
) -> None:
    # Arrange
    df = _create_time_series_with_increasing_quantity(
        spark=spark,
        from_date=from_date,
        to_date=to_date,
        resolution=resolution,
    )
    total_columns = 25 if resolution == DataProductMeteringPointResolution.HOUR else 100

    mock_repository = Mock()
    mock_repository.read_metering_point_time_series.return_value = df

    # Act
    actual_df = create_time_series(
        period_start=from_date,
        period_end=to_date,
        calculation_id_by_grid_area={
            default_data.DEFAULT_GRID_AREA_CODE: uuid.UUID(
                default_data.DEFAULT_CALCULATION_ID
            )
        },
        energy_supplier_ids=None,
        resolution=resolution,
        requesting_actor_market_role=MarketRole.DATAHUB_ADMINISTRATOR,
        requesting_actor_id=DATAHUB_ADMINISTRATOR_ID,
        time_zone=DEFAULT_TIME_ZONE,
        repository=mock_repository,
    )

    # Assert
    assert actual_df.count() == 2
    dst_day = actual_df.where(
        F.col(TimeSeriesPointCsvColumnNames.start_of_day) == from_date
    ).collect()[0]
    for i in range(1, total_columns):
        expected_value = None if i > expected_columns_with_data else Decimal(i - 1)
        assert dst_day[f"ENERGYQUANTITY{i}"] == expected_value


@pytest.mark.parametrize(
    "resolution",
    [
        DataProductMeteringPointResolution.HOUR,
        DataProductMeteringPointResolution.QUARTER,
    ],
)
def test_create_time_series__when_input_has_both_resolution_types__returns_only_data_with_expected_resolution(
    spark: SparkSession,
    resolution: DataProductMeteringPointResolution,
) -> None:
    # Arrange
    hourly_metering_point_id = "1111111111111"
    quarterly_metering_point_id = "1515151515115"
    expected_metering_point_id = (
        hourly_metering_point_id
        if resolution == DataProductMeteringPointResolution.HOUR
        else quarterly_metering_point_id
    )
    spec_hour = default_data.create_time_series_data_spec(
        metering_point_id=hourly_metering_point_id,
        resolution=DataProductMeteringPointResolution.HOUR,
    )
    spec_quarter = default_data.create_time_series_data_spec(
        metering_point_id=quarterly_metering_point_id,
        resolution=DataProductMeteringPointResolution.QUARTER,
    )
    df = time_series_factory.create(spark, spec_hour).union(
        time_series_factory.create(spark, spec_quarter)
    )

    mock_repository = Mock()
    mock_repository.read_metering_point_time_series.return_value = df

    # Act
    actual_df = create_time_series(
        period_start=DEFAULT_FROM_DATE,
        period_end=DEFAULT_TO_DATE,
        calculation_id_by_grid_area={
            default_data.DEFAULT_GRID_AREA_CODE: uuid.UUID(
                default_data.DEFAULT_CALCULATION_ID
            )
        },
        energy_supplier_ids=None,
        requesting_actor_market_role=MarketRole.DATAHUB_ADMINISTRATOR,
        requesting_actor_id=DATAHUB_ADMINISTRATOR_ID,
        resolution=resolution,
        time_zone=DEFAULT_TIME_ZONE,
        repository=mock_repository,
    )

    # Assert
    assert actual_df.count() == 1
    assert (
        actual_df.collect()[0][TimeSeriesPointCsvColumnNames.metering_point_id]
        == expected_metering_point_id
    )


def test_create_time_series__returns_only_days_within_selected_period(
    spark: SparkSession,
) -> None:
    # Arrange
    df = time_series_factory.create(
        spark,
        default_data.create_time_series_data_spec(),
    )
    mock_repository = Mock()
    mock_repository.read_metering_point_time_series.return_value = df

    # Act
    actual_df = create_time_series(
        period_start=DEFAULT_FROM_DATE,
        period_end=DEFAULT_TO_DATE,
        calculation_id_by_grid_area={
            default_data.DEFAULT_GRID_AREA_CODE: uuid.UUID(
                default_data.DEFAULT_CALCULATION_ID
            )
        },
        energy_supplier_ids=None,
        requesting_actor_market_role=MarketRole.DATAHUB_ADMINISTRATOR,
        requesting_actor_id=DATAHUB_ADMINISTRATOR_ID,
        resolution=DataProductMeteringPointResolution.HOUR,
        time_zone=DEFAULT_TIME_ZONE,
        repository=mock_repository,
    )

    # Assert
    assert actual_df.count() == 1
    assert (
        actual_df.collect()[0][TimeSeriesPointCsvColumnNames.start_of_day]
        == DEFAULT_FROM_DATE
    )


def test_create_time_series__returns_only_selected_grid_area(
    spark: SparkSession,
) -> None:
    # Arrange
    selected_grid_area_code = "805"
    not_selected_grid_area_code = "806"
    df = time_series_factory.create(
        spark,
        default_data.create_time_series_data_spec(
            grid_area_code=selected_grid_area_code,
        ),
    ).union(
        time_series_factory.create(
            spark,
            default_data.create_time_series_data_spec(
                grid_area_code=not_selected_grid_area_code,
            ),
        )
    )
    mock_repository = Mock()
    mock_repository.read_metering_point_time_series.return_value = df

    # Act
    actual_df = create_time_series(
        calculation_id_by_grid_area={
            selected_grid_area_code: uuid.UUID(default_data.DEFAULT_CALCULATION_ID)
        },
        energy_supplier_ids=None,
        requesting_actor_market_role=MarketRole.DATAHUB_ADMINISTRATOR,
        requesting_actor_id=DATAHUB_ADMINISTRATOR_ID,
        resolution=DataProductMeteringPointResolution.HOUR,
        time_zone=DEFAULT_TIME_ZONE,
        repository=mock_repository,
    )

    # Assert
    assert actual_df.count() == 1
    assert (
        actual_df.collect()[0][DataProductColumnNames.grid_area_code]
        == selected_grid_area_code
    )


def test_create_time_series__returns_only_selected_calculation_id(
    spark: SparkSession,
) -> None:
    # Arrange
    selected_calculation_id = "11111111-9fc8-409a-a169-fbd49479d718"
    not_selected_calculation_id = "22222222-9fc8-409a-a169-fbd49479d718"
    expected_metering_point_id = "123456789012345678901234567"
    other_metering_point_id = "765432109876543210987654321"
    df = time_series_factory.create(
        spark,
        default_data.create_time_series_data_spec(
            calculation_id=selected_calculation_id,
            metering_point_id=expected_metering_point_id,
        ),
    ).union(
        time_series_factory.create(
            spark,
            default_data.create_time_series_data_spec(
                calculation_id=not_selected_calculation_id,
                metering_point_id=other_metering_point_id,
            ),
        )
    )
    mock_repository = Mock()
    mock_repository.read_metering_point_time_series.return_value = df

    # Act
    actual_df = create_time_series(
        period_start=DEFAULT_FROM_DATE,
        period_end=DEFAULT_TO_DATE,
        calculation_id_by_grid_area={
            default_data.DEFAULT_GRID_AREA_CODE: uuid.UUID(selected_calculation_id)
        },
        energy_supplier_ids=None,
        requesting_actor_market_role=MarketRole.DATAHUB_ADMINISTRATOR,
        requesting_actor_id=DATAHUB_ADMINISTRATOR_ID,
        resolution=DataProductMeteringPointResolution.HOUR,
        time_zone=DEFAULT_TIME_ZONE,
        repository=mock_repository,
    )

    # Assert
    assert actual_df.count() == 1
    assert (
        actual_df.collect()[0][TimeSeriesPointCsvColumnNames.metering_point_id]
        == expected_metering_point_id
    )


ENERGY_SUPPLIER_A = "1000000000000"
ENERGY_SUPPLIER_B = "2000000000000"
ENERGY_SUPPLIER_C = "3000000000000"
ENERGY_SUPPLIERS_ABC = [ENERGY_SUPPLIER_A, ENERGY_SUPPLIER_B, ENERGY_SUPPLIER_C]


@pytest.mark.parametrize(
    "selected_energy_supplier_ids,expected_energy_supplier_ids",
    [
        (None, ENERGY_SUPPLIERS_ABC),
        ([ENERGY_SUPPLIER_B], [ENERGY_SUPPLIER_B]),
        (
            [ENERGY_SUPPLIER_A, ENERGY_SUPPLIER_B],
            [ENERGY_SUPPLIER_A, ENERGY_SUPPLIER_B],
        ),
        (ENERGY_SUPPLIERS_ABC, ENERGY_SUPPLIERS_ABC),
    ],
)
def test_create_time_series__returns_data_for_expected_energy_suppliers(
    spark: SparkSession,
    selected_energy_supplier_ids: list[str] | None,
    expected_energy_supplier_ids: list[str],
) -> None:
    # Arrange
    df = reduce(
        lambda df1, df2: df1.union(df2),
        [
            time_series_factory.create(
                spark,
                default_data.create_time_series_data_spec(
                    energy_supplier_id=energy_supplier_id,
                ),
            )
            for energy_supplier_id in ENERGY_SUPPLIERS_ABC
        ],
    )
    mock_repository = Mock()
    mock_repository.read_metering_point_time_series.return_value = df

    # Act
    actual_df = create_time_series(
        period_start=DEFAULT_FROM_DATE,
        period_end=DEFAULT_TO_DATE,
        calculation_id_by_grid_area={
            default_data.DEFAULT_GRID_AREA_CODE: uuid.UUID(
                default_data.DEFAULT_CALCULATION_ID
            )
        },
        energy_supplier_ids=selected_energy_supplier_ids,
        requesting_actor_market_role=MarketRole.DATAHUB_ADMINISTRATOR,
        requesting_actor_id=DATAHUB_ADMINISTRATOR_ID,
        resolution=DataProductMeteringPointResolution.HOUR,
        time_zone=DEFAULT_TIME_ZONE,
        repository=mock_repository,
    )

    # Assert
    assert set(
        row[TimeSeriesPointCsvColumnNames.energy_supplier_id]
        for row in actual_df.collect()
    ) == set(expected_energy_supplier_ids)


@pytest.mark.parametrize(
    "charge_owner_id,return_rows",
    [
        (SYSTEM_OPERATOR_ID, True),
        (NOT_SYSTEM_OPERATOR_ID, False),
    ],
)
def test_create_time_series__when_system_operator__returns_only_time_series_with_system_operator_as_charge_owner(
    spark: SparkSession,
    charge_owner_id: str,
    return_rows: bool,
) -> None:
    # Arrange
    time_series_df = time_series_factory.create(
        spark,
        default_data.create_time_series_data_spec(),
    )
    charge_price_information_period_df = charge_price_information_periods.create(
        spark,
        default_data.create_charge_price_information_periods_data_spec(
            charge_owner_id=SYSTEM_OPERATOR_ID
        ),
    )
    charge_link_periods_df = charge_links_factory.create(
        spark,
        default_data.create_charge_link_periods_data_spec(
            charge_owner_id=SYSTEM_OPERATOR_ID
        ),
    )
    mock_repository = Mock()
    mock_repository.read_metering_point_time_series.return_value = time_series_df
    mock_repository.read_charge_price_information_periods.return_value = (
        charge_price_information_period_df
    )
    mock_repository.read_charge_link_periods.return_value = charge_link_periods_df

    # Act
    actual = create_time_series(
        period_start=DEFAULT_FROM_DATE,
        period_end=DEFAULT_TO_DATE,
        calculation_id_by_grid_area={
            default_data.DEFAULT_GRID_AREA_CODE: uuid.UUID(
                default_data.DEFAULT_CALCULATION_ID
            )
        },
        energy_supplier_ids=None,
        requesting_actor_market_role=MarketRole.SYSTEM_OPERATOR,
        requesting_actor_id=charge_owner_id,
        resolution=DataProductMeteringPointResolution.HOUR,
        time_zone=DEFAULT_TIME_ZONE,
        repository=mock_repository,
    )

    # Assert
    assert (actual.count() > 0) == return_rows
