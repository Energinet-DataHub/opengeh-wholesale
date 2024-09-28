import uuid
from datetime import datetime
from unittest.mock import Mock

from settlement_report_job.domain.metering_point_resolution import (
    DataProductMeteringPointResolution,
)
from settlement_report_job.domain.time_series_factory import create_time_series


DEFAULT_PERIOD_START = datetime(2024, 4, 30, 22)
DEFAULT_PERIOD_END = datetime(2024, 5, 31, 2)
DEFAULT_TIME_ZONE = "Europe/Copenhagen"
DEFAULT_CALCULATION_BY_GRID_AREA = {
    "804": uuid.UUID("11111111-1111-1111-1111-111111111111"),
    "805": uuid.UUID("22222222-2222-2222-2222-222222222222"),
}


def test_create_time_series() -> None:
    # Arrange
    mock_repository = Mock()
    mock_repository.read_metering_point_time_series.return_value = sample_df
    resolution = DataProductMeteringPointResolution.HOUR

    # Act
    result_df = create_time_series(
        period_start=DEFAULT_PERIOD_START,
        period_end=DEFAULT_PERIOD_END,
        calculation_id_by_grid_area=DEFAULT_CALCULATION_BY_GRID_AREA,
        resolution=resolution,
        time_zone=DEFAULT_TIME_ZONE,
        repository=mock_repository,
    )

    # Assert
    assert result_df is not None
    assert result_df.count() > 0
    # Add more assertions as needed to verify the correctness of the result_df
