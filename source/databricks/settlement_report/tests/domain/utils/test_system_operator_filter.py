from datetime import datetime
import pytest
from pyspark.sql import SparkSession
import tests.test_factories.default_test_data_spec as default_data
import tests.test_factories.metering_point_time_series_factory as time_series_points_factory
import tests.test_factories.charge_link_periods_factory as charge_link_periods_factory
import tests.test_factories.charge_price_information_periods_factory as charge_price_information_periods_factory

from settlement_report_job.domain.time_series_points.system_operator_filter import (
    filter_time_series_points_on_charge_owner,
)
from settlement_report_job.infrastructure.wholesale.column_names import (
    DataProductColumnNames,
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
def test_filter_time_series_points_on_charge_owner__returns_only_time_series_points_within_charge_period(
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
        default_data.create_charge_link_periods_row(
            from_date=charge_from_date, to_date=charge_to_date
        ),
    )
    charge_price_information_periods_df = (
        charge_price_information_periods_factory.create(
            spark,
            default_data.create_charge_price_information_periods_row(
                from_date=charge_from_date, to_date=charge_to_date
            ),
        )
    )
    time_series_points_df = time_series_points_factory.create(
        spark,
        default_data.create_time_series_points_data_spec(
            from_date=mp_from_date, to_date=mp_to_date
        ),
    )

    # Act
    actual = filter_time_series_points_on_charge_owner(
        time_series_points=time_series_points_df,
        system_operator_id=default_data.DEFAULT_CHARGE_OWNER_ID,
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
def test_filter_time_series_points_on_charge_owner__returns_only_time_series_points_if_calculation_id_is_the_same_as_for_the_charge(
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
            default_data.create_charge_price_information_periods_row(
                calculation_id=calculation_id_charge_price_information,
            ),
        )
    )
    charge_link_periods_df = charge_link_periods_factory.create(
        spark,
        default_data.create_charge_link_periods_row(
            calculation_id=calculation_id_charge_link,
        ),
    )

    time_series_points_df = time_series_points_factory.create(
        spark,
        default_data.create_time_series_points_data_spec(
            calculation_id=calculation_id_metering_point,
        ),
    )

    # Act
    actual = filter_time_series_points_on_charge_owner(
        time_series_points=time_series_points_df,
        system_operator_id=default_data.DEFAULT_CHARGE_OWNER_ID,
        charge_link_periods=charge_link_periods_df,
        charge_price_information_periods=charge_price_information_periods_df,
    )

    # Assert
    assert (actual.count() > 0) == returns_data


def test_filter_time_series_points_on_charge_owner__returns_only_time_series_points_where_the_charge_link_has_the_same_metering_point_id(
    spark: SparkSession,
) -> None:
    # Arrange
    charge_price_information_periods_df = (
        charge_price_information_periods_factory.create(
            spark,
            default_data.create_charge_price_information_periods_row(),
        )
    )
    charge_link_periods_df = charge_link_periods_factory.create(
        spark,
        default_data.create_charge_link_periods_row(
            metering_point_id="matching_metering_point_id"
        ),
    )

    time_series_points_df = time_series_points_factory.create(
        spark,
        default_data.create_time_series_points_data_spec(
            metering_point_id="matching_metering_point_id"
        ),
    ).union(
        time_series_points_factory.create(
            spark,
            default_data.create_time_series_points_data_spec(
                metering_point_id="non_matching_metering_point_id"
            ),
        )
    )

    # Act
    actual = filter_time_series_points_on_charge_owner(
        time_series_points=time_series_points_df,
        system_operator_id=default_data.DEFAULT_CHARGE_OWNER_ID,
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


def test_filter_time_series_points_on_charge_owner__when_multiple_links_matches_on_metering_point_id__returns_expected_number_of_time_series_points(
    spark: SparkSession,
) -> None:
    # Arrange
    charge_price_information_periods_df = (
        charge_price_information_periods_factory.create(
            spark,
            default_data.create_charge_price_information_periods_row(
                charge_code="code1"
            ),
        )
    ).union(
        charge_price_information_periods_factory.create(
            spark,
            default_data.create_charge_price_information_periods_row(
                charge_code="code2"
            ),
        )
    )
    charge_link_periods_df = charge_link_periods_factory.create(
        spark,
        default_data.create_charge_link_periods_row(charge_code="code1"),
    ).union(
        charge_link_periods_factory.create(
            spark,
            default_data.create_charge_link_periods_row(charge_code="code2"),
        )
    )

    time_series_points_df = time_series_points_factory.create(
        spark,
        default_data.create_time_series_points_data_spec(),
    )

    # Act
    actual = filter_time_series_points_on_charge_owner(
        time_series_points=time_series_points_df,
        system_operator_id=default_data.DEFAULT_CHARGE_OWNER_ID,
        charge_link_periods=charge_link_periods_df,
        charge_price_information_periods=charge_price_information_periods_df,
    )

    # Assert
    assert actual.count() == 24


def test_filter_time_series_points_on_charge_owner__when_charge_owner_is_not_system_operator__returns_time_series_points_without_that_metering_point(
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
            default_data.create_charge_price_information_periods_row(
                charge_owner_id=system_operator_id
            ),
        )
    ).union(
        charge_price_information_periods_factory.create(
            spark,
            default_data.create_charge_price_information_periods_row(
                charge_owner_id=not_system_operator_id
            ),
        )
    )
    charge_link_periods_df = charge_link_periods_factory.create(
        spark,
        default_data.create_charge_link_periods_row(
            metering_point_id=system_operator_metering_point_id,
            charge_owner_id=system_operator_id,
        ),
    ).union(
        charge_link_periods_factory.create(
            spark,
            default_data.create_charge_link_periods_row(
                metering_point_id=not_system_operator_metering_point_id,
                charge_owner_id=not_system_operator_id,
            ),
        )
    )

    time_series_points_df = time_series_points_factory.create(
        spark,
        default_data.create_time_series_points_data_spec(
            metering_point_id=system_operator_metering_point_id
        ),
    ).union(
        time_series_points_factory.create(
            spark,
            default_data.create_time_series_points_data_spec(
                metering_point_id=not_system_operator_metering_point_id
            ),
        )
    )

    # Act
    actual = filter_time_series_points_on_charge_owner(
        time_series_points=time_series_points_df,
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


@pytest.mark.parametrize(
    "is_tax, returns_rows",
    [
        (True, False),
        (False, True),
    ],
)
def test_filter_time_series_points_on_charge_owner__returns_only_time_series_points_from_metering_points_without_tax_associated(
    spark: SparkSession, is_tax: bool, returns_rows: bool
) -> None:
    # Arrange
    system_operator_id = "1234567890123"
    charge_price_information_periods_df = (
        charge_price_information_periods_factory.create(
            spark,
            default_data.create_charge_price_information_periods_row(
                charge_owner_id=system_operator_id, is_tax=is_tax
            ),
        )
    )
    charge_link_periods_df = charge_link_periods_factory.create(
        spark,
        default_data.create_charge_link_periods_row(charge_owner_id=system_operator_id),
    )

    time_series_points_df = time_series_points_factory.create(
        spark,
        default_data.create_time_series_points_data_spec(),
    )

    # Act
    actual = filter_time_series_points_on_charge_owner(
        time_series_points=time_series_points_df,
        system_operator_id=system_operator_id,
        charge_link_periods=charge_link_periods_df,
        charge_price_information_periods=charge_price_information_periods_df,
    )

    # Assert
    assert (actual.count() > 0) == returns_rows
