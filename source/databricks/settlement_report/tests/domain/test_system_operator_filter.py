from datetime import datetime, timedelta
from decimal import Decimal

import pytest
from pyspark.sql import SparkSession
import test_factories.metering_point_time_series_factory as time_series_factory
import test_factories.charge_link_periods_factory as charge_link_periods_factory
import test_factories.charge_price_information_periods_factory as charge_price_information_periods_factory
from settlement_report_job.domain.metering_point_resolution import (
    DataProductMeteringPointResolution,
)
from settlement_report_job.infrastructure.column_names import DataProductColumnNames
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


def create_charge_link_periods_test_data_spec(
    calculation_id: str = DEFAULT_CALCULATION_ID,
    calculation_type: CalculationType = CalculationType.WHOLESALE_FIXING,
    calculation_version: int = DEFAULT_CALCULATION_VERSION,
    charge_code: str = DEFAULT_CHARGE_CODE,
    charge_type: ChargeType = DEFAULT_CHARGE_TYPE,
    charge_owner_id: str = DEFAULT_CHARGE_OWNER_ID,
    metering_point_id: str = DEFAULT_METERING_POINT_ID,
    from_date: datetime = DEFAULT_PERIOD_START,
    to_date: datetime = DEFAULT_PERIOD_END,
    quantity: int = 1,
) -> charge_link_periods_factory.ChargeLinkPeriodsTestDataSpec:
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


