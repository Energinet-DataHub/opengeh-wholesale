import uuid
from datetime import datetime
from unittest.mock import Mock

import pytest
from pyspark.sql import SparkSession

import test_factories.default_test_data_spec as default_data
from settlement_report_job.domain.wholesale_results.read_and_filter import (
    read_and_filter_from_view,
)
from test_factories.default_test_data_spec import create_wholesale_data_spec
from test_factories.wholesale_factory import create_wholesale


DEFAULT_FROM_DATE = default_data.DEFAULT_FROM_DATE
DEFAULT_TO_DATE = default_data.DEFAULT_TO_DATE
DATAHUB_ADMINISTRATOR_ID = "1234567890123"
SYSTEM_OPERATOR_ID = "3333333333333"
NOT_SYSTEM_OPERATOR_ID = "4444444444444"
DEFAULT_TIME_ZONE = "Europe/Copenhagen"
ENERGY_SUPPLIER_IDS = ["1234567890123", "2345678901234"]


@pytest.mark.parametrize(
    "args_start_date, args_end_date, expected_rows",
    [
        pytest.param(
            datetime(2024, 1, 2, 23),
            datetime(2024, 1, 10, 23),
            1,
            id="when time is within the range, return 1 row",
        ),
        pytest.param(
            datetime(2024, 1, 5, 23),
            datetime(2024, 1, 10, 23),
            0,
            id="when time is outside the range, return 0 rows",
        ),
    ],
)
def test_time_within_and_outside_of_date_range(
    spark: SparkSession,
    args_start_date: datetime,
    args_end_date: datetime,
    expected_rows: int,
) -> None:
    # Arrange
    time = datetime(2024, 1, 3, 23)

    df = create_wholesale(spark, create_wholesale_data_spec(time=time))
    mock_repository = Mock()
    mock_repository.read_wholesale_results.return_value = df

    # Act
    actual = read_and_filter_from_view(
        energy_supplier_ids=ENERGY_SUPPLIER_IDS,
        calculation_id_by_grid_area={
            default_data.DEFAULT_GRID_AREA_CODE: uuid.UUID(
                default_data.DEFAULT_CALCULATION_ID
            )
        },
        period_start=args_start_date,
        period_end=args_end_date,
        repository=mock_repository,
    )

    # Assert
    assert actual.count() == expected_rows


@pytest.mark.parametrize(
    "energy_supplier_ids, energy_supplier_id, expected_rows",
    [
        pytest.param(
            ["1234567890123"], "1234567890123", 1, id="time is within the range"
        ),
        pytest.param(
            ["2345678901234"],
            "1234567890123",
            0,
            id="time is outside the range (different energy_supplier_id)",
        ),
        pytest.param(
            None,
            "1234567890123",
            1,
            id="time is outside the range (None energy_supplier_ids)",
        ),
    ],
)
def test_when_time_is_within_and_outside_of_date_range(
    spark: SparkSession,
    energy_supplier_ids: list[str] | None,
    energy_supplier_id: str,
    expected_rows: int,
) -> None:
    # Arrange
    df = create_wholesale(
        spark, create_wholesale_data_spec(energy_supplier_id=energy_supplier_id)
    )
    mock_repository = Mock()
    mock_repository.read_wholesale_results.return_value = df

    # Act
    actual = read_and_filter_from_view(
        energy_supplier_ids=energy_supplier_ids,
        calculation_id_by_grid_area={
            default_data.DEFAULT_GRID_AREA_CODE: uuid.UUID(
                default_data.DEFAULT_CALCULATION_ID
            )
        },
        period_start=default_data.DEFAULT_FROM_DATE,
        period_end=default_data.DEFAULT_TO_DATE,
        repository=mock_repository,
    )

    # Assert
    assert actual.count() == expected_rows
