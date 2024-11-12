from datetime import datetime

import pytest
from pyspark.sql import SparkSession, functions as F

from settlement_report_job.domain.utils.merge_periods import (
    merge_connected_periods,
)
from settlement_report_job.infrastructure.wholesale.column_names import (
    DataProductColumnNames,
)

JAN_1ST = datetime(2023, 12, 31, 23)
JAN_2ND = datetime(2024, 1, 1, 23)
JAN_3RD = datetime(2024, 1, 2, 23)
JAN_4TH = datetime(2024, 1, 3, 23)
JAN_5TH = datetime(2024, 1, 4, 23)
JAN_6TH = datetime(2024, 1, 5, 23)
JAN_7TH = datetime(2024, 1, 6, 23)
JAN_8TH = datetime(2024, 1, 7, 23)
JAN_9TH = datetime(2024, 1, 8, 23)


@pytest.mark.parametrize(
    "periods,expected_periods",
    [
        pytest.param(
            [
                (JAN_1ST, JAN_3RD),
                (JAN_2ND, JAN_4TH),
            ],
            [
                (JAN_1ST, JAN_4TH),
            ],
            id="two overlapping periods",
        ),
        pytest.param(
            [
                (JAN_1ST, JAN_3RD),
                (JAN_2ND, JAN_4TH),
                (JAN_3RD, JAN_5TH),
            ],
            [
                (JAN_1ST, JAN_5TH),
            ],
            id="three overlapping periods",
        ),
        pytest.param(
            [
                (JAN_1ST, JAN_3RD),
                (JAN_2ND, JAN_4TH),
                (JAN_5TH, JAN_6TH),
            ],
            [
                (JAN_1ST, JAN_4TH),
                (JAN_5TH, JAN_6TH),
            ],
            id="two overlaps and one isolated",
        ),
        pytest.param(
            [
                (JAN_1ST, JAN_3RD),
                (JAN_2ND, JAN_4TH),
                (JAN_5TH, JAN_7TH),
                (JAN_6TH, JAN_8TH),
            ],
            [
                (JAN_1ST, JAN_4TH),
                (JAN_5TH, JAN_8TH),
            ],
            id="two times two overlaps",
        ),
        pytest.param(
            [
                (JAN_1ST, JAN_3RD),
                (JAN_1ST, JAN_3RD),
            ],
            [
                (JAN_1ST, JAN_3RD),
            ],
            id="two perfect overlaps",
        ),
    ],
)
def test_merge_connecting_periods__when_overlapping_periods__returns_merged_periods(
    spark: SparkSession,
    periods: list[tuple[datetime, datetime]],
    expected_periods: list[tuple[datetime, datetime]],
) -> None:
    # Arrange
    df = spark.createDataFrame(
        [("1", from_date, to_date) for from_date, to_date in periods],
        [
            DataProductColumnNames.charge_key,
            DataProductColumnNames.from_date,
            DataProductColumnNames.to_date,
        ],
    ).orderBy(F.rand())

    # Act
    actual = merge_connected_periods(df)

    # Assert
    actual = actual.orderBy(DataProductColumnNames.from_date)
    assert actual.count() == len(expected_periods)
    for i, (expected_from, expected_to) in enumerate(expected_periods):
        assert actual.collect()[i][DataProductColumnNames.from_date] == expected_from
        assert actual.collect()[i][DataProductColumnNames.to_date] == expected_to


@pytest.mark.parametrize(
    "periods,expected_periods",
    [
        pytest.param(
            [
                (JAN_1ST, JAN_2ND),
                (JAN_3RD, JAN_4TH),
            ],
            [
                (JAN_1ST, JAN_2ND),
                (JAN_3RD, JAN_4TH),
            ],
            id="no connected periods",
        ),
        pytest.param(
            [
                (JAN_1ST, JAN_2ND),
                (JAN_2ND, JAN_3RD),
                (JAN_4TH, JAN_5TH),
                (JAN_5TH, JAN_6TH),
            ],
            [
                (JAN_1ST, JAN_3RD),
                (JAN_4TH, JAN_6TH),
            ],
            id="two connect and two others connected",
        ),
        pytest.param(
            [
                (JAN_1ST, JAN_2ND),
                (JAN_2ND, JAN_3RD),
                (JAN_3RD, JAN_4TH),
                (JAN_5TH, JAN_6TH),
            ],
            [
                (JAN_1ST, JAN_4TH),
                (JAN_5TH, JAN_6TH),
            ],
            id="three connected and one not connected",
        ),
    ],
)
def test_merge_connecting_periods__when_connections_and_gaps_between_periods__returns_merged_rows(
    spark: SparkSession,
    periods: list[tuple[datetime, datetime]],
    expected_periods: list[tuple[datetime, datetime]],
) -> None:
    # Arrange
    df = spark.createDataFrame(
        [("1", from_date, to_date) for from_date, to_date in periods],
        [
            DataProductColumnNames.charge_key,
            DataProductColumnNames.from_date,
            DataProductColumnNames.to_date,
        ],
    ).orderBy(F.rand())

    # Act
    actual = merge_connected_periods(df)

    # Assert
    actual = actual.orderBy(DataProductColumnNames.from_date)
    assert actual.count() == len(expected_periods)
    for i, (expected_from, expected_to) in enumerate(expected_periods):
        assert actual.collect()[i][DataProductColumnNames.from_date] == expected_from
        assert actual.collect()[i][DataProductColumnNames.to_date] == expected_to