def create_default_charge_price_information_periods_test_data_spec(
    calculation_id: str = DEFAULT_CALCULATION_ID,
    calculation_type: CalculationType = CalculationType.WHOLESALE_FIXING,
    calculation_version: int = DEFAULT_CALCULATION_VERSION,
    charge_code: str = DEFAULT_CHARGE_CODE,
    charge_type: ChargeType = DEFAULT_CHARGE_TYPE,
    charge_owner_id: str = DEFAULT_CHARGE_OWNER_ID,
    resolution: ChargeResolution = ChargeResolution.HOUR,
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


def create_time_series_test_data_spec(
    calculation_id: str = DEFAULT_CALCULATION_ID,
    calculation_type: CalculationType = CalculationType.WHOLESALE_FIXING,
    calculation_version: int = DEFAULT_CALCULATION_VERSION,
    metering_point_id: str = DEFAULT_METERING_POINT_ID,
    metering_point_type: MeteringPointType = DEFAULT_METERING_TYPE,
    resolution: DataProductMeteringPointResolution = DataProductMeteringPointResolution.HOUR,
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


@pytest.mark.parametrize(
    "mp_from_date, mp_to_date, charge_from_date, charge_to_date, expected_row_count",
    [
        (
            # one day overlap charge starts later
            datetime(2022, 1, 1, 23),
            datetime(2022, 1, 3, 23),
            datetime(2022, 1, 2, 23),
            datetime(2022, 1, 4, 23),
            24,
        ),
        (
            # one day overlap metering point period starts later
            datetime(2022, 1, 2, 23),
            datetime(2022, 1, 4, 23),
            datetime(2022, 1, 1, 23),
            datetime(2022, 1, 3, 23),
            24,
        ),
        (
            # no overlap
            datetime(2022, 1, 2, 23),
            datetime(2022, 1, 4, 23),
            datetime(2022, 1, 4, 23),
            datetime(2022, 1, 6, 23),
            0,
        ),
    ],
)
def test_filter_time_series_on_charge_owner__returns_only_time_series_points_within_charge_period(
    spark: SparkSession,
    mp_from_date: datetime,
    mp_to_date: datetime,
    charge_from_date: datetime,
    charge_to_date: datetime,
    expected_row_count: int,
) -> None:
    # Arrange
    charge_link_periods_df = charge_link_periods_factory.create(
        spark,
        create_charge_link_periods_test_data_spec(
            from_date=charge_from_date, to_date=charge_to_date
        ),
    )
    charge_price_information_periods_df = (
        charge_price_information_periods_factory.create(
            spark,
            create_default_charge_price_information_periods_test_data_spec(
                from_date=charge_from_date, to_date=charge_to_date
            ),
        )
    )
    time_series_df = time_series_factory.create(
        spark,
        create_time_series_test_data_spec(from_date=mp_from_date, to_date=mp_to_date),
    )

    # Act
    actual = filter_time_series_on_charge_owner(
        time_series=time_series_df,
        system_operator_id=DEFAULT_CHARGE_OWNER_ID,
        charge_link_periods=charge_link_periods_df,
        charge_price_information_periods=charge_price_information_periods_df,
    )

    # Assert
    assert actual.count() == expected_row_count


@pytest.mark.parametrize(
    "calculation_id_charge_price_information, calculation_id_charge_link, calculation_id_metering_point, returns_data",
    [
        (
            "11111111-1111-1111-1111-111111111111",
            "11111111-1111-1111-1111-111111111111",
            "11111111-1111-1111-1111-111111111111",
            True,
        ),
        (
            "22222222-1111-1111-1111-111111111111",
            "22222222-1111-1111-1111-111111111111",
            "11111111-1111-1111-1111-111111111111",
            False,
        ),
        (
            "22222222-1111-1111-1111-111111111111",
            "11111111-1111-1111-1111-111111111111",
            "11111111-1111-1111-1111-111111111111",
            False,
        ),
    ],
)
def test_filter_time_series_on_charge_owner__returns_only_time_series_if_calculation_id_is_the_same_as_for_the_charge(
    spark: SparkSession,
    calculation_id_charge_price_information: str,
    calculation_id_charge_link: str,
    calculation_id_metering_point: str,
    returns_data: bool,
) -> None:
    # Arrange
    charge_price_information_periods_df = (
        charge_price_information_periods_factory.create(
            spark,
            create_default_charge_price_information_periods_test_data_spec(
                calculation_id=calculation_id_charge_price_information,
            ),
        )
    )
    charge_link_periods_df = charge_link_periods_factory.create(
        spark,
        create_charge_link_periods_test_data_spec(
            calculation_id=calculation_id_charge_link,
        ),
    )

    time_series_df = time_series_factory.create(
        spark,
        create_time_series_test_data_spec(
            calculation_id=calculation_id_metering_point,
        ),
    )

    # Act
    actual = filter_time_series_on_charge_owner(
        time_series=time_series_df,
        system_operator_id=DEFAULT_CHARGE_OWNER_ID,
        charge_link_periods=charge_link_periods_df,
        charge_price_information_periods=charge_price_information_periods_df,
    )

    # Assert
    assert (actual.count() > 0) == returns_data


def test_filter_time_series_on_charge_owner__returns_only_time_series_where_the_charge_link_has_the_same_metering_point_id(
    spark: SparkSession,
) -> None:
    # Arrange
    charge_price_information_periods_df = (
        charge_price_information_periods_factory.create(
            spark,
            create_default_charge_price_information_periods_test_data_spec(),
        )
    )
    charge_link_periods_df = charge_link_periods_factory.create(
        spark,
        create_charge_link_periods_test_data_spec(
            metering_point_id="matching_metering_point_id"
        ),
    )

    time_series_df = time_series_factory.create(
        spark,
        create_time_series_test_data_spec(
            metering_point_id="matching_metering_point_id"
        ),
    ).union(
        time_series_factory.create(
            spark,
            create_time_series_test_data_spec(
                metering_point_id="non_matching_metering_point_id"
            ),
        )
    )

    # Act
    actual = filter_time_series_on_charge_owner(
        time_series=time_series_df,
        system_operator_id=DEFAULT_CHARGE_OWNER_ID,
        charge_link_periods=charge_link_periods_df,
        charge_price_information_periods=charge_price_information_periods_df,
    )

    # Assert
    assert (
        actual.select(DataProductColumnNames.metering_point_id).distinct().count() == 1
    )
    assert (
        actual.select(DataProductColumnNames.metering_point_id).distinct().first()[0]
        == "matching_metering_point_id"
    )


def test_filter_time_series_on_charge_owner__when_multiple_links_matches_on_metering_point_id__returns_expected_number_of_time_series_points(
    spark: SparkSession,
) -> None:
    # Arrange
    charge_price_information_periods_df = (
        charge_price_information_periods_factory.create(
            spark,
            create_default_charge_price_information_periods_test_data_spec(
                charge_code="code1"
            ),
        )
    ).union(
        charge_price_information_periods_factory.create(
            spark,
            create_default_charge_price_information_periods_test_data_spec(
                charge_code="code2"
            ),
        )
    )
    charge_link_periods_df = charge_link_periods_factory.create(
        spark,
        create_charge_link_periods_test_data_spec(charge_code="code1"),
    ).union(
        charge_link_periods_factory.create(
            spark,
            create_charge_link_periods_test_data_spec(charge_code="code2"),
        )
    )

    time_series_df = time_series_factory.create(
        spark,
        create_time_series_test_data_spec(),
    )

    # Act
    actual = filter_time_series_on_charge_owner(
        time_series=time_series_df,
        system_operator_id=DEFAULT_CHARGE_OWNER_ID,
        charge_link_periods=charge_link_periods_df,
        charge_price_information_periods=charge_price_information_periods_df,
    )

    # Assert
    assert actual.count() == 24


def test_filter_time_series_on_charge_owner__when_charge_owner_is_not_system_operator__returns_time_series_without_that_metering_point(
    spark: SparkSession,
) -> None:
    # Arrange
    system_operator_id = "1234567890123"
    not_system_operator_id = "9876543210123"
    system_operator_metering_point_id = "1111111111111"
    not_system_operator_metering_point_id = "2222222222222"
    charge_price_information_periods_df = (
        charge_price_information_periods_factory.create(
            spark,
            create_default_charge_price_information_periods_test_data_spec(
                charge_owner_id=system_operator_id
            ),
        )
    ).union(
        charge_price_information_periods_factory.create(
            spark,
            create_default_charge_price_information_periods_test_data_spec(
                charge_owner_id=not_system_operator_id
            ),
        )
    )
    charge_link_periods_df = charge_link_periods_factory.create(
        spark,
        create_charge_link_periods_test_data_spec(
            metering_point_id=system_operator_metering_point_id,
            charge_owner_id=system_operator_id,
        ),
    ).union(
        charge_link_periods_factory.create(
            spark,
            create_charge_link_periods_test_data_spec(
                metering_point_id=not_system_operator_metering_point_id,
                charge_owner_id=not_system_operator_id,
            ),
        )
    )

    time_series_df = time_series_factory.create(
        spark,
        create_time_series_test_data_spec(
            metering_point_id=system_operator_metering_point_id
        ),
    ).union(
        time_series_factory.create(
            spark,
            create_time_series_test_data_spec(
                metering_point_id=not_system_operator_metering_point_id
            ),
        )
    )

    # Act
    actual = filter_time_series_on_charge_owner(
        time_series=time_series_df,
        system_operator_id=system_operator_id,
        charge_link_periods=charge_link_periods_df,
        charge_price_information_periods=charge_price_information_periods_df,
    )

    # Assert
    assert (
        actual.select(DataProductColumnNames.metering_point_id).distinct().count() == 1
    )
    assert (
        actual.select(DataProductColumnNames.metering_point_id).distinct().first()[0]
        == system_operator_metering_point_id
    )