@pytest.mark.parametrize(
    "periods,expected_periods",
    [
        pytest.param(
            [
                ("1", JAN_1ST, JAN_2ND),
                ("2", JAN_2ND, JAN_3RD),
            ],
            [
                ("1", JAN_1ST, JAN_2ND),
                ("2", JAN_2ND, JAN_3RD),
            ],
            id="connected but different group",
        ),
        pytest.param(
            [
                ("1", JAN_1ST, JAN_2ND),
                ("2", JAN_2ND, JAN_4TH),
                ("1", JAN_2ND, JAN_3RD),
            ],
            [
                ("1", JAN_1ST, JAN_3RD),
                ("2", JAN_2ND, JAN_4TH),
            ],
            id="one group has overlap and another group has no overlap",
        ),
    ],
)
def test_merge_connecting_periods__when_overlap_but_difference_groups__returns_without_merge(
    spark: SparkSession,
    periods: list[tuple[str, datetime, datetime]],
    expected_periods: list[tuple[str, datetime, datetime]],
) -> None:
    # Arrange
    some_column_name = "some_column"
    df = spark.createDataFrame(
        [
            (some_column_value, from_date, to_date)
            for some_column_value, from_date, to_date in periods
        ],
        [
            some_column_name,
            DataProductColumnNames.from_date,
            DataProductColumnNames.to_date,
        ],
    ).orderBy(F.rand())

    # Act
    actual = merge_connected_periods(df)

    # Assert
    actual = actual.orderBy(DataProductColumnNames.from_date)
    assert actual.count() == len(expected_periods)
    for i, (expected_some_column_value, expected_from, expected_to) in enumerate(
        expected_periods
    ):
        assert actual.collect()[i][some_column_name] == expected_some_column_value
        assert actual.collect()[i][DataProductColumnNames.from_date] == expected_from
        assert actual.collect()[i][DataProductColumnNames.to_date] == expected_to


@pytest.mark.parametrize(
    "periods,expected_periods",
    [
        pytest.param(
            [
                ("A", "B", JAN_1ST, JAN_2ND),
                ("A", "C", JAN_2ND, JAN_3RD),
                ("B", "C", JAN_3RD, JAN_4TH),
            ],
            [
                ("A", "B", JAN_1ST, JAN_2ND),
                ("A", "C", JAN_2ND, JAN_3RD),
                ("B", "C", JAN_3RD, JAN_4TH),
            ],
            id="overlaps but not same group",
        ),
        pytest.param(
            [
                ("A", "B", JAN_1ST, JAN_2ND),
                ("A", "C", JAN_2ND, JAN_3RD),
                ("A", "C", JAN_3RD, JAN_4TH),
            ],
            [
                ("A", "B", JAN_1ST, JAN_2ND),
                ("A", "C", JAN_2ND, JAN_4TH),
            ],
            id="overlaps and including same group",
        ),
    ],
)
def test_merge_connecting_periods__when_multiple_other_columns_and_no___returns_expected(
    spark: SparkSession,
    periods: list[tuple[str, str, datetime, datetime]],
    expected_periods: list[tuple[str, str, datetime, datetime]],
) -> None:
    # Arrange
    column_a = "column_a"
    column_b = "column_b"
    df = spark.createDataFrame(
        [
            (col_a_value, col_b_value, from_date, to_date)
            for col_a_value, col_b_value, from_date, to_date in periods
        ],
        [
            column_a,
            column_b,
            DataProductColumnNames.from_date,
            DataProductColumnNames.to_date,
        ],
    ).orderBy(F.rand())

    # Act
    actual = merge_connected_periods(df)

    # Assert
    actual = actual.orderBy(DataProductColumnNames.from_date)
    assert actual.count() == len(expected_periods)
    for i, (
        expected_col_a,
        expected_col_b,
        expected_from,
        expected_to,
    ) in enumerate(expected_periods):
        assert actual.collect()[i][column_a] == expected_col_a
        assert actual.collect()[i][column_b] == expected_col_b
        assert actual.collect()[i][DataProductColumnNames.from_date] == expected_from
        assert actual.collect()[i][DataProductColumnNames.to_date] == expected_to
